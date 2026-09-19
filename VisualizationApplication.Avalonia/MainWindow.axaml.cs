using Avalonia.Controls;

namespace VisualizationApplication.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        MainPlot.Plot.XLabel("Latitude");
        MainPlot.Plot.YLabel("Longitude");
        MainPlot.Refresh();
    }
}