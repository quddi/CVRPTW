using System.Windows;
using CVRPTW;
using ScottPlot.Plottables;
using VisualizationApplication.Other;
using VisualizationApplication.Tools;
using Point = System.Windows.Point;

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
        if (_mainData == null)
        {
            MessageBox.Show("Дані не завантажено!");
            return;
        }

        var points = _mainData.PointsByIds.Values.ToList();
        var xs = points.Select(point => point.Coordinates.Latitude).ToArray();
        var ys = points.Select(point => point.Coordinates.Longitude).ToArray();

        _mainWindowElements.FilterPlot.Plot.Add.Scatter(xs, ys).LineWidth = 0;
        _mainWindowElements.FilterPlot.Refresh();
    }

    private void LoadDataButtonClickHandler(object _, RoutedEventArgs __)
    {
        _resetHandler.ResetAll();
        _mainData = DataLoader.LoadData();
        SetPoints();
    }

    private void OptimizeButtonClickHandler(object _, RoutedEventArgs __)
    {
        
    }

    private void FollowUiEvents()
    {
        _mainWindowElements.LoadDataButton.Click += LoadDataButtonClickHandler;
        _mainWindowElements.OptimizeButton.Click += OptimizeButtonClickHandler;
    }

    private void UnfollowUiEvents()
    {
        _mainWindowElements.LoadDataButton.Click -= LoadDataButtonClickHandler;
        _mainWindowElements.OptimizeButton.Click -= OptimizeButtonClickHandler;
    }

    public void Dispose()
    {
        UnfollowUiEvents();
    }
}