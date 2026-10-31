using System;
using Avalonia.Controls;
using VisualizationApplication.Avalonia.Other;
using VisualizationApplication.Avalonia.Tools;

namespace VisualizationApplication.Avalonia;

public partial class MainWindow : Window, IDisposable
{
    private readonly MainWindowElements _elements;
    private readonly MainWindowReactionHandler _reactionHandler;

    public MainWindow()
    {
        InitializeComponent();

        _elements = new MainWindowElements(
            LoadDataButton, 
            MainPlot, 
            OptimizeButton, 
            VisualizationComboBox, 
            PathCostTextBox, 
            OptimizationMethodComboBox);

        _reactionHandler = new MainWindowReactionHandler(_elements, this);

        _reactionHandler.FileLoaded += FileLoadedHandler;

        ScenarioButton.Click += (_, _) => new ScenarioWindow().Show(this);
        LoadOsmButton.Click += async (_, _) => await RoadNetworkLoader.LoadAsync(this);

        Closed += (_, _) => Dispose();
    }

    private void FileLoadedHandler(string fileName)
    {
        Title = fileName;
    }

    public void Dispose()
    {
        _reactionHandler.FileLoaded -= FileLoadedHandler;
        _reactionHandler.Dispose();
    }
}