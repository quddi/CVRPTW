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

    public static double[,] Calculate(IReadOnlyList<Point2D> points, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(points) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(points);

    public static double[,] Calculate(IReadOnlyList<(double X, double Y)> points, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(points) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(points);

    public static double[,] Calculate<T>(IReadOnlyList<T> items, Func<T, Point2D> pointSelector, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(items, pointSelector) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(items, pointSelector);

    public static double[,] Calculate<T>(IReadOnlyList<T> items, Func<T, (double X, double Y)> coordinateSelector, bool useParallel = true) =>
        useParallel ? Shared.ComputeDistanceMatrix(items, coordinateSelector) : new CpuDistanceCalculator(false).ComputeDistanceMatrix(items, coordinateSelector);

    public override double[,] ComputeDistanceMatrix(IReadOnlyList<Point2D> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        int n = points.Count;
        if (n == 0) return new double[0, 0];
        if (n == 1) return new double[1, 1] { { 0.0 } };

        Point2D[] array = points as Point2D[] ?? [.. points];
        var matrix = new double[n, n];

        if (UseParallel && n >= 64)
        {
            Parallel.For(0, n, i =>
            {
                var p1 = array[i];
                matrix[i, i] = 0.0;
                for (int j = i + 1; j < n; j++)
                {
                    var p2 = array[j];
                    double dx = p1.X - p2.X;
                    double dy = p1.Y - p2.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
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
                matrix[i, i] = 0.0;
                for (int j = i + 1; j < n; j++)
                {
                    var p2 = array[j];
                    double dx = p1.X - p2.X;
                    double dy = p1.Y - p2.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    matrix[i, j] = dist;
                    matrix[j, i] = dist;
                }
            }
        }

        return matrix;
    }
}
