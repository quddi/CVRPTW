using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ScottPlot;
using SkiaSharp;

namespace VisualizationApplication.Avalonia.Map;

/// <summary>
/// Підкладка з растрових плиток (схема z/x/y, веб-меркатор). Плитки завантажуються у фоні, кешуються на диску
/// і в пам'яті; доки плитка не завантажена, показується більш груба плитка з пам'яті.
/// </summary>
public class TileLayer : IPlottable, IDisposable
{
    // Публічні плитки OSM: потрібні атрибуція й власний User-Agent, не більше двох одночасних завантажень.
    private const string UrlTemplate = "https://tile.openstreetmap.org/{0}/{1}/{2}.png";
    private const double WorldSize = 40075016.685578488;
    private const double Origin = WorldSize / 2;
    private const int TileSize = 256;
    private const int MaxZoom = 19;
    private const int MaxMemoryTiles = 400;
    private const int MaxTilesPerFrame = 100;
    private const int MaxFallbackLevels = 4;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(60);

    private static readonly HttpClient Http = CreateClient();

    private readonly string _cacheDirectory;
    private readonly ConcurrentDictionary<TileKey, SKImage> _memory = new();
    private readonly ConcurrentQueue<TileKey> _memoryOrder = new();
    private readonly ConcurrentDictionary<TileKey, byte> _pending = new();
    private readonly ConcurrentDictionary<TileKey, DateTime> _failed = new();
    private readonly SemaphoreSlim _downloads = new(2);
    private readonly CancellationTokenSource _cancellation = new();
    private volatile HashSet<TileKey> _wanted = new();

    private readonly record struct TileKey(int Zoom, int X, int Y);

    public TileLayer(string cacheDirectory)
    {
        _cacheDirectory = cacheDirectory;
    }

    public bool IsVisible { get; set; } = true;

    public IAxes Axes { get; set; } = new Axes();

    public IEnumerable<LegendItem> LegendItems => LegendItem.None;

    // Масштаб екрана (Retina = 2): плитка займає 256 незалежних пікселів, а не 256 фізичних.
    public Func<double>? ScaleProvider { get; set; }

    // Викликається з фонового потоку, коли з'явилась нова плитка й карту треба перемалювати.
    public event Action? TilesChanged;

    public AxisLimits GetAxisLimits() => AxisLimits.NoLimits;

    public void Render(RenderPack rp)
    {
        if (rp.DataRect.Width <= 0) return;

        var scale = ScaleProvider?.Invoke() ?? 1;
        var metersPerPixel = (Axes.XAxis.Max - Axes.XAxis.Min) / rp.DataRect.Width;
        var zoom = Math.Clamp((int)Math.Round(Math.Log2(WorldSize / (TileSize * Math.Max(scale, 1) * metersPerPixel))), 0, MaxZoom);

        var count = 1 << zoom;
        var tileMeters = WorldSize / count;
        var fromX = Math.Max(0, (int)Math.Floor((Axes.XAxis.Min + Origin) / tileMeters));
        var toX = Math.Min(count - 1, (int)Math.Floor((Axes.XAxis.Max + Origin) / tileMeters));
        var fromY = Math.Max(0, (int)Math.Floor((Origin - Axes.YAxis.Max) / tileMeters));
        var toY = Math.Min(count - 1, (int)Math.Floor((Origin - Axes.YAxis.Min) / tileMeters));

        if ((toX - fromX + 1) * (toY - fromY + 1) > MaxTilesPerFrame) return;

        // Спершу фіксуємо потрібні плитки, щоб завантаження не відкидало щойно запитані.
        var wanted = new HashSet<TileKey>();

        for (var x = fromX; x <= toX; x++)
        for (var y = fromY; y <= toY; y++)
            wanted.Add(new TileKey(zoom, x, y));

        _wanted = wanted;

        using var paint = new SKPaint { IsAntialias = false };
        var sampling = new SKSamplingOptions(SKFilterMode.Linear);

        foreach (var key in wanted)
        {
            var (x, y) = (key.X, key.Y);
            var left = x * tileMeters - Origin;
            var top = Origin - y * tileMeters;
            var topLeft = Axes.GetPixel(new Coordinates(left, top));
            var bottomRight = Axes.GetPixel(new Coordinates(left + tileMeters, top - tileMeters));

            // Невелике перекриття прибирає шви між плитками.
            var destination = new SKRect(topLeft.X - 0.5f, topLeft.Y - 0.5f, bottomRight.X + 0.5f, bottomRight.Y + 0.5f);

            if (_memory.TryGetValue(key, out var image))
            {
                rp.Canvas.DrawImage(image, new SKRect(0, 0, image.Width, image.Height), destination, sampling, paint);
                continue;
            }

            Request(key);
            DrawFallback(rp.Canvas, key, destination, sampling, paint);
        }
    }

    // Поки плитки немає, показуємо відповідну частину грубішої плитки.
    private void DrawFallback(SKCanvas canvas, TileKey key, SKRect destination, SKSamplingOptions sampling, SKPaint paint)
    {
        for (var level = 1; level <= MaxFallbackLevels && key.Zoom - level >= 0; level++)
        {
            var parent = new TileKey(key.Zoom - level, key.X >> level, key.Y >> level);

            if (!_memory.TryGetValue(parent, out var image)) continue;

            var part = image.Width / (float)(1 << level);
            var sourceX = (key.X - (parent.X << level)) * part;
            var sourceY = (key.Y - (parent.Y << level)) * part;

            canvas.DrawImage(image, new SKRect(sourceX, sourceY, sourceX + part, sourceY + part), destination, sampling, paint);

            return;
        }
    }

    private void Request(TileKey key)
    {
        if (_failed.TryGetValue(key, out var failedAt) && DateTime.UtcNow - failedAt < RetryDelay) return;

        if (!_pending.TryAdd(key, 0)) return;

        Task.Run(() => LoadAsync(key));
    }

    private async Task LoadAsync(TileKey key)
    {
        try
        {
            var path = Path.Combine(_cacheDirectory, key.Zoom.ToString(), key.X.ToString(), key.Y + ".png");
            byte[]? bytes = File.Exists(path) ? await File.ReadAllBytesAsync(path, _cancellation.Token) : null;

            if (bytes == null)
            {
                await _downloads.WaitAsync(_cancellation.Token);

                try
                {
                    // Плитка, яку користувач уже проскролив, не потрібна.
                    if (!_wanted.Contains(key)) return;

                    bytes = await Http.GetByteArrayAsync(string.Format(UrlTemplate, key.Zoom, key.X, key.Y), _cancellation.Token);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    var temporaryPath = path + ".part";
                    await File.WriteAllBytesAsync(temporaryPath, bytes, _cancellation.Token);
                    File.Move(temporaryPath, path, overwrite: true);
                }
                finally
                {
                    _downloads.Release();
                }
            }

            var image = SKImage.FromEncodedData(bytes);
            if (image == null) return;

            _memory[key] = image;
            _memoryOrder.Enqueue(key);

            while (_memory.Count > MaxMemoryTiles && _memoryOrder.TryDequeue(out var oldest))
                _memory.TryRemove(oldest, out _);

            _failed.TryRemove(key, out _);
            TilesChanged?.Invoke();
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or OperationCanceledException or ObjectDisposedException)
        {
            _failed[key] = DateTime.UtcNow;
        }
        finally
        {
            _pending.TryRemove(key, out _);
        }
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("CVRPTW-dispatcher/1.0");

        return client;
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
    }
}
