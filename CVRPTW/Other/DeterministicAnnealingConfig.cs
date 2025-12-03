namespace CVRPTW.Computing.Optimizers;

public class DeterministicAnnealingConfig
{
    public double InitialTemperature { get; set; } = 1000.0;
    public double FinalTemperature { get; set; } = 0.1;
    public double CoolingRate { get; set; } = 0.95;
    public double LinearStep { get; set; } = 0.5;
    public int MaxIterations { get; set; } = 10000;
    public int AttemptsPerTemperature { get; set; } = 100;
    public CoolingScheduleType CoolingSchedule { get; set; } = CoolingScheduleType.Geometric;
    public bool Verbose { get; set; } = false;
}