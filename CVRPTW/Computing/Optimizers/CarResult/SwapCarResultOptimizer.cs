using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class SwapCarResultOptimizer(IMainResultEstimator mainResultEstimator) : CarResultOptimizer
{
    protected override void Optimize(MainResult mainResult, Car car)
    {
        var carResult = mainResult.Results[car];
        
        if (carResult.Path.Count < 4) return;
        
        var path = carResult.Path;
        var cost = mainResult.Estimation;
        var pathLength = carResult.Path.Count;
        
        for (var i = 1; i < pathLength - 1; i++)
        {
            for (int j = 1; j < pathLength - 1; j++)
            {
                if (i == j) continue;
                
                (path[i], path[j]) = (path[j], path[i]);

                var newCost = mainResultEstimator.Estimate(mainResult);

                if (newCost < cost)
                {
                    cost = newCost;
                }
                else
                {
                    (path[i], path[j]) = (path[j], path[i]);
                    mainResultEstimator.Estimate(mainResult);
                }
            }
        }

        carResult.Estimation = cost;
    }
}