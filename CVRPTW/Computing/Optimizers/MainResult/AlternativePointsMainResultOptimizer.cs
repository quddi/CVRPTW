using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class AlternativePointsMainResultOptimizer(IMainResultEstimator mainResultEstimator, MainData mainData) : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        if (mainData?.AlternativePoints == null) return;
        
        foreach (var currentAlternativePoints in mainData.AlternativePoints!.AllCorteges)
        {
            var minEstimation = mainResult.Estimation;
            var bestPointId = -1;
            
            foreach (var alternativePointId in currentAlternativePoints)
            {
                var (car, inPathIndex) = GetContainingPath(mainResult, alternativePointId);
                
                if (car == null || inPathIndex == null) continue;

                var taken = mainResult.Results[car].Path.TakeAt(inPathIndex!.Value);

                mainResult.ReEstimateCost(mainResultEstimator);
                
                if (mainResult.Estimation < minEstimation)
                {
                    minEstimation = mainResult.Estimation;
                    bestPointId = taken.Id;
                }
                
                mainResult.Results[car].Path.Insert(inPathIndex!.Value, taken);
                mainResult.ReEstimateCost(mainResultEstimator);
            }
            
            if (bestPointId == -1) 
                continue;
            
            var (bestCar, bestInPathIndex) = GetContainingPath(mainResult, bestPointId);
            mainResult.Results[bestCar!].Path.RemoveAt(bestInPathIndex!.Value);
            mainResult.ReEstimateCost(mainResultEstimator);
        } 
    }
    
    private (Car? car, int? inPathIndex) GetContainingPath(MainResult mainResult, int pointId)
    {
        var car = mainResult.Results
            .FirstOrDefault(pair => pair.Value.Path.Contains(pointId)).Key;
        
        return car == null 
            ? (null, null) 
            : (car, mainResult.Results[car].Path.IndexOf(pointId));
    }
}