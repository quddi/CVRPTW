using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Optimizers;

namespace CVRPTW.Computing;

public class OptimizedPathComputer(PathComputer baseComputer, MainResultOptimizer optimizer, MainData mainData, PathEstimator pathEstimator)
    : PathComputer(mainData, pathEstimator)
{
    public override MainResult Compute()
    {
        var mainResult = baseComputer.Compute();
        optimizer.Optimize(mainResult);

        return mainResult;
    }
}