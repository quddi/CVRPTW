using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

using SubResult = (Car targetCar, int targetIndex, double estimation);

public class PointTransposeMainResultOptimizer(IMainResultEstimator mainResultEstimator,  MainData mainData) : MainResultOptimizer
{
    private MainResult? _mainResult;
    private MainData? _mainData;

    private const int SameBestPointIndex = -1;
    
    public override void Optimize(MainResult mainResult)
    {
        _mainData = mainData;
        _mainResult = mainResult;
        
        if (mainData.Cars.Count == 1) return;
        
        for (var i = 0; i < _mainData.Cars.Count; i++)
        {
            var sourceCar = _mainData.Cars[i];
            var sourceResult = _mainResult!.Results[sourceCar];
            
            if (sourceResult.Path.Count <= 2) continue;

            var results = new List<(int sourceIndex, SubResult sub)>();
            
            for (var sourceIndex = 1; sourceIndex < sourceResult.Path.Count - 1; sourceIndex++)
            {
                var subResults = new List<SubResult>();
                
                for (int j = 0; j < _mainData.Cars.Count; j++)
                {
                    if (i == j) continue;
                    
                    var targetCar = _mainData.Cars[j];
                    
                    subResults.Add(GetBest(targetCar, sourceCar, sourceIndex));
                }
                
                results.Add((sourceIndex, subResults.MinBy(x => x.estimation)));
            }
            
            //Apply
            for (var j = 0; j < results.Count; j++)
            {
                var (sourceIndex, (targetCar, targetIndex, _)) = results[j];
                
                if (targetCar == sourceCar || targetIndex == SameBestPointIndex) continue;

                var targetResult = _mainResult.Results[targetCar];

                //Take from source
                var pointId = sourceResult.Path.TakeAt(sourceIndex);

                //Put to target
                targetResult.Path.Insert(targetIndex, pointId);

                //Shift nextIndices
                for (int k = j + 1; k < results.Count; k++)
                {
                    var sub = targetCar == results[k].sub.targetCar
                        ? (results[k].sub.targetCar, results[k].sub.targetIndex + 1, results[k].sub.estimation)
                        : results[k].sub;
                    
                    results[k] = (results[k].sourceIndex - 1, sub);
                }

                //ReEstimate both
                _mainResult.ReEstimateCost(mainResultEstimator);
            }
        }
        
        _mainData = null;
        _mainResult = null;
    }
    
    private SubResult GetBest(Car targetCar, Car sourceCar, int sourcePointIndex)
    {
        var sourceResult = _mainResult!.Results[sourceCar];
        var sourcePointResult = sourceResult.Path[sourcePointIndex];
        var bestPositionIndex = SameBestPointIndex;
        var startEstimation = _mainResult.Estimation;

        if (!CanTranspose(targetCar, sourcePointResult.Id)) return (sourceCar, bestPositionIndex, startEstimation);
        
        var bestEstimation = startEstimation; 
        var targetResult = _mainResult!.Results[targetCar];
        var targetPath = targetResult.Path;
        
        sourceResult.Path.RemoveAt(sourcePointIndex);
        
        _mainResult.ReEstimateCost(mainResultEstimator);
        
        for (int i = 1; i < targetPath.Count; i++)
        {
            targetPath.Insert(i, sourcePointResult);
            
            _mainResult.ReEstimateCost(mainResultEstimator);

            if (_mainResult.Estimation < bestEstimation)
            {
                bestEstimation = _mainResult.Estimation;
                bestPositionIndex = i;
            }
            
            targetPath.RemoveAt(i);
            
            _mainResult.ReEstimateCost(mainResultEstimator);
        }
        
        sourceResult.Path.Insert(sourcePointIndex, sourcePointResult);
        
        _mainResult.ReEstimateCost(mainResultEstimator);
        
        return (targetCar, bestPositionIndex, bestEstimation);
    }

    private bool CanTranspose(Car targetCar, int pointId)
    {
        var currentCapacity = _mainResult!.Results[targetCar].Estimation;
        
        return targetCar.Capacity - currentCapacity >  _mainData!.PointsByIds[pointId].Demand;
    }
}