using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt2CarResultOptimizer(PathEstimator pathEstimator) : CarResultOptimizer
{
    public override void Optimize(CarResult carResult)
    {
        if (carResult.Path.Count < 4) return;

        var pathLength = carResult.Path.Count;

        for (int beginIndex = 0; beginIndex < pathLength - 3; beginIndex++)
        {
            for (int endIndex = beginIndex + 3; endIndex < pathLength - 1; endIndex++)
            {
                TryOptimize(carResult, beginIndex, endIndex);
            }
        }
    }
    
    private void TryOptimize(CarResult result, int fromIndex, int toIndex)
    {
        var firstPairFirstPointId = result.Path[fromIndex];
        var firstPairSecondPointId = result.Path[fromIndex + 1];
        var secondPairFirstPointId = result.Path[toIndex - 1];
        var secondPairSecondPointId = result.Path[toIndex];

        var currentPrice = pathEstimator.Estimate(firstPairFirstPointId, firstPairSecondPointId) +
                                 pathEstimator.Estimate(secondPairFirstPointId, secondPairSecondPointId);
        
        var potentialPrice = pathEstimator.Estimate(firstPairFirstPointId, secondPairFirstPointId) +
                             pathEstimator.Estimate(firstPairSecondPointId, secondPairSecondPointId);

        if (potentialPrice >= currentPrice) return;
        
        result.Path.Invert(fromIndex + 1, toIndex - 1);
        
        result.ReEstimate(pathEstimator);
    }
}