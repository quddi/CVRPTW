using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt2CarResultOptimizer(PathCostEstimator pathCostEstimator) : CarResultOptimizer
{
    public override void Optimize(CarResult carResult)
    {
        if (carResult.Path.Count < 4) return;

        var pathLength = carResult.Path.Count;

        for (int beginIndex = 0; beginIndex < pathLength - 2; beginIndex++)
        {
            for (int endIndex = beginIndex + 3; endIndex < pathLength - 1; endIndex++)
            {
                TryOptimize(carResult, beginIndex, endIndex);
            }
        }
    }
    
    private void TryOptimize(CarResult result, int fromIndex, int toIndex)
    {
        var previousEstimation = result.Estimation;
        
        result.Path.Invert(fromIndex + 1, toIndex - 1);
        
        result.ReEstimate(pathCostEstimator);

        var newEstimation = result.Estimation;
        
        if (newEstimation < previousEstimation)
            return;
        
        result.Path.Invert(fromIndex + 1, toIndex - 1);
        
        result.ReEstimate(pathCostEstimator);
    }
}