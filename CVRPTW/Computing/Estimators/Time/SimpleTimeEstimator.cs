namespace CVRPTW.Computing.Estimators.Time;

public class SimpleTimeEstimator(MainData mainData) : ITimeEstimator
{
    public double Estimate(CarPath path, Car car)
    {
        var timeSum = car.TimeWindow.Start;
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            var firstPoint = mainData.GetPoint(path[i].Id);
            var firstPointIndex = firstPoint.Index;
            var secondPointIndex = mainData.GetPoint(path[i + 1].Id).Index;

            path[i] = path[i] with { VisitTime = timeSum };

            // VisitTime лишається часом прибуття (за ним рахуються штрафи); очікування й обслуговування зсувають лише виїзд.
            if (firstPoint.TimeWindow is { } window && timeSum < window.Start)
                timeSum = window.Start;

            timeSum += firstPoint.ServiceTime;

            timeSum += mainData.Times!.GetTime(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex);
            
            path[i + 1] = path[i + 1] with { VisitTime = timeSum };
        }
        
        return timeSum;
    }
}