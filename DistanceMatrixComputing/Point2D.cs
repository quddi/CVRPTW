namespace DistanceMatrixComputing;

public readonly record struct Point2D(double X, double Y)
{
    public double Latitude => X;
    public double Longitude => Y;

    public static Point2D FromCoordinates(double latitude, double longitude) => new(latitude, longitude);

    public static implicit operator Point2D((double X, double Y) tuple) => new(tuple.X, tuple.Y);
    public static implicit operator (double X, double Y)(Point2D point) => (point.X, point.Y);
}
