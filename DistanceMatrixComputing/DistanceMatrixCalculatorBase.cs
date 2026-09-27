namespace DistanceMatrixComputing;

public abstract class DistanceMatrixCalculatorBase : IDistanceMatrixCalculator
{
    public abstract float[,] ComputeDistanceMatrix(IReadOnlyList<Point2D> points);

    public float[,] ComputeDistanceMatrix(IReadOnlyList<(float X, float Y)> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        var converted = new Point2D[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            var (x, y) = points[i];
            converted[i] = new Point2D(x, y);
        }
        return ComputeDistanceMatrix(converted);
    }

    public float[,] ComputeDistanceMatrix(IReadOnlyList<(double X, double Y)> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        var converted = new Point2D[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            var (x, y) = points[i];
            converted[i] = new Point2D((float)x, (float)y);
        }
        return ComputeDistanceMatrix(converted);
    }

    public float[,] ComputeDistanceMatrix<T>(IReadOnlyList<T> items, Func<T, Point2D> pointSelector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(pointSelector);

        var converted = new Point2D[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            converted[i] = pointSelector(items[i]);
        }
        return ComputeDistanceMatrix(converted);
    }

    public float[,] ComputeDistanceMatrix<T>(IReadOnlyList<T> items, Func<T, (float X, float Y)> coordinateSelector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(coordinateSelector);

        var converted = new Point2D[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            var (x, y) = coordinateSelector(items[i]);
            converted[i] = new Point2D(x, y);
        }
        return ComputeDistanceMatrix(converted);
    }

    public float[,] ComputeDistanceMatrix<T>(IReadOnlyList<T> items, Func<T, (double X, double Y)> coordinateSelector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(coordinateSelector);

        var converted = new Point2D[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            var (x, y) = coordinateSelector(items[i]);
            converted[i] = new Point2D((float)x, (float)y);
        }
        return ComputeDistanceMatrix(converted);
    }
}
