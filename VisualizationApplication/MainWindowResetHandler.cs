using CVRPTW;
using VisualizationApplication.Other;
// ReSharper disable MemberCanBePrivate.Global

namespace VisualizationApplication;

public class MainWindowResetHandler(MainWindowElements mainWindowElements)
{
    public void ResetAll(MainResult? mainResult)
    {
        ResetMainPlot();
        ResetVisualizationComboBox(mainResult);
    }

    public void ResetMainPlot()
    {
        var filterPlot = mainWindowElements.FilterPlot;
        
        filterPlot.Plot.Clear();
        filterPlot.Refresh();
        filterPlot.Plot.XLabel("Latitude");
        filterPlot.Plot.YLabel("Longitude");
    }

    public void ResetVisualizationComboBox(MainResult? mainResult)
    {
        var comboBox = mainWindowElements.VisualizationComboBox;

        comboBox.SelectedIndex = Constants.NotSelectedIndex;
        
        comboBox.Items.Clear();
        
        comboBox.Items.Add("Тільки точки");

        if (mainResult != null)
        {
            comboBox.Items.Add("Усі результати");
            
            for (var i = 0; i < mainResult.Results.Count; i++)
            {
                comboBox.Items.Add($"{i + 1}й результат");
            }
        }

        comboBox.SelectedIndex = 0;
    }
}