using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public abstract class MainResultOptimizer : IOptimizer
{
    public string Name { get; set; } = string.Empty;
    
    public abstract void Optimize(MainResult mainResult);
}