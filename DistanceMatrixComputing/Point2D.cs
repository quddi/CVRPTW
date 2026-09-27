namespace DistanceMatrixComputing;

public readonly record struct Point2D(float X, float Y)
{
    public float Latitude => X;
    public float Longitude => Y;

    public static Point2D FromCoordinates(float latitude, float longitude) => new(latitude, longitude);
    public static Point2D FromCoordinates(double latitude, double longitude) => new((float)latitude, (float)longitude);

    public static implicit operator Point2D((float X, float Y) tuple) => new(tuple.X, tuple.Y);
    public static implicit operator (float X, float Y)(Point2D point) => (point.X, point.Y);

    public static implicit operator Point2D((double X, double Y) tuple) => new((float)tuple.X, (float)tuple.Y);
}
