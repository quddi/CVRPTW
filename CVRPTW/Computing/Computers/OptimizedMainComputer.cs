using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Optimizers;

namespace CVRPTW.Computing;

public class OptimizedMainComputer(MainComputer baseComputer, MainResultOptimizer optimizer, MainData mainData) : MainComputer(mainData)
{
    public override MainResult Compute()
    {
        var mainResult = baseComputer.Compute();
        optimizer.Optimize(mainResult);

        return mainResult;
    }
}