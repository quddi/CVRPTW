using System.Windows;
using System.Windows.Controls;
using CVRPTW;
using CVRPTW.Computing;
using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Estimators.Time;
using CVRPTW.Computing.Optimizers;
using ScottPlot;
using VisualizationApplication.Other;
using VisualizationApplication.Tools;

// ReSharper disable CoVariantArrayConversion

namespace VisualizationApplication;

public class MainWindowReactionHandler : IDisposable
{
    private readonly MainWindowResetHandler _resetHandler;
    private readonly MainWindowElements _mainWindowElements;
    private MainData? _mainData;
    private MainResult? _mainResult;
    private MainComputer? _startMainComputer;
    private IMainResultEstimator? _mainResultEstimator;

    private Dictionary<CarResult, Color>? _resultColors;
    private MainResultOptimizer[]? _optimizers;

    public event Action<string>? FileLoaded;
    
    public MainWindowReactionHandler(MainWindowElements mainWindowElements)
    {
        _mainWindowElements = mainWindowElements;
        _resetHandler = new MainWindowResetHandler(mainWindowElements);
        
        FollowUiEvents();
        
        _resetHandler.ResetAll(_mainResult, _optimizers);
    }

    private void SetPoints()
    {
        if (_mainData == null) return;

        var points = _mainData.PointsByIds.Values.ToList();
        var xs = points.Select(point => point.Coordinates.Latitude).ToArray();
        var ys = points.Select(point => point.Coordinates.Longitude).ToArray();

        AddDepoPoints();
        
        _mainWindowElements.FilterPlot.Plot.Add.Scatter(xs, ys, VisualizationConstants.DefaultPointsColor).LineWidth = 0;

        _mainWindowElements.FilterPlot.Refresh();
        _mainWindowElements.FilterPlot.Plot.Axes.AutoScale();
    }

    private void AddDepoPoints()
    {
        foreach (var depoPoint in _mainData!.DepoPointsByIds.Values)
        {
            var depoCoordinates = depoPoint!.Coordinates.ToScottCoordinates();
        
            var circle = _mainWindowElements.FilterPlot.Plot.Add.Ellipse(depoCoordinates, 
                VisualizationConstants.DepoPointRadiusX, VisualizationConstants.DepoPointRadiusY);
        
            circle.FillColor = VisualizationConstants.DepoPointFillColor;
            circle.LineColor = VisualizationConstants.DepoPointLineColor;
        }
    }

    private void SetAllResults()
    {
        foreach (var (_, result) in _mainResult!.Results)
        {
            SetResult(result, addDepoPoint: false);
        }
        
        AddDepoPoints();
        
        _mainWindowElements.FilterPlot.Refresh();
    }

    private void SetResult(CarResult carResult, bool addDepoPoint = true)
    {
        var color = _resultColors![carResult];

        var points = carResult.Path.Select(pointVisitResult => _mainData!.GetPoint(pointVisitResult.Id)).ToList();
        var xs = points.Select(point => point.Coordinates.Latitude).ToArray();
        var ys = points.Select(point => point.Coordinates.Longitude).ToArray();

        if (addDepoPoint) AddDepoPoints();
        
        _mainWindowElements.FilterPlot.Plot.Add.Scatter(xs, ys, color);
        _mainWindowElements.FilterPlot.Refresh();
    }

    private void LoadDataButtonClickHandler(object _, RoutedEventArgs __)
    {
        _resetHandler.ResetAll(_mainResult, _optimizers);
        (_mainData, var fileName) = DataLoader.LoadData();

        if (_mainData == null)
        {
            MessageBox.Show("Файл не було завантажено!");
            return;
        }
        
        SetUpFunctionality();
        SetPoints();
        
        _resetHandler.ResetOptimizationComboBox(_optimizers);
        
        _mainResult = _startMainComputer!.Compute();
        
        _resultColors = _mainResult!.Results.Values
            .ToDictionary(carResult => carResult, _ => Color.RandomHue());
        
        _resetHandler.ResetAll(_mainResult, _optimizers);

        _mainWindowElements.VisualizationComboBox.SelectedIndex = Constants.OnlyPointsIndex;
        
        FileLoaded?.Invoke(fileName);
    }

