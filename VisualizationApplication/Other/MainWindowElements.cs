using System.Windows.Controls;
using ScottPlot.WPF;

namespace VisualizationApplication.Other;

public class MainWindowElements(Button loadDataButton, WpfPlot filterPlot, Button optimizeButton, TextBox inputTextBox)
{
    public Button LoadDataButton { get; set; } = loadDataButton;

    public WpfPlot FilterPlot { get; set; } = filterPlot;

    public Button OptimizeButton { get; set; } = optimizeButton;

    public TextBox InputTextBox { get; set; } = inputTextBox;
}