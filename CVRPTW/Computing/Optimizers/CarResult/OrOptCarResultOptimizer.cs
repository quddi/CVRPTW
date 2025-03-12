using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class OrOptCarResultOptimizer(IPathCostEstimator pathCostEstimator) : CarResultOptimizer
{
    public override void Optimize(CarResult carResult)
    {
        if (carResult.Path.Count < 4) return;
        
        carResult.ReEstimateCost(pathCostEstimator);
        
        CheckPointsTranspose(carResult, 1);
        
        if (carResult.Path.Count < 5) return;
        
        CheckPointsTranspose(carResult, 2);
        
        if (carResult.Path.Count < 6) return;
        
        CheckPointsTranspose(carResult, 3);
    }

    private void CheckPointsTranspose(CarResult carResult, int pointsCount)
    {
        var currentEstimation = carResult.Estimation;
        var path = carResult.Path;
        
        for (int fromIndex = 1; fromIndex < carResult.Path.Count - 2; fromIndex++)
        {
            for (int toIndex = fromIndex + pointsCount; toIndex < carResult.Path.Count - 1; toIndex++)
            {
                for (int i = 0; i < pointsCount; i++)
                {
                    var point = path.TakeAt(fromIndex);
                    path.Insert(toIndex, point);
                }
                
                carResult.ReEstimateCost(pathCostEstimator);

                if (carResult.Estimation < currentEstimation)
                {
                    currentEstimation = carResult.Estimation;
                }
                else
                {
                    for (int i = 0; i < pointsCount; i++)
                    {
                        var point = path.TakeAt(toIndex);
                        path.Insert(fromIndex, point);
                    }
                    
                    carResult.ReEstimateCost(pathCostEstimator);
                }
            }
        }
    }
}