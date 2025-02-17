using System.Windows;
using System.Windows.Controls;
using CVRPTW;
using CVRPTW.Computing;
using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Optimizers;
using ScottPlot;
using VisualizationApplication.Other;
using VisualizationApplication.Tools;

namespace VisualizationApplication;

public class MainWindowReactionHandler : IDisposable
{
    private readonly MainWindowResetHandler _resetHandler;
    private readonly MainWindowElements _mainWindowElements;
    private MainData? _mainData;
    private MainResult? _mainResult;
    private PathEstimator? _pathEstimator;
    private MainResultEstimator? _mainResultEstimator;
    private PathComputer? _pathComputer;
    
    private Dictionary<CarResult, Color>? _resultColors;

    private const int OnlyPointsIndex = 0;
    private const int AllResultsIndex = 1;
    private const int ServiceIndexesCount = 2;
    
    public MainWindowReactionHandler(MainWindowElements mainWindowElements)
    {
        _mainWindowElements = mainWindowElements;
        _resetHandler = new MainWindowResetHandler(mainWindowElements);
        
        FollowUiEvents();
        _resetHandler.ResetAll(_mainResult);
    }

    private void SetPoints()
    {
        if (_mainData == null) return;

        var points = _mainData.PointsByIds.Values.ToList();
        var xs = points.Select(point => point.Coordinates.Latitude).ToArray();
        var ys = points.Select(point => point.Coordinates.Longitude).ToArray();

        _mainWindowElements.FilterPlot.Plot.Add.Scatter(xs, ys).LineWidth = 0;
        _mainWindowElements.FilterPlot.Refresh();
        _mainWindowElements.FilterPlot.Plot.Axes.AutoScale();
    }

    private void SetAllResults()
    {
        foreach (var (_, result) in _mainResult!.Results)
        {
            SetResult(result);
        }
    }

    private void SetResult(CarResult carResult)
    {
        var color = _resultColors![carResult];

        var points = carResult.Path.Select(pointId => _mainData!.GetPoint(pointId)).ToList();
        var xs = points.Select(point => point.Coordinates.Latitude).ToArray();
        var ys = points.Select(point => point.Coordinates.Longitude).ToArray();

        _mainWindowElements.FilterPlot.Plot.Add.Scatter(xs, ys, color);
        _mainWindowElements.FilterPlot.Refresh();
    }

    private void LoadDataButtonClickHandler(object _, RoutedEventArgs __)
    {
        _resetHandler.ResetAll(_mainResult);
        _mainData = DataLoader.LoadData();

        _pathEstimator = new DistancePathEstimator(_mainData!);
        _mainResultEstimator = new SumMainResultEstimator(_mainData!, _pathEstimator);
        
        _pathComputer = new OptimizedPathComputer
        (
            new StartPathComputer(_mainData!, _mainResultEstimator),
            new CompositeMainResultOptimizer(
            [
                new PointTransposeMainResultOptimizer(_mainResultEstimator, _mainData!),
                new Opt3CarResultOptimizer(_pathEstimator),
            ], 
            report: false),
            _mainData!
        );
        SetPoints();
    }

    private void ComputeButtonClickHandler(object _, RoutedEventArgs __)
    {
        _mainResult = _pathComputer!.Compute();
        
        _resultColors = _mainResult!.Results.Values
            .ToDictionary(carResult => carResult, _ => Color.RandomHue());
        
        _resetHandler.ResetAll(_mainResult);

        _mainWindowElements.VisualizationComboBox.SelectedIndex = AllResultsIndex;
    }

    private void VisualizationComboBoxSelectionChangedHandler(object _, SelectionChangedEventArgs __)
    {
        var comboBoxSelectionIndex = _mainWindowElements.VisualizationComboBox.SelectedIndex;

        switch (comboBoxSelectionIndex)
        {
            case OnlyPointsIndex:
                _resetHandler.ResetMainPlot();
                SetPoints();
                _mainWindowElements.PathCostLabel.Content = string.Empty;
                break;

            case AllResultsIndex:
                _resetHandler.ResetMainPlot();
                SetAllResults();
                _mainWindowElements.PathCostLabel.Content = $"Загальна вартість шляхів: {_mainResult!.Estimation}";
                break;
            
            case not Constants.NotSelectedIndex:
                _resetHandler.ResetMainPlot();
                SetPoints();
                var chosenResult = _mainResult!.Results.Values.ElementAt(comboBoxSelectionIndex - ServiceIndexesCount);
                SetResult(chosenResult);
                _mainWindowElements.PathCostLabel.Content = $"Вартість шляху: {chosenResult.PathCost}";
                break;
        }
    }

    private void FollowUiEvents()
    {
        _mainWindowElements.LoadDataButton.Click += LoadDataButtonClickHandler;
        _mainWindowElements.ComputeButton.Click += ComputeButtonClickHandler;
        _mainWindowElements.VisualizationComboBox.SelectionChanged += VisualizationComboBoxSelectionChangedHandler;
    }

    private void UnfollowUiEvents()
    {
        _mainWindowElements.LoadDataButton.Click -= LoadDataButtonClickHandler;
        _mainWindowElements.ComputeButton.Click -= ComputeButtonClickHandler;
        _mainWindowElements.VisualizationComboBox.SelectionChanged -= VisualizationComboBoxSelectionChangedHandler;
    }

    public void Dispose()
    {
        UnfollowUiEvents();
    }
}