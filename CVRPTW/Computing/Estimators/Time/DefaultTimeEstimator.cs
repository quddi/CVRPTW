namespace CVRPTW.Computing.Estimators.Time;

public class DefaultTimeEstimator(MainData mainData) : TimeEstimator(mainData)
{
    public override void Estimate(CarPath path)
    {
        var timeSum = 0d;
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            var firstPointIndex = _idToIndex[path[i].Id];
            var secondPointIndex = _idToIndex[path[i + 1].Id];

            path[i] = path[i] with { VisitTime = timeSum };

            timeSum += _mainData.Times!.GetTime(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex);
        }
    }
}