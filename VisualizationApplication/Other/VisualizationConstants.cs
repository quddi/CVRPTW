using CVRPTW.Computing.Optimizers;
using ScottPlot;

namespace VisualizationApplication.Other;

public static class VisualizationConstants
{
    public static readonly Color DefaultPointsColor = new("#41B0FFFF");
    public static readonly Color DepoPointFillColor = new("#FF6666FF");
    
    public static readonly DeterministicAnnealingConfig DeterministicAnnealingConfig = new()
    {
        InitialTemperature = 5000,
        FinalTemperature = 0.5,
        CoolingRate = 0.97,
        MaxIterations = 5000,
        AttemptsPerTemperature = 50,
        CoolingSchedule = CoolingScheduleType.Geometric,
        Verbose = true
    };
}