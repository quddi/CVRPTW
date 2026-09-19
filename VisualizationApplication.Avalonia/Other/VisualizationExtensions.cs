using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using VisualizationApplication.Avalonia.Tools;

namespace VisualizationApplication.Avalonia.Other;

public static class VisualizationExtensions
{
    public static ScottPlot.Coordinates ToScottCoordinates(this CVRPTW.Coordinates coordinates)
    {
        return new ScottPlot.Coordinates(coordinates.Latitude, coordinates.Longitude);
    }

    public static async Task StartAsProgress(Window? owner, Action action)
    {
        var progressWindow = new ProgressWindow
        {
            WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
        };
            
        try
        {
            if (owner != null)
                progressWindow.Show(owner);
            else
                progressWindow.Show();
            
            await Task.Run(action);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(owner, $"Error: {ex.Message}");
        }
        finally
        {
            progressWindow.Close();
        }
    }
}
