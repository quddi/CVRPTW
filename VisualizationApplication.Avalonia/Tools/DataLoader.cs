using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CVRPTW;

namespace VisualizationApplication.Avalonia.Tools;

public static class DataLoader
{
    private static readonly MainParser Parser = new();
    
    public static async Task<(MainData? mainData, string fileName)> LoadDataAsync(Visual visual)
    {
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null) return default;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Оберіть файл даних",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Всі файли (*.*)") { Patterns = ["*.*"] },
                new FilePickerFileType("Текстові файли (*.txt)") { Patterns = ["*.txt"] }
            ]
        });

        if (files.Count == 0) return default;

        var file = files[0];
        await using var stream = await file.OpenReadAsync();
        using var streamReader = new StreamReader(stream);
                
        return (Parser.Parse(streamReader), file.Name);
    }
}
