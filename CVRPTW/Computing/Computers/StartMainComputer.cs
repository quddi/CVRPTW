using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public class StartMainComputer(MainData mainData, IMainResultEstimator mainResultEstimator) : IteratedMainComputer(mainData, mainResultEstimator)
{
    private Dictionary<int, Point> _notVisitedPoints = new();

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
        var currentPointId = _mainData!.DepoPoint!.Id;
        var result = new CarResult(car)
        {
            Estimation = 0,
            Path = new CarPath
            {
                StartPoint = new(car.PointsDatas.First().Id!.Value),
                EndPoint = new(car.PointsDatas.Last().Id!.Value)
            }
        };

        while (_notVisitedPoints.Any(CanVisit))
        {
            var nextPointPair = _notVisitedPoints
                .Where(CanVisit)
                .MinBy(Distance);

            _notVisitedPoints.Remove(nextPointPair.Key);
                
            freeSpace -= nextPointPair.Value.Demand;
                
            result.Path.AddNextPoint(new(nextPointPair.Value.Id));
        }
            
        result.RemainedFreeSpace = freeSpace;
        
        bool CanVisit(KeyValuePair<int, Point> pair)
        {
            return pair.Value.Demand <= freeSpace;
        }

        double Distance(KeyValuePair<int, Point> pair)
        {
            var firstPointIndex = _mainData.GetPoint(currentPointId).Index;
            var secondPointIndex = _mainData.GetPoint(pair.Value.Id).Index;
            return _mainData.Distances!.GetDistance(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex);
        }
        
        return result;
    }
}