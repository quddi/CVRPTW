using System.Windows;
using CVRPTW;
using VisualizationApplication.Other;
using VisualizationApplication.Tools;

namespace VisualizationApplication;

public class MainWindowReactionHandler : IDisposable
{
    private readonly MainWindowResetHandler _resetHandler;
    private readonly MainWindowElements _mainWindowElements;
    private MainData? _mainData;
    private MainResult? _mainResult;

    public MainWindowReactionHandler(MainWindowElements mainWindowElements)
    {
        _mainWindowElements = mainWindowElements;
        _resetHandler = new MainWindowResetHandler(mainWindowElements);
        
        FollowUiEvents();
        _resetHandler.ResetAll();
    }

    private void SetPoints()
    {
        if (_mainData is null)
        {
            MessageBox.Show("Дані не завантажено!");
            return;
        }

        var points = _mainData.PointsByIds.Values.ToList();
        var xs = points.Select(point => point.Coordinates.Latitude).ToArray();
        var ys = points.Select(point => point.Coordinates.Longitude).ToArray();
        
        _mainWindowElements.FilterPlot.Plot.Add.Scatter(xs, ys).LineWidth = 0;
        _mainWindowElements.FilterPlot.Refresh();
        Console.WriteLine(_mainWindowElements.FilterPlot.Plot.GetPlottables().Count());
    }

    private void LoadDataButtonClickHandler(object _, RoutedEventArgs __)
    {
        _resetHandler.ResetAll();
        _mainData = DataLoader.LoadData();
        Task.Run(SetPoints);
    }

    private void FollowUiEvents()
    {
        _mainWindowElements.LoadDataButton.Click += LoadDataButtonClickHandler;
    }
    
    private void UnfollowUiEvents()
    {
        _mainWindowElements.LoadDataButton.Click -= LoadDataButtonClickHandler;
    }

    public void Dispose()
    {
        UnfollowUiEvents();
    }
}