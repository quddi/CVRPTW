namespace CVRPTW.Computing.Estimators.Time;

public abstract class TimeEstimator
{
    protected readonly MainData _mainData;
    protected readonly Dictionary<int, int> _idToIndex;

    protected TimeEstimator(MainData mainData)
    {
        _mainData = mainData;
        _idToIndex = _mainData.PointsByIds.Values.ToDictionary(point => point.Id, point => point.Index);
        _idToIndex[_mainData.DepoPoint!.Id] = _mainData.DepoPoint.Index;
    }
    
    public abstract void Estimate(CarPath path);
}