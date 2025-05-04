using System.Windows;
using CVRPTW.Computing;
using VisualizationApplication.Other;

namespace VisualizationApplication;

public partial class MainWindow : Window, IDisposable
{
    private readonly MainWindowElements _elements;
    private readonly MainWindowReactionHandler _reactionHandler;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _elements = new MainWindowElements(LoadDataButton, MainPlot, OptimizeButton, VisualizationComboBox, 
            PathCostTextBox, OptimizationMethodComboBox); 
        
        _reactionHandler = new MainWindowReactionHandler(_elements);
        
        _reactionHandler.FileLoaded += FileLoadedHandler;
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