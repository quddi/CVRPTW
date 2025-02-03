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
        
        sourceResult.Path.RemoveAt(sourcePointIndex);
        
        ReEstimate(sourceResult);

        for (int i = 1; i < targetPath.Count - 1; i++)
        {
            targetPath.Insert(i, sourcePointId);
            
            ReEstimate(targetResult);

            if (_mainResult.Estimation < bestEstimation)
            {
                bestEstimation = _mainResult.Estimation;
                bestPositionIndex = i;
            }
            
            targetPath.RemoveAt(i);
            
            ReEstimate(targetResult);
        }
        
        sourceResult.Path.Insert(sourcePointIndex, sourcePointId);
        
        ReEstimate(sourceResult);
        
        return (bestPositionIndex, bestEstimation);
    }

    private void ReEstimate(CarResult result)
    {
        result.PathCost = _pathEstimator.Estimate(result.Path);
    }

    private bool CanTranspose(Car targetCar, int pointId)
    {
        var currentCapacity = _mainResult!.Results[targetCar].PathCost;
        
        return targetCar.Capacity - currentCapacity >  _mainData!.PointsByIds[pointId].Demand;
    }
}