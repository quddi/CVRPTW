namespace CVRPTW;

public struct PointVisitResult(int id, double visitTime)
{
    public int Id { get; set; } = id;

    public double VisitTime { get; set; } = visitTime;

    public PointVisitResult() : this(0, 0) { }

    public PointVisitResult(int id) : this(id, 0) { }

    public static bool operator ==(PointVisitResult left, PointVisitResult right) => left.Id == right.Id && 
                                                                                     Math.Abs(left.VisitTime - right.VisitTime) < Constants.DoubleComparisonTolerance;


    public static bool operator !=(PointVisitResult left, PointVisitResult right) => !(left == right);

    public bool Equals(PointVisitResult other) => Id == other.Id && VisitTime.Equals(other.VisitTime);

    public override bool Equals(object? obj) => obj is PointVisitResult other && Equals(other);

    public override string ToString() => $"{Id} {VisitTime}";
    
    public override int GetHashCode() => HashCode.Combine(Id, VisitTime);
}