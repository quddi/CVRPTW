using CVRPTW;
using CVRPTW.Computing.Optimizers;
using VisualizationApplication.Other;
// ReSharper disable MemberCanBePrivate.Global

namespace VisualizationApplication;

public class MainWindowResetHandler(MainWindowElements mainWindowElements)
{
    public void ResetAll(MainResult? mainResult, INamed[]? optimizers)
    {
        ResetMainPlot();
        ResetVisualizationComboBox(mainResult);
        ResetOptimizationComboBox(optimizers);
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

        var previousIndex = comboBox.SelectedIndex;
        
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

        comboBox.SelectedIndex = previousIndex;
    }

    public void ResetOptimizationComboBox(INamed[]? optimizers)
    {
        var comboBox = mainWindowElements.OptimizationComboBox;
        
        var previousIndex = comboBox.SelectedIndex;
        
        comboBox.SelectedIndex = Constants.NotSelectedIndex;
        
        comboBox.Items.Clear();
        
        if (optimizers == null) return;

        foreach (var optimizer in optimizers)
        {
            comboBox.Items.Add(optimizer.Name);
        }

        comboBox.SelectedIndex = previousIndex;
    }
}
