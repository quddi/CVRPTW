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

    // work отримує IProgress<string>, повідомлення якого показуються у вікні прогресу.
    public static async Task<T?> StartAsProgress<T>(Window? owner, string message, Func<IProgress<string>, T> work)
    {
        var progressWindow = new ProgressWindow
        {
            Title = message,
            WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
        };

        progressWindow.SetMessage(message);

        try
        {
            if (owner != null)
                progressWindow.Show(owner);
            else
                progressWindow.Show();

            var progress = new Progress<string>(progressWindow.SetMessage);

            return await Task.Run(() => work(progress));
        }
        catch (Exception ex)
        {
            await MessageBox.Show(owner, $"Error: {ex.Message}");
            return default;
        }
        finally
        {
            progressWindow.Close();
        }
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
