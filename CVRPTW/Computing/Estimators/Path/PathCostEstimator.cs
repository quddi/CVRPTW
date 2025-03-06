namespace CVRPTW.Computing.Estimators;

public abstract class PathCostEstimator
{
    protected readonly MainData _mainData;
    protected readonly Dictionary<int, int> _idToIndex;

    protected PathCostEstimator(MainData mainData)
    {
        _mainData = mainData;
        _idToIndex = _mainData.PointsByIds.Values.ToDictionary(point => point.Id, point => point.Index);
        _idToIndex[_mainData.DepoPoint!.Id] = _mainData.DepoPoint.Index;
    }

    public abstract double Estimate(CarPath path);
}