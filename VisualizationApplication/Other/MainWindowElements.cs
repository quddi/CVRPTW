using System.Windows.Controls;
using ScottPlot.WPF;

namespace VisualizationApplication.Other;

public class MainWindowElements
{
    public Button LoadDataButton { get; set; }

    public WpfPlot FilterPlot { get; set; }

    public MainWindowElements(Button loadDataButton, WpfPlot filterPlot)
    {
        LoadDataButton = loadDataButton;
        FilterPlot = filterPlot;
    }
}