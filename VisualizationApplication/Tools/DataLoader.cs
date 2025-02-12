using System.IO;
using System.Windows;
using CVRPTW;
using Microsoft.Win32;

namespace VisualizationApplication.Tools;

public static class DataLoader
{
    private static readonly MainParser Parser = new();
    
    public static MainData? LoadData()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Все файлы (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != true) return null;
        
        var filePath = openFileDialog.FileName;

        try
        {
            using var streamReader = new StreamReader(filePath);
                
            return Parser.Parse(streamReader);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка при читанні файлу: {ex.Message}");
            
            throw;
        }
    }
}