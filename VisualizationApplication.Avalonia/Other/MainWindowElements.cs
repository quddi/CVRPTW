using Avalonia.Controls;
using ScottPlot.Avalonia;

namespace VisualizationApplication.Avalonia.Other;

public class MainWindowElements(Button loadDataButton, AvaPlot filterPlot, Button optimizeButton, 
    ComboBox visualizationComboBox, TextBox pathCostTextBox, ComboBox optimizationComboBox)
{
    public Button LoadDataButton { get; set; } = loadDataButton;

    public AvaPlot FilterPlot { get; set; } = filterPlot;

    public Button OptimizeButton { get; set; } = optimizeButton;
    
    public ComboBox VisualizationComboBox { get; set; } = visualizationComboBox;
    
    public TextBox PathCostTextBox { get; set; } = pathCostTextBox;
    
    public ComboBox OptimizationComboBox { get; set; } = optimizationComboBox;
}
