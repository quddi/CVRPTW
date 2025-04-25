using System.Windows.Controls;
using ScottPlot.WPF;

namespace VisualizationApplication.Other;

public class MainWindowElements(Button loadDataButton, WpfPlot filterPlot, Button optimizeButton, 
    ComboBox visualizationComboBox, TextBox pathCostTextBox, ComboBox optimizationComboBox)
{
    public Button LoadDataButton { get; set; } = loadDataButton;

    public WpfPlot FilterPlot { get; set; } = filterPlot;

    public Button OptimizeButton { get; set; } = optimizeButton;
    
    public ComboBox VisualizationComboBox { get; set; } = visualizationComboBox;
    
    public TextBox PathCostTextBox { get; set; } = pathCostTextBox;
    
    public ComboBox OptimizationComboBox { get; set; } = optimizationComboBox;
}