    private void SetUpFunctionality()
    {
        var pathEstimator = new DistancePathCostEstimator(_mainData!);
        var timeEstimator = new SimpleTimeEstimator(_mainData!);
        _mainResultEstimator = new ComplexMainResultEstimator(_mainData!, pathEstimator, timeEstimator);
        
        _startMainComputer = new DistanceMainComputer(_mainData!, _mainResultEstimator);
        
        _optimizers = 
        [
            new Opt2CarResultOptimizer(_mainResultEstimator!) { Name = "Opt 2"},
            new Opt3CarResultOptimizer(_mainResultEstimator!) { Name = "Opt 3" },
            new OrOptCarResultOptimizer(_mainResultEstimator!) { Name = "Or Opt" },
            new SwapCarResultOptimizer(_mainResultEstimator!) { Name = "Swap" },
            new AlternativePointsMainResultOptimizer(_mainResultEstimator, _mainData!) { Name = "Видалення альтернативних"},
            new PointTransposeMainResultOptimizer(_mainResultEstimator, _mainData!) { Name = "Перекидування точок"}
        ];
    }

    private async void OptimizeButtonClickHandler(object _, RoutedEventArgs __)
    {
        if (_optimizers == null || _mainResult == null || _mainResultEstimator == null) return;

        if (_mainWindowElements.OptimizationComboBox.SelectedIndex == Constants.NotSelectedIndex)
        {
            MessageBox.Show("Оберіть оптимізатор!");
            
            return;
        }

        var selectedOptimizer = _optimizers[_mainWindowElements.OptimizationComboBox.SelectedIndex];

        await VisualizationExtensions.StartAsProgress(() => selectedOptimizer.Optimize(_mainResult));
        
        _resetHandler.ResetAll(_mainResult, _optimizers);
    }

    private void VisualizationComboBoxSelectionChangedHandler(object _, SelectionChangedEventArgs __)
    {
        var comboBoxSelectionIndex = _mainWindowElements.VisualizationComboBox.SelectedIndex;

        switch (comboBoxSelectionIndex)
        {
            case Constants.OnlyPointsIndex:
                _resetHandler.ResetMainPlot();
                SetPoints();
                _mainWindowElements.PathCostTextBox.Text = string.Empty;
                break;

            case Constants.AllResultsIndex:
                _resetHandler.ResetMainPlot();
                SetAllResults();
                _mainWindowElements.PathCostTextBox.Text = $"Загальна вартість шляхів: {_mainResult!.Estimation}";
                break;
            
            case not Constants.NotSelectedIndex:
                _resetHandler.ResetMainPlot();
                SetPoints();
                var (chosenCar, chosenResult) = _mainResult!.Results.ElementAt(comboBoxSelectionIndex - Constants.ServiceIndexesCount);
                SetResult(chosenResult);
                _mainWindowElements.PathCostTextBox.Text = $"Вартість шляху: {chosenResult.Estimation}. Вага {(chosenCar.Capacity - chosenResult.RemainedFreeSpace).ToFormattedString()}/{chosenCar.Capacity}" +
                                                           $"\nШлях ({chosenResult.Path.Count} точок): {chosenResult.Path.ToText()}";
                break;
        }
    }

    private void FollowUiEvents()
    {
        _mainWindowElements.LoadDataButton.Click += LoadDataButtonClickHandler;
        _mainWindowElements.OptimizeButton.Click += OptimizeButtonClickHandler;
        _mainWindowElements.VisualizationComboBox.SelectionChanged += VisualizationComboBoxSelectionChangedHandler;
    }

    private void UnfollowUiEvents()
    {
        _mainWindowElements.LoadDataButton.Click -= LoadDataButtonClickHandler;
        _mainWindowElements.OptimizeButton.Click -= OptimizeButtonClickHandler;
        _mainWindowElements.VisualizationComboBox.SelectionChanged -= VisualizationComboBoxSelectionChangedHandler;
    }

    public void Dispose()
    {
        UnfollowUiEvents();
    }
}