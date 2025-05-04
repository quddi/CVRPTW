using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class AlternativePointsMainResultOptimizer(IMainResultEstimator mainResultEstimator, MainData mainData) : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        foreach (var currentAlternativePoints in mainData.AlternativePoints!.AllCorteges)
        {
            var minEstimation = mainResult.Estimation;
            var bestPointId = -1;
            
            foreach (var alternativePointId in currentAlternativePoints)
            {
                var (car, inPathIndex) = GetContainingPath(mainResult, alternativePointId);

                var taken = mainResult.Results[car].Path.TakeAt(inPathIndex);

                mainResult.ReEstimateCost(mainResultEstimator);
                
                if (mainResult.Estimation < minEstimation)
                {
                    minEstimation = mainResult.Estimation;
                    bestPointId = taken.Id;
                }
                
                mainResult.Results[car].Path.Insert(inPathIndex, taken);
                mainResult.ReEstimateCost(mainResultEstimator);
            }
            
            if (bestPointId == -1) 
                continue;
            
            var (bestCar, bestInPathIndex) = GetContainingPath(mainResult, bestPointId);
            mainResult.Results[bestCar].Path.RemoveAt(bestInPathIndex);
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