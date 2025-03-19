using System.Windows;

namespace VisualizationApplication.Other;

public static class VisualizationExtensions
{
    public static ScottPlot.Coordinates ToScottCoordinates(this CVRPTW.Coordinates coordinates)
    {
        return new ScottPlot.Coordinates(coordinates.Latitude, coordinates.Longitude);
    }

    public static async Task StartAsProgress(Action action)
    {
        var progressWindow = new ProgressWindow { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            
        try
        {
            progressWindow.Show();
            
            await Task.Run(action);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
        finally
        {
            progressWindow.Close();
        }
    }
}