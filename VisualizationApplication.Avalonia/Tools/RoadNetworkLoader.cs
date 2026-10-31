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

    private static readonly string OsmPath = Path.Combine(DataDirectory, "osm", OsmFileName);

    // Чи є на диску раніше завантажений файл OSM.
    public static bool IsOsmFilePresent => File.Exists(OsmPath);

    // Дорожня мережа, завантажена останньою; null, доки користувач не завантажив OSM.
    public static RoadNetwork? Current { get; private set; }

    // Індекси від повного до грубого (лише траси); [0] придатний для пошуку найближчої дороги.
    public static RoadSpatialIndex[] CurrentIndexLevels { get; private set; } = [];

    public static event Action? Changed;

    /// <summary>
    /// Будує дорожню мережу з файлу OSM України. Із download = true спершу завантажує свіжий файл з інтернету
    /// поверх наявного; якщо завантажити не вдалося, а старий файл є, використовується він.
    /// З download = false використовується лише наявний файл (кеш графа, якщо файл не змінився).
    /// </summary>
    public static async Task LoadAsync(Window owner, bool download = true)
    {
        var path = OsmPath;

        var loaded = await VisualizationExtensions.StartAsProgress(owner, "Завантаження OSM...", async (messages, fractions) =>
        {
            string? downloadError = null;

            try
            {
                if (download) await OsmDownloader.DownloadAsync(OsmUrl, path, (received, total) =>
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

            RoadSpatialIndex[] levels =
            [
                new RoadSpatialIndex(road),
                new RoadSpatialIndex(road, maxRank: 1, cellSize: 500_000),
                new RoadSpatialIndex(road, maxRank: 0, cellSize: 1_000_000)
            ];

            return (Network: road, Levels: levels, DownloadError: downloadError);
        });

        if (loaded.Network == null) return;

        var network = loaded.Network;
        Current = network;
        CurrentIndexLevels = loaded.Levels;
        Changed?.Invoke();

        if (!download) return;

        await MessageBox.Show(owner,
            (loaded.DownloadError == null ? "Файл OSM оновлено.\n" : $"Не вдалося оновити файл ({loaded.DownloadError}), використано наявний.\n") +
            $"Вершин: {network.NodeCount}, напрямлених ребер: {network.EdgeCount}\n" +
            $"Довжина доріг: {network.EdgeLength.Sum(length => (double)length) / 1000:F1} км\n" +
            $"Заборон поворотів: {network.ForbiddenTurns.Length} (обмежень застосовано: {network.RestrictionsApplied}, " +
            $"не підтримується: {network.RestrictionsUnsupported})");
    }
}
