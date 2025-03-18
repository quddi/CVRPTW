using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class OrOptCarResultOptimizer(IMainResultEstimator resultEstimator) : CarResultOptimizer
{
    protected override void Optimize(MainResult mainResult, Car car)
    {
        var carResult = mainResult.Results[car];
        
        if (carResult.Path.Count < 4) return;

        resultEstimator.Estimate(mainResult);
        
        CheckPointsTranspose(mainResult, car, 1);
        
        if (carResult.Path.Count < 5) return;
        
        CheckPointsTranspose(mainResult, car, 2);
        
        if (carResult.Path.Count < 6) return;
        
        CheckPointsTranspose(mainResult, car, 3);
    }

    private void CheckPointsTranspose(MainResult mainResult, Car car, int pointsCount)
    {
        var carResult = mainResult.Results[car];
        var minEstimation = resultEstimator.Estimate(mainResult);
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
                
                var newEstimation = resultEstimator.Estimate(mainResult);

                if (newEstimation < minEstimation)
                {
                    minEstimation = carResult.Estimation;
                }
                else
                {
                    for (int i = 0; i < pointsCount; i++)
                    {
                        var point = path.TakeAt(toIndex);
                        path.Insert(fromIndex, point);
                    }

                    resultEstimator.Estimate(mainResult);
                }
            }
        }
    }
}