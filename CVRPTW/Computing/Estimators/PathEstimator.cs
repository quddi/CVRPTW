namespace CVRPTW.Computing.Estimators;

public abstract class PathEstimator
{
    protected readonly MainData _mainData;
    protected readonly Dictionary<int, int> _idToIndex;

    protected PathEstimator(MainData mainData)
    {
        _mainData = mainData;
        _idToIndex = _mainData.PointsByIds.Values.ToDictionary(point => point.Id, point => point.Index);
        _idToIndex[_mainData.DepoPoint.Id] = _mainData.DepoPoint.Index;
    }

    public abstract double Estimate(CarPath path);

    public abstract double Estimate(int[] path);

    public abstract double Estimate(int firstPointId, int secondPointId);
}