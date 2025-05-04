using System.IO;
using System.Windows;
using CVRPTW;
using Microsoft.Win32;

namespace VisualizationApplication.Tools;

public static class DataLoader
{
    private static readonly MainParser Parser = new();
    
    public static (MainData? mainData, string fileName) LoadData()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Все файлы (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != true) return default;
        
        var filePath = openFileDialog.FileName;

        using var streamReader = new StreamReader(filePath);
                
        return (Parser.Parse(streamReader), openFileDialog.SafeFileName);
    }
}