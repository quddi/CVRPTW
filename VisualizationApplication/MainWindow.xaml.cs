using System.Windows;
using CVRPTW.Computing;
using VisualizationApplication.Other;

namespace VisualizationApplication;

public partial class MainWindow : Window
{
    private readonly MainWindowElements _elements;
    private readonly MainWindowReactionHandler _reactionHandler;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _elements = new MainWindowElements(LoadDataButton, MainPlot, ComputeButton, VisualizationComboBox, 
            PathCostLabel, OptimizationMethodComboBox); 
        
        _reactionHandler = new MainWindowReactionHandler(_elements);
    }
}