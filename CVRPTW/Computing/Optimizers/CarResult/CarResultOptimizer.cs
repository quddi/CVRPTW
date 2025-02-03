using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public abstract class CarResultOptimizer(PathEstimator pathEstimator)
{
    public abstract void Optimize(CarResult carResult);
}