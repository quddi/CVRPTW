namespace DistanceMatrixComputing.Cpu;

public sealed class CpuDistanceCalculator : DistanceMatrixCalculatorBase
{
    private static readonly Lazy<CpuDistanceCalculator> LazyShared = new(() => new CpuDistanceCalculator());
    public static CpuDistanceCalculator Shared => LazyShared.Value;

    public bool UseParallel { get; }

    public CpuDistanceCalculator(bool useParallel = true)
    {
        UseParallel = useParallel;
    }

    public static float[,] Calculate(IReadOnlyList<Point2D> points, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(points) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(points);

    public static float[,] Calculate(IReadOnlyList<(float X, float Y)> points, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(points) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(points);

    public static float[,] Calculate(IReadOnlyList<(double X, double Y)> points, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(points) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(points);

    public static float[,] Calculate<T>(IReadOnlyList<T> items, Func<T, Point2D> pointSelector, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(items, pointSelector) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(items, pointSelector);

    public static float[,] Calculate<T>(IReadOnlyList<T> items, Func<T, (float X, float Y)> coordinateSelector, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(items, coordinateSelector) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(items, coordinateSelector);

    public static float[,] Calculate<T>(IReadOnlyList<T> items, Func<T, (double X, double Y)> coordinateSelector, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(items, coordinateSelector) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(items, coordinateSelector);

    public override float[,] ComputeDistanceMatrix(IReadOnlyList<Point2D> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        int n = points.Count;
        if (n == 0) return new float[0, 0];
        if (n == 1) return new float[1, 1] { { 0.0f } };

        Point2D[] array = points as Point2D[] ?? [.. points];
        var matrix = new float[n, n];

        if (UseParallel && n >= 64)
        {
            Parallel.For(0, n, i =>
            {
                var p1 = array[i];
                matrix[i, i] = 0.0f;
                for (int j = i + 1; j < n; j++)
                {
                    var p2 = array[j];
                    float dx = p1.X - p2.X;
                    float dy = p1.Y - p2.Y;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    matrix[i, j] = dist;
                    matrix[j, i] = dist;
                }
            });
        }
        else
        {
            for (int i = 0; i < n; i++)
            {
                var p1 = array[i];
                matrix[i, i] = 0.0f;
                for (int j = i + 1; j < n; j++)
                {
                    var p2 = array[j];
                    float dx = p1.X - p2.X;
                    float dy = p1.Y - p2.Y;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    matrix[i, j] = dist;
                    matrix[j, i] = dist;
                }
            }
        }

        return matrix;
    }
}
