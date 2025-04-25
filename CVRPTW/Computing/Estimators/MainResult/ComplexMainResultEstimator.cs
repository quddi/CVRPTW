using CVRPTW.Computing.Estimators.Time;

namespace CVRPTW.Computing.Estimators;

public class ComplexMainResultEstimator(MainData mainData, IMainResultEstimator baseEstimator, ITimeEstimator timeEstimator) 
    : IMainResultEstimator
{
    private double _carPenalty = 0;
    
    public double Estimate(MainResult mainResult)
    {
        var sum = baseEstimator.Estimate(mainResult);
        
        foreach (var (car, _) in mainResult.Results)
        {
            sum += GetTimePenalty(mainResult, car);
        }
        
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
            var point = mainData.PointsByIds[pointId];

            if (pointVisitResult.VisitTime < point.TimeWindow!.Start)
            {
                var waitPenalty = (point.TimeWindow.Start - pointVisitResult.VisitTime) * point.WaitPenalty;
                
                sum += waitPenalty;

                _carPenalty += waitPenalty;
            }

            if (pointVisitResult.VisitTime > point.TimeWindow!.End)
            {
                var latePenalty = (pointVisitResult.VisitTime - point.TimeWindow.End) * point.LatePenalty;
                
                sum += latePenalty;

                _carPenalty += latePenalty;
            }
        }
        
        if (carResult.Path.TravelTime > car.TimeWindow.End)
        {
            var carDriverOvertimePenalty = (carResult.Path.TravelTime - car.TimeWindow.End) * car.DriverOvertimePenalty;
            
            _carPenalty += carDriverOvertimePenalty;
        }
        
        return sum;
    }
}