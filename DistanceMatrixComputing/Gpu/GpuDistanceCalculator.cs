using ILGPU;
using ILGPU.Runtime;

namespace DistanceMatrixComputing.Gpu;

public sealed class GpuDistanceCalculator : DistanceMatrixCalculatorBase, IDisposable
{
    private static readonly Lazy<GpuDistanceCalculator> LazyShared = new(() => new GpuDistanceCalculator());
    public static GpuDistanceCalculator Shared => LazyShared.Value;

    private readonly Context _context;
    private readonly Accelerator _accelerator;
    private readonly Action<Index2D, ArrayView1D<Point2D, Stride1D.Dense>, ArrayView2D<double, Stride2D.DenseY>> _kernel;
    private readonly object _lock = new();
    private bool _disposed;

    public string DeviceName => _accelerator.Name;
    public AcceleratorType AcceleratorType => _accelerator.AcceleratorType;

    public GpuDistanceCalculator(bool preferCpu = false)
    {
        _context = Context.CreateDefault();
        var device = _context.GetPreferredDevice(preferCPU: preferCpu);
        _accelerator = device.CreateAccelerator(_context);
        _kernel = _accelerator.LoadAutoGroupedStreamKernel<
            Index2D,
            ArrayView1D<Point2D, Stride1D.Dense>,
            ArrayView2D<double, Stride2D.DenseY>>(ComputeDistanceMatrixKernel);
    }

    public static double[,] Calculate(IReadOnlyList<Point2D> points) =>
        Shared.ComputeDistanceMatrix(points);

    public static double[,] Calculate(IReadOnlyList<(double X, double Y)> points) =>
        Shared.ComputeDistanceMatrix(points);

    public static double[,] Calculate<T>(IReadOnlyList<T> items, Func<T, Point2D> pointSelector) =>
        Shared.ComputeDistanceMatrix(items, pointSelector);

    public static double[,] Calculate<T>(IReadOnlyList<T> items, Func<T, (double X, double Y)> coordinateSelector) =>
        Shared.ComputeDistanceMatrix(items, coordinateSelector);

    public override double[,] ComputeDistanceMatrix(IReadOnlyList<Point2D> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        ThrowIfDisposed();

        int n = points.Count;
        if (n == 0) return new double[0, 0];
        if (n == 1) return new double[1, 1] { { 0.0 } };

        Point2D[] array = points as Point2D[] ?? [.. points];
        return ComputeInternal(array);
    }

    private double[,] ComputeInternal(Point2D[] points)
    {
        int n = points.Length;
        lock (_lock)
        {
            using var bufPoints = _accelerator.Allocate1D<Point2D>(n);
            using var bufDist = _accelerator.Allocate2DDenseY<double>(new Index2D(n, n));

            bufPoints.CopyFromCPU(points);
            _kernel(new Index2D(n, n), bufPoints.View, bufDist.View);
            _accelerator.Synchronize();

            return bufDist.GetAsArray2D();
        }
    }

    private static void ComputeDistanceMatrixKernel(
        Index2D index,
        ArrayView1D<Point2D, Stride1D.Dense> points,
        ArrayView2D<double, Stride2D.DenseY> distances)
    {
        int i = index.X;
        int j = index.Y;
        var p1 = points[i];
        var p2 = points[j];
        double dx = p1.X - p2.X;
        double dy = p1.Y - p2.Y;
        distances[index] = Math.Sqrt(dx * dx + dy * dy);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _accelerator.Dispose();
        _context.Dispose();
    }
}
