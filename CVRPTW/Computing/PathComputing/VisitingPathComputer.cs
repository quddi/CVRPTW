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
        carResult.Path.StartPointId = startPoint.Index;
        carResult.Path.EndPointId = startPoint.Index;

        while (_notVisitedPoints.Any())
        {
            var nextPoint = GetNextPoint(currentPoint);

            var distance = _mainData.Distances.GetDistance(Constants.DefaultMatrixId,
                currentPoint.Index, nextPoint.Index);

            carResult.PathCost += distance;
            carResult.Path.PathPointsIds.Add(nextPoint.Index);
            _notVisitedPoints.Remove(nextPoint);

            currentPoint = nextPoint;
        }

        var returnDistance = _mainData.Distances.GetDistance(Constants.DefaultMatrixId,
            carResult.Path.PathPointsIds.Last(), startPoint.Index);

        carResult.PathCost += returnDistance;

        _notVisitedPoints.Clear();
        
        return carResult;
    }

    protected abstract Point GetNextPoint(Point currentPoint);
}