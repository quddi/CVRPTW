using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CVRPTW;
using CVRPTW.Computing;
using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Estimators.Time;
using CVRPTW.Computing.Optimizers;
using ScottPlot;
using VisualizationApplication.Avalonia.Other;
using VisualizationApplication.Avalonia.Tools;

// ReSharper disable CoVariantArrayConversion

namespace VisualizationApplication.Avalonia;

public class MainWindowReactionHandler : IDisposable
{
    private readonly MainWindowResetHandler _resetHandler;
    private readonly MainWindowElements _mainWindowElements;
    private readonly Window _ownerWindow;
    private MainData? _mainData;
    private MainResult? _mainResult;
    private MainComputer? _startMainComputer;
    private IMainResultEstimator? _mainResultEstimator;

    private Dictionary<CarResult, Color>? _resultColors;
    private MainResultOptimizer[]? _optimizers;

    public event Action<string>? FileLoaded;

    // Поточні дані й результат (після оптимізації вони оновлюються в тих самих об'єктах).
    public MainData? CurrentData => _mainData;

    public MainResult? CurrentResult => _mainResult;
    
    public MainWindowReactionHandler(MainWindowElements mainWindowElements, Window ownerWindow)
    {
        _mainWindowElements = mainWindowElements;
        _ownerWindow = ownerWindow;
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
        
            var circle = _mainWindowElements.FilterPlot.Plot.Add.Marker(depoCoordinates);
        
            circle.Color = VisualizationConstants.DepoPointFillColor;
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

    private async void LoadDataButtonClickHandler(object? sender, RoutedEventArgs e)
    {
        _resetHandler.ResetAll(_mainResult, _optimizers);
        var (mainData, fileName) = await DataLoader.LoadDataAsync(_mainWindowElements.LoadDataButton);

        if (mainData == null)
        {
            await MessageBox.Show(_ownerWindow, "Файл не було завантажено!");
            return;
        }

        LoadMainData(mainData, fileName);
    }

    // Джерело даних — файл вводу або сценарій з карти: далі розрахунок однаковий.
    public void LoadMainData(MainData mainData, string title)
    {
        _resetHandler.ResetAll(_mainResult, _optimizers);

        _mainData = mainData;
        
        SetUpFunctionality();
        SetPoints();
        
        _resetHandler.ResetOptimizationComboBox(_optimizers);
        
        _mainResult = _startMainComputer!.Compute();
        
        _resultColors = _mainResult!.Results.Values
            .ToDictionary(carResult => carResult, _ => Color.RandomHue());
        
        _resetHandler.ResetAll(_mainResult, _optimizers);

        _mainWindowElements.VisualizationComboBox.SelectedIndex = Constants.OnlyPointsIndex;
        
        FileLoaded?.Invoke(title);
    }

    private void SetUpFunctionality()
    {
        var pathEstimator = new ByDistanceMainResultEstimator(_mainData!);
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
            new InsertUnservedMainResultOptimizer(_mainResultEstimator, _mainData!) { Name = "Вставка необслужених"},
            new PointTransposeMainResultOptimizer(_mainResultEstimator, _mainData!) { Name = "Перекидування точок"},
            new TabuSearchMainResultOptimizer(_mainResultEstimator, _mainData!) { Name = "Табу пошук"},
            new DeterministicAnnealingOptimizer(_mainResultEstimator, _mainData!, VisualizationConstants.DeterministicAnnealingConfig) { Name = "Deterministic Annealing" },
            ExtensionsMethods.GetBaseOptimizer(_mainResultEstimator, _mainData!).WithName("Базовий"),
            ExtensionsMethods.GetAlternativeOptimizer(_mainResultEstimator, _mainData!).WithName("Альтернативний"),
            ExtensionsMethods.GetBaseAdvancedOptimizer(_mainResultEstimator, _mainData!).WithName("Базовий покращений"),
            ExtensionsMethods.GetAlternativeAdvancedOptimizer(_mainResultEstimator, _mainData!).WithName("Альтернативний покращений"),
        ];
    }

    private async void OptimizeButtonClickHandler(object? sender, RoutedEventArgs e)
    {
        if (_optimizers == null || _mainResult == null || _mainResultEstimator == null) return;

        if (_mainWindowElements.OptimizationComboBox.SelectedIndex == Constants.NotSelectedIndex)
        {
            await MessageBox.Show(_ownerWindow, "Оберіть оптимізатор!");
            return;
        }

        var selectedOptimizer = _optimizers[_mainWindowElements.OptimizationComboBox.SelectedIndex];

        if (selectedOptimizer.GetType() == typeof(AlternativePointsMainResultOptimizer) &&
            _mainData!.AlternativePoints.AreEmpty())
        {
            await MessageBox.Show(_ownerWindow, "У вхідних даних відсутня інформація про альтернативні точки!");
            return;
        }

        await VisualizationExtensions.StartAsProgress(_ownerWindow, () => selectedOptimizer.Optimize(_mainResult));
        
        _resetHandler.ResetAll(_mainResult, _optimizers);
    }

    private void VisualizationComboBoxSelectionChangedHandler(object? sender, SelectionChangedEventArgs e)
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
                _mainWindowElements.PathCostTextBox.Text = $"Загальна вартість шляхів: {_mainResult!.Estimation.ToFormattedString()}" +
                                                           (_mainResult.UnreachableEdgesCount > 0 ? $"\nНедосяжних переходів: {_mainResult.UnreachableEdgesCount} (розв'язок недопустимий)" : string.Empty) +
                                                           GetUnservedText();
                break;
            
            case not Constants.NotSelectedIndex:
                _resetHandler.ResetMainPlot();
                SetPoints();
                var (chosenCar, chosenResult) = _mainResult!.Results.ElementAt(comboBoxSelectionIndex - Constants.ServiceIndexesCount);
                SetResult(chosenResult);
                _mainWindowElements.PathCostTextBox.Text = $"Вартість шляху: {chosenResult.Estimation.ToFormattedString()}. Вага {_mainData!.GetLoad(chosenResult).ToFormattedString()}/{chosenCar.Capacity}" +
                                                           $"\nШлях ({chosenResult.Path.Count} точок): {chosenResult.Path.ToText()}" +
                                                           (chosenResult.UnreachableEdgesCount > 0 ? $"\nНедосяжних переходів: {chosenResult.UnreachableEdgesCount} (маршрут недопустимий)" : string.Empty);
                break;
        }
    }

    // Клієнти, яких немає в жодному маршруті (не вмістились у авто або недосяжні).
    private string GetUnservedText()
    {
        var unserved = _mainData!.CountUnserved(_mainResult!);

        return unserved > 0 ? $"\nНе обслужено клієнтів: {unserved} (спробуйте оптимізатор «Вставка необслужених»)" : string.Empty;
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
