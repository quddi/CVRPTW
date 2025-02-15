using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public abstract class MainResultOptimizer : IOptimizer
{
    public abstract void Optimize(MainResult mainResult);
}