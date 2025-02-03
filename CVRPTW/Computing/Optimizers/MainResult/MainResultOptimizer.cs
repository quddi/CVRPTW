using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public abstract class MainResultOptimizer
{
    public abstract void Optimize(MainResult mainResult, MainData mainData);
}