using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

using SubResult = (int index, double estimation);

public class PointTransposeMainResultOptimizer(PathEstimator _pathEstimator) : MainResultOptimizer
{
    private MainResult? _mainResult;
    private MainData? _mainData;

    public override void Optimize(MainResult mainResult, MainData mainData)
    {
        _mainData = mainData;
        _mainResult = mainResult;

        var bestSubResults = 
            mainData.Cars.ToDictionary(car => car, _ =>
                mainData.Cars.ToDictionary(car => car, _ => default(SubResult?)));
        
        foreach (var (car, subDictionary) in bestSubResults)
        {
            subDictionary.Remove(car);
        }
        
        for (var i = 0; i < _mainData.Cars.Count; i++)
        {
            var sourceCar = _mainData.Cars[i];
            var sourceResult = _mainResult!.Results[sourceCar];
            
            if (sourceResult.Path.Count <= 2) continue;

            for (int j = 0; j < _mainData.Cars.Count; j++)
            {
                if (i == j) continue;
                
                var targetCar = _mainData.Cars[j];
                var currentResults = new List<SubResult>();
                
                for (var k = 1; k < sourceResult.Path.Count - 1; k++)
                {
                    currentResults.Add(GetBestPosition(targetCar, sourceCar, k));
                }

                bestSubResults[sourceCar][targetCar] = currentResults.MinBy(result => result.estimation);
            }
        }
        
        _mainData = null;
        _mainResult = null;
    }
    
    private SubResult GetBestPosition(Car targetCar, Car sourceCar, int sourcePointIndex)
    {
        var sourceResult = _mainResult!.Results[sourceCar];
        var sourcePointId = sourceResult.Path[sourcePointIndex];
        var bestPositionIndex = -1;
        var startEstimation = _mainResult.Estimation;
        var bestEstimation = startEstimation; 

        if (!CanTranspose(targetCar, sourcePointId)) return (bestPositionIndex, bestEstimation);
        
        var targetResult = _mainResult!.Results[targetCar];
        var targetPath = targetResult.Path;

        var previous = sourceResult.PathCost;
        
        sourceResult.Path.RemoveAt(sourcePointIndex);
        
        sourceResult.ReEstimate(_pathEstimator);

        var current = sourceResult.PathCost;

        var diff = previous - current;
        
        for (int i = 1; i < targetPath.Count; i++)
        {
            targetPath.Insert(i, sourcePointId);
            
            targetResult.ReEstimate(_pathEstimator);

            if (_mainResult.Estimation < bestEstimation)
            {
                bestEstimation = _mainResult.Estimation;
                bestPositionIndex = i;
            }
            
            targetPath.RemoveAt(i);
            
            targetResult.ReEstimate(_pathEstimator);
        }
        
        sourceResult.Path.Insert(sourcePointIndex, sourcePointId);
        
        sourceResult.ReEstimate(_pathEstimator);
        
        return (bestPositionIndex, bestEstimation);
    }

    private bool CanTranspose(Car targetCar, int pointId)
    {
        var currentCapacity = _mainResult!.Results[targetCar].PathCost;
        
        return targetCar.Capacity - currentCapacity >  _mainData!.PointsByIds[pointId].Demand;
    }
}