namespace CVRPTW.Computing;

public class NearestPathComputer : VisitingPathComputer
{
    protected override Point GetNextPoint(Point currentPoint)
    {
        var toPointsDistances = _mainData!.Distances.Matrix[Constants.DefaultMatrixId][currentPoint.Id];
        var nearestPointId = toPointsDistances
            .Where(pair => pair.Key != currentPoint.Id && _notVisitedPoints.Any(point => point.Id == pair.Key))
            .MinBy(pair => pair.Value)
            .Key;
        
        return _mainData!.Points[nearestPointId];
    }
}