namespace CVRPTW.Computing;

public class SimplePathComputer : PathComputer
{
    protected override List<CarResult> Compute()
    {
        var list = new List<CarResult>();
        
        foreach (var car in _mainData!.Cars)
        {
            list.Add(GetCarResult(car));
        }

        return list;
    }

    private CarResult GetCarResult(Car car)
    {
        var startPoint = _mainData!.Points.Random();
        var currentPoint = startPoint;
        
        var notVisitedPoints = new List<Point>(_mainData.Points);
        notVisitedPoints.Remove(startPoint);
        notVisitedPoints.Shuffle();

        var carResult = new CarResult(car);

        while (notVisitedPoints.Any())
        {
            var nextPoint = notVisitedPoints.SnatchFirst();

            var distance = _mainData.Distances.GetDistance(Constants.DefaultMatrixId,
                currentPoint.Id, nextPoint.Id);

            carResult.PathCost += distance;
            carResult.PathPointsIds.Add(currentPoint.Id);

            currentPoint = nextPoint;
        }

        var returnDistance = _mainData.Distances.GetDistance(Constants.DefaultMatrixId,
            carResult.PathPointsIds.Last(), startPoint.Id);

        carResult.PathCost += returnDistance;
        carResult.PathPointsIds.Add(startPoint.Id);

        return carResult;
    }
}