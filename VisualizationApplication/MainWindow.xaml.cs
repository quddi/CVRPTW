using System.Windows;
using VisualizationApplication.Other;

namespace VisualizationApplication;

public partial class MainWindow : Window
{
    private readonly MainWindowElements _elements;
    private readonly MainWindowReactionHandler _reactionHandler;
    
    public MainWindow()
    {
        InitializeComponent();

        _elements = new MainWindowElements(LoadDataButton, MainPlot, OptimizeButton, InputTextBox);
        _reactionHandler = new MainWindowReactionHandler(_elements);
    }
}