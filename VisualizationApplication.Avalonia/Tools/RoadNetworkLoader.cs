using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CVRPTW;
using VisualizationApplication.Avalonia.Other;

namespace VisualizationApplication.Avalonia.Tools;

public static class RoadNetworkLoader
{
    private const string OsmUrl = "https://download.geofabrik.de/europe/ukraine-latest.osm.pbf";
    private const string OsmFileName = "ukraine-latest.osm.pbf";
    private const double BytesInMegabyte = 1024 * 1024;

    private static readonly string DataDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CVRPTW");

    // Дорожня мережа, завантажена останньою; null, доки користувач не завантажив OSM.
    public static RoadNetwork? Current { get; private set; }

    public static RoadSpatialIndex? CurrentIndex { get; private set; }

    public static event Action? Changed;

    /// <summary>
    /// Завантажує свіжий файл OSM України з інтернету поверх наявного й будує дорожню мережу.
    /// Якщо завантажити не вдалося, а старий файл є, використовується він.
    /// </summary>
    public static async Task LoadAsync(Window owner)
    {
        var path = Path.Combine(DataDirectory, "osm", OsmFileName);

        var loaded = await VisualizationExtensions.StartAsProgress(owner, "Завантаження OSM...", async (messages, fractions) =>
        {
            string? downloadError = null;

            try
            {
                await OsmDownloader.DownloadAsync(OsmUrl, path, (received, total) =>
                {
                    messages.Report(total == null
                        ? $"Завантаження OSM: {received / BytesInMegabyte:F0} МБ"
                        : $"Завантаження OSM: {received / BytesInMegabyte:F0} / {total / BytesInMegabyte:F0} МБ");
                    fractions.Report(total == null ? null : (double)received / total.Value);
                });
            }
            catch (Exception exception) when (File.Exists(path))
            {
                downloadError = exception.Message;
            }

            fractions.Report(null);

            var road = RoadNetworkCache.LoadOrBuild(path, DataDirectory, messages);
            messages.Report("Побудова індексу доріг...");

            return (Network: road, Index: new RoadSpatialIndex(road), DownloadError: downloadError);
        });

        if (loaded.Network == null) return;

        var network = loaded.Network;
        Current = network;
        CurrentIndex = loaded.Index;
        Changed?.Invoke();

        await MessageBox.Show(owner,
            (loaded.DownloadError == null ? "Файл OSM оновлено.\n" : $"Не вдалося оновити файл ({loaded.DownloadError}), використано наявний.\n") +
            $"Вершин: {network.NodeCount}, напрямлених ребер: {network.EdgeCount}\n" +
            $"Довжина доріг: {network.EdgeLength.Sum(length => (double)length) / 1000:F1} км\n" +
            $"Заборон поворотів: {network.ForbiddenTurns.Length} (обмежень застосовано: {network.RestrictionsApplied}, " +
            $"не підтримується: {network.RestrictionsUnsupported})");
    }
}
