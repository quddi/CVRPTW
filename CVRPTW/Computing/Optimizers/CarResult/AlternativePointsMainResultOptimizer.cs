using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class AlternativePointsMainResultOptimizer(PathEstimator pathEstimator, MainData mainData) : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        foreach (var (firstPointId, secondPointId) in mainData.AlternativePoints!)
        {
            var (firstCar, firstIndex) = GetContainingPath(mainResult, firstPointId);
            var (secondCar, secondIndex) = GetContainingPath(mainResult, secondPointId);
            
            var previousEstimation = mainResult.Estimation;
            
            var firstPath = mainResult.Results[firstCar].Path;
            var secondPath = mainResult.Results[secondCar].Path;
            
            (firstPath[firstIndex], secondPath[secondIndex]) = (secondPath[secondIndex], firstPath[firstIndex]);
            
            mainResult.Results[firstCar].ReEstimate(pathEstimator);
            mainResult.Results[secondCar].ReEstimate(pathEstimator);
            
            var currentEstimation = mainResult.Estimation;
            
            if (currentEstimation < previousEstimation) continue;
            
            (firstPath[firstIndex], secondPath[secondIndex]) = (secondPath[secondIndex], firstPath[firstIndex]);
            
            mainResult.Results[firstCar].ReEstimate(pathEstimator);
            mainResult.Results[secondCar].ReEstimate(pathEstimator);
        } 
    }
    
    private (Car car, int inPathIndex) GetContainingPath(MainResult mainResult, int pointId)
    {
        var car = mainResult.Results.FirstOrDefault(pair => pair.Value.Path.Contains(pointId)).Key;
        
        return (car, mainResult.Results[car].Path.IndexOf(pointId));
    }
}