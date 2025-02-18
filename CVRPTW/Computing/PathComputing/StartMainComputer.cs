using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public class StartMainComputer(MainData mainData, MainResultEstimator mainResultEstimator) : IteratedMainComputer(mainData, mainResultEstimator)
{
    private Dictionary<int, Point> _notVisitedPoints = new();
    private readonly MainResultEstimator _mainResultEstimator = mainResultEstimator;

    public override MainResult Compute()
    {
        _notVisitedPoints = new Dictionary<int, Point>(_mainData!.PointsByIds);
        
        return base.Compute();
    }

    protected override CarResult GetCarResult(Car car)
    {
        var freeSpace = car.Capacity * 1.0;
        var currentPointId = _mainData!.DepoPoint!.Id;
        var result = new CarResult(car)
        {
            PathCost = 0,
            Path = new CarPath
            {
                StartPointId = car.PointsInfos.First().Id!.Value,
                EndPointId = car.PointsInfos.Last().Id!.Value
            }
        };

        while (_notVisitedPoints.Any(CanVisit))
        {
            var nextPointPair = _notVisitedPoints
                .Where(CanVisit)
                .MinBy(point => _mainResultEstimator.PathEstimator.Estimate(currentPointId, point.Value.Id));

            _notVisitedPoints.Remove(nextPointPair.Key);
                
            freeSpace -= nextPointPair.Value.Demand;
                
            result.PathCost += _mainResultEstimator.PathEstimator.Estimate(currentPointId, nextPointPair.Value.Id);
            result.Path.AddNextPoint(nextPointPair.Value.Id);
        }
            
        result.RemainedFreeSpace = freeSpace;
            
        return result;

        bool CanVisit(KeyValuePair<int, Point> pair)
        {
            return pair.Value.Demand <= freeSpace;
        }
    }
}