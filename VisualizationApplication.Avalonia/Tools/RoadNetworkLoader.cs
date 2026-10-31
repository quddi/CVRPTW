using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CVRPTW;
using VisualizationApplication.Avalonia.Other;

namespace VisualizationApplication.Avalonia.Tools;

public static class RoadNetworkLoader
{
    private static readonly string CacheDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CVRPTW");

    // Дорожня мережа, завантажена останньою; null, доки користувач не завантажив OSM.
    public static RoadNetwork? Current { get; private set; }

    public static async Task LoadAsync(Window owner)
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Оберіть файл OSM (.osm.pbf)",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("OSM (*.pbf)") { Patterns = ["*.pbf"] }]
        });

        if (files.Count == 0) return;

        var path = files[0].TryGetLocalPath();

        if (path == null)
        {
            await MessageBox.Show(owner, "Файл має бути на локальному диску.");
            return;
        }

        var network = await VisualizationExtensions.StartAsProgress(owner, "Завантаження OSM...",
            progress => RoadNetworkCache.LoadOrBuild(path, CacheDirectory, progress));

        if (network == null) return;

        Current = network;

        await MessageBox.Show(owner,
            $"Завантажено: {files[0].Name}\n" +
            $"Вершин: {network.NodeCount}, напрямлених ребер: {network.EdgeCount}\n" +
            $"Довжина доріг: {network.EdgeLength.Sum(length => (double)length) / 1000:F1} км\n" +
            $"Заборон поворотів: {network.ForbiddenTurns.Length} (обмежень застосовано: {network.RestrictionsApplied}, " +
            $"не підтримується: {network.RestrictionsUnsupported})");
    }
}
