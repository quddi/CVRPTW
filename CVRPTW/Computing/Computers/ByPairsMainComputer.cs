using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class ByPairsMainComputer(MainData mainData, IMainResultEstimator mainResultEstimator) : IteratedMainComputer(mainData, mainResultEstimator)
{
    protected Dictionary<int, Point> _notVisitedPoints = new();

    public override MainResult Compute()
    {
        _notVisitedPoints = new Dictionary<int, Point>(_mainData!.PointsByIds);

        var mainResult = base.Compute();
        
        mainResultEstimator.Estimate(mainResult);
        
        return mainResult;
    }

    protected override CarResult GetCarResult(Car car)
    {
        var freeSpace = car.Capacity * 1.0;
        var result = new CarResult(car)
        {
            Estimation = 0,
            Path = new CarPath
            {
                StartPoint = new(car.PointsDatas.First().Id!.Value),
                EndPoint = new(car.PointsDatas.Last().Id!.Value)
            }
        };
        var currentPointId = result.Path.StartPoint.Id;

        while (TryFindNextPoint(currentPointId, freeSpace, out var nextPointPair))
        {
            _notVisitedPoints.Remove(nextPointPair.Key);

            freeSpace -= nextPointPair.Value.Demand;

            result.Path.AddNextPoint(new(nextPointPair.Value.Id));

            currentPointId = nextPointPair.Value.Id;
        }

        result.RemainedFreeSpace = freeSpace;

        return result;
    }

    protected bool TryFindNextPoint(int currentPointId, double freeSpace, out KeyValuePair<int, Point> nextPointPair)
    {
        var found = false;
        var bestEstimation = double.MaxValue;
        nextPointPair = default;

        foreach (var pair in _notVisitedPoints)
        {
            if (pair.Value.Demand > freeSpace) continue;
            if (!TryEstimatePair(pair.Value.Id, currentPointId, out var estimation)) continue;
            if (found && estimation >= bestEstimation) continue;

            found = true;
            bestEstimation = estimation;
            nextPointPair = pair;
        }

        return found;
    }

    protected abstract bool TryEstimatePair(int checkedPointId, int currentPointId, out double estimation);
}