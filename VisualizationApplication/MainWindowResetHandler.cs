using VisualizationApplication.Other;

namespace VisualizationApplication;

public class MainWindowResetHandler(MainWindowElements mainWindowElements)
{
    public void ResetAll()
    {
        ResetMainPlot();
    }

    private void ResetMainPlot()
    {
        mainWindowElements.FilterPlot.Plot.Clear();
        mainWindowElements.FilterPlot.Refresh();
        mainWindowElements.FilterPlot.Plot.XLabel("Latitude");
        mainWindowElements.FilterPlot.Plot.YLabel("Longitude");
    }
}