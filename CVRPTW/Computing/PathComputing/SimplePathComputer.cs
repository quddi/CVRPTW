namespace CVRPTW.Computing;

public class SimplePathComputer : VisitingPathComputer
{
    protected override Point GetNextPoint(Point _)
    {
        return _notVisitedPoints.SnatchFirst();
    }
}