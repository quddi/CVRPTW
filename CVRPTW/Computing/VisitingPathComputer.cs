namespace CVRPTW.Computing;

public abstract class VisitingPathComputer : IteratedPathComputer
{
    protected List<Point> _notVisitedPoints = new();
    
    protected override CarResult GetCarResult(Car car)
    {
        var startPoint = _mainData!.Points.RandomValue();
        var currentPoint = startPoint;
        
        _notVisitedPoints.AddRange(_mainData.Points.Values);
        _notVisitedPoints.Remove(startPoint);
        _notVisitedPoints.Shuffle();

        var carResult = new CarResult(car);

        while (_notVisitedPoints.Any())
        {
            var nextPoint = GetNextPoint(currentPoint);

            var distance = _mainData.Distances.GetDistance(Constants.DefaultMatrixId,
                currentPoint.Id, nextPoint.Id);

            carResult.PathCost += distance;
            carResult.PathPointsIds.Add(currentPoint.Id);
            _notVisitedPoints.Remove(nextPoint);

            currentPoint = nextPoint;
        }

        var returnDistance = _mainData.Distances.GetDistance(Constants.DefaultMatrixId,
            carResult.PathPointsIds.Last(), startPoint.Id);

        carResult.PathCost += returnDistance;
        carResult.PathPointsIds.Add(startPoint.Id);

        _notVisitedPoints.Clear();
        
        return carResult;
    }

    protected abstract Point GetNextPoint(Point currentPoint);
}