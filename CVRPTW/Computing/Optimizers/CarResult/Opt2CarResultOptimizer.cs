using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt2CarResultOptimizer(IMainResultEstimator mainResultEstimator) : CarResultOptimizer
{
    protected override void Optimize(MainResult mainResult, Car car)
    {
        var carResult = mainResult.Results[car];
        
        if (carResult.Path.Count < 4) return;

        var pathLength = carResult.Path.Count;

        for (int beginIndex = 0; beginIndex < pathLength - 2; beginIndex++)
        {
            for (int endIndex = beginIndex + 3; endIndex < pathLength - 1; endIndex++)
            {
                TryOptimize(mainResult, carResult, beginIndex, endIndex);
            }
        }
    }
    
    private void TryOptimize(MainResult mainResult, CarResult result, int fromIndex, int toIndex)
    {
        var previousEstimation = mainResult.Estimation;
        
        result.Path.Invert(fromIndex + 1, toIndex - 1);

        var newEstimation = mainResultEstimator.Estimate(mainResult);
        
        if (newEstimation < previousEstimation)
            return;
        
        result.Path.Invert(fromIndex + 1, toIndex - 1);
        
        mainResult.ReEstimateCost(mainResultEstimator);
    }
}