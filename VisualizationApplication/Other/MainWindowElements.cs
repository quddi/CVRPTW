using System.Windows.Controls;
using ScottPlot.WPF;

namespace VisualizationApplication.Other;

public class MainWindowElements(Button loadDataButton, WpfPlot filterPlot, Button optimizeButton, 
    ComboBox visualizationComboBox, Label pathCostLabel, ComboBox optimizationComboBox)
{
    public Button LoadDataButton { get; set; } = loadDataButton;

    public WpfPlot FilterPlot { get; set; } = filterPlot;

    public Button OptimizeButton { get; set; } = optimizeButton;
    
    public ComboBox VisualizationComboBox { get; set; } = visualizationComboBox;
    
    public Label PathCostLabel { get; set; } = pathCostLabel;
    
    public ComboBox OptimizationComboBox { get; set; } = optimizationComboBox;
}
