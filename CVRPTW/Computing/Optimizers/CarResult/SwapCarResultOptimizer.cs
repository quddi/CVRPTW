using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class SwapCarResultOptimizer(IPathCostEstimator pathCostEstimator) : CarResultOptimizer
{
    public override void Optimize(CarResult carResult)
    {
        if (carResult.Path.Count < 4) return;
        
        var path = carResult.Path;
        var cost = carResult.Estimation;
        var pathLength = carResult.Path.Count;
        
        for (var i = 1; i < pathLength - 1; i++)
        {
            for (int j = 1; j < pathLength - 1; j++)
            {
                if (i == j) continue;
                
                (path[i], path[j]) = (path[j], path[i]);

                var newCost = pathCostEstimator.Estimate(path);

                if (newCost < cost) cost = newCost;
                else (path[i], path[j]) = (path[j], path[i]);
            }
        }

        carResult.Estimation = cost;
    }
}