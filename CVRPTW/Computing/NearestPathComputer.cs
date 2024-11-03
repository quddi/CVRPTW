namespace CVRPTW.Computing;

public class NearestPathComputer : VisitingPathComputer
{
    protected override Point GetNextPoint(Point currentPoint)
    {
        var toPointsDistances = _mainData!.Distances.Matrix[Constants.DefaultMatrixId][currentPoint.Index];
        var nearestPointId = toPointsDistances
            .Where(pair => pair.Key != currentPoint.Index && _notVisitedPoints.Any(point => point.Index == pair.Key))
            .MinBy(pair => pair.Value)
            .Key;
        
        return _mainData!.Points[nearestPointId];
    }
}