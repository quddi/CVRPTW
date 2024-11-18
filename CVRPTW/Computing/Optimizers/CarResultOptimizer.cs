using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class CarResultOptimizer(PathEstimator pathEstimator)
{
    public void Optimize(CarResult carResult)
    {
        if (carResult.Path.Length < 4)
            throw new ArgumentException("Trying to optimize a path with less then 4 points!");
        
        var path = carResult.Path;
        var cost = carResult.PathCost;
        var pathLength = carResult.Path.Length;
        
        for (var i = 1; i < pathLength - 1; i++)
        {
            for (int j = 1; j < pathLength - 1; j++)
            {
                if (i == j) continue;
                
                (path[i], path[j]) = (path[j], path[i]);

                var newCost = pathEstimator.Estimate(path);

                if (newCost < cost) cost = newCost;
                else (path[i], path[j]) = (path[j], path[i]);
            }
        }

        carResult.PathCost = cost;
    }
}