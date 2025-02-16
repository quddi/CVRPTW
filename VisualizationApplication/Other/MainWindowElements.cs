using System.Windows.Controls;
using ScottPlot.WPF;

namespace VisualizationApplication.Other;

public class MainWindowElements(Button loadDataButton, WpfPlot filterPlot, Button computeButton, 
    ComboBox visualizationComboBox, Label pathCostLabel)
{
    public Button LoadDataButton { get; set; } = loadDataButton;

    public WpfPlot FilterPlot { get; set; } = filterPlot;

    public Button ComputeButton { get; set; } = computeButton;
    
    public ComboBox VisualizationComboBox { get; set; } = visualizationComboBox;
    
    public Label PathCostLabel { get; set; } = pathCostLabel;
}