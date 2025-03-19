using CVRPTW.Computing.Estimators.Time;

namespace CVRPTW.Computing.Estimators;

public class ComplexMainResultEstimator(MainData mainData, IMainResultEstimator baseEstimator, ITimeEstimator timeEstimator) 
    : IMainResultEstimator
{
    public double Estimate(MainResult mainResult)
    {
        var sum = baseEstimator.Estimate(mainResult);
        
        foreach (var (car, carResult) in mainResult.Results)
        {
            //sum += GetTimePenalty(mainResult, car);
        }

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
                sum += (point.TimeWindow.Start - pointVisitResult.VisitTime) * point.WaitPenalty;
            
            if (pointVisitResult.VisitTime > point.TimeWindow!.End)
                sum += (pointVisitResult.VisitTime - point.TimeWindow.End) * point.LatePenalty;
        }

        if (carResult.Path.TravelTime > car.TimeWindow.End)
            sum += (carResult.Path.TravelTime - car.TimeWindow.End) * car.DriverOvertimePenalty;
        
        return sum;
    }
}