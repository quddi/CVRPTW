namespace CVRPTW.Computing.Estimators.Time;

public class SimpleTimeEstimator(MainData mainData) : ITimeEstimator
{
    public double Estimate(CarPath path, Car car)
    {
        var timeSum = car.TimeWindow.Start;
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            var firstPointIndex = mainData.GetPoint(path[i].Id).Index;
            var secondPointIndex = mainData.GetPoint(path[i + 1].Id).Index;

            path[i] = path[i] with { VisitTime = timeSum };

            timeSum += mainData.Times!.GetTime(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex);
        }
        
        return timeSum;
    }
}