namespace CVRPTW;

public struct PointVisitResult
{
    public int Id { get; set; }
    
    public double VisitTime { get; set; }

    public PointVisitResult() : this(0, 0) { }

    public PointVisitResult(int id) : this(id, 0) { }

    public PointVisitResult(int id, double visitTime)
    {
        Id = id;
        VisitTime = visitTime;
    }

    public static bool operator ==(PointVisitResult left, PointVisitResult right)
    {
        return left.Id == right.Id && Math.Abs(left.VisitTime - right.VisitTime) < Constants.DoubleComparisonTolerance;
    }

    public static bool operator !=(PointVisitResult left, PointVisitResult right)
    {
        return !(left == right);
    }
}