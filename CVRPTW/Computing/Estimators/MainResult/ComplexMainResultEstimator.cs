using CVRPTW.Computing.Estimators.Time;

namespace CVRPTW.Computing.Estimators;

public class ComplexMainResultEstimator(MainData mainData, IMainResultEstimator baseEstimator, ITimeEstimator timeEstimator) 
    : IMainResultEstimator
{
    private readonly double _unservedPenalty = ByDistanceMainResultEstimator.CalculateUnreachablePenalty(mainData);

    public double Estimate(MainResult mainResult)
    {
        var sum = baseEstimator.Estimate(mainResult);
        
        foreach (var (car, _) in mainResult.Results)
        {
            var timePenalty = GetTimePenalty(mainResult, car);

            mainResult.Results[car].Estimation += timePenalty;
            sum += timePenalty;
        }
        
        sum += mainData.CountUnserved(mainResult) * _unservedPenalty;

        mainResult.Estimation = sum;

        return sum;
    }

    private double GetTimePenalty(MainResult result, Car car)
    {
        var carResult = result.Results[car];
        
        carResult.ReEstimateTime(timeEstimator, car);

        var sum = 0d;
        
        for (var i = 1; i < carResult.Path.Count - 1; i++)
        {
            var pointVisitResult = carResult.Path[i];
            var pointId = pointVisitResult.Id;
            
            if (pointId.IsDepoId()) continue;
            
            var point = mainData.GetPoint(pointId);
            
            if (point.TimeWindow == null) continue;

            if (pointVisitResult.VisitTime < point.TimeWindow!.Start)
            {
                var waitPenalty = (point.TimeWindow.Start - pointVisitResult.VisitTime) * point.WaitPenalty;
                
                sum += waitPenalty;
            }

            if (pointVisitResult.VisitTime > point.TimeWindow!.End)
            {
                var latePenalty = (pointVisitResult.VisitTime - point.TimeWindow.End) * point.LatePenalty;
                
                sum += latePenalty;
            }
        }

        var returnTime = carResult.Path[^1].VisitTime;

        if (returnTime > car.TimeWindow.End)
            sum += (returnTime - car.TimeWindow.End) * car.DriverOvertimePenalty;
        
        return sum;
    }
}