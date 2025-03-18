using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class AlternativePointsMainResultOptimizer(IMainResultEstimator mainResultEstimator, MainData mainData) : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        foreach (var (firstPointId, secondPointId) in mainData.AlternativePoints!)
        {
            var (firstCar, firstIndex) = GetContainingPath(mainResult, firstPointId);
            var (secondCar, secondIndex) = GetContainingPath(mainResult, secondPointId);
            
            var firstPath = mainResult.Results[firstCar].Path;
            var secondPath = mainResult.Results[secondCar].Path;
            
            var firstTaken = firstPath.TakeAt(firstIndex);

            var firstEstimation = mainResultEstimator.Estimate(mainResult);
            
            firstPath.Insert(firstIndex, firstTaken);
            
            var secondTaken = secondPath.TakeAt(secondIndex);
            
            mainResult.ReEstimateCost(mainResultEstimator);
            
            if (mainResult.Estimation < firstEstimation) continue;
            
            secondPath.Insert(secondIndex, secondTaken);
            
            firstPath.RemoveAt(firstIndex);
            
            mainResult.ReEstimateCost(mainResultEstimator);
        } 
    }
    
    private (Car car, int inPathIndex) GetContainingPath(MainResult mainResult, int pointId)
    {
        var car = mainResult.Results
            .FirstOrDefault(pair => pair.Value.Path.Contains(pointId)).Key;
        
        return (car, mainResult.Results[car].Path.IndexOf(pointId));
    }
}