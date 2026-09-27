using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;

namespace DistanceMatrixComputing.Gpu;

public sealed class IlgpuDistanceCalculator : DistanceMatrixCalculatorBase, IDisposable
{
    private static readonly Lazy<IlgpuDistanceCalculator> LazyShared = new(() => new IlgpuDistanceCalculator());
    public static IlgpuDistanceCalculator Shared => LazyShared.Value;

    private readonly Context _context;
    private readonly Accelerator _accelerator;
    private readonly Action<Index2D, ArrayView1D<Point2D, Stride1D.Dense>, ArrayView2D<float, Stride2D.DenseY>> _kernel;
    private readonly object _lock = new();
    private bool _disposed;

    public string DeviceName => _accelerator.Name;
    public AcceleratorType AcceleratorType => _accelerator.AcceleratorType;

    public IlgpuDistanceCalculator(bool preferCpu = false)
    {
        _context = Context.Create(builder =>
        {
            try { builder.OpenCL(); } catch { }
            try { builder.Cuda(); } catch { }
            builder.CPU();
            builder.Optimize(OptimizationLevel.O2);
        });

        Device device = SelectDevice(_context, preferCpu);
        _accelerator = device.CreateAccelerator(_context);
        _kernel = _accelerator.LoadAutoGroupedStreamKernel<
            Index2D,
            ArrayView1D<Point2D, Stride1D.Dense>,
            ArrayView2D<float, Stride2D.DenseY>>(ComputeDistanceMatrixKernel);
    }

    private static Device SelectDevice(Context context, bool preferCpu)
    {
        if (preferCpu)
            return context.GetPreferredDevice(preferCPU: true);

        // 1. CUDA (NVIDIA GPU - fastest when available)
        var cudaDevices = context.GetCudaDevices();
        if (cudaDevices.Count > 0)
            return cudaDevices[0];

        // 2. OpenCL (AMD / Intel / NVIDIA GPU)
        var clDevices = context.GetCLDevices();
        if (clDevices.Count > 0)
            return clDevices[0];

        // 3. Fallback (CPU accelerator or default)
        return context.GetPreferredDevice(preferCPU: false);
    }

    public static float[,] Calculate(IReadOnlyList<Point2D> points) =>
        Shared.ComputeDistanceMatrix(points);

    public static float[,] Calculate(IReadOnlyList<(float X, float Y)> points) =>
        Shared.ComputeDistanceMatrix(points);

    public static float[,] Calculate(IReadOnlyList<(double X, double Y)> points) =>
        Shared.ComputeDistanceMatrix(points);

    public static float[,] Calculate<T>(IReadOnlyList<T> items, Func<T, Point2D> pointSelector) =>
        Shared.ComputeDistanceMatrix(items, pointSelector);

    public static float[,] Calculate<T>(IReadOnlyList<T> items, Func<T, (float X, float Y)> coordinateSelector) =>
        Shared.ComputeDistanceMatrix(items, coordinateSelector);

    public static float[,] Calculate<T>(IReadOnlyList<T> items, Func<T, (double X, double Y)> coordinateSelector) =>
        Shared.ComputeDistanceMatrix(items, coordinateSelector);

    public override float[,] ComputeDistanceMatrix(IReadOnlyList<Point2D> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        ThrowIfDisposed();

        int n = points.Count;
        if (n == 0) return new float[0, 0];
        if (n == 1) return new float[1, 1] { { 0.0f } };

        Point2D[] array = points as Point2D[] ?? [.. points];
        return ComputeInternal(array);
    }

    private float[,] ComputeInternal(Point2D[] points)
    {
        int n = points.Length;
        lock (_lock)
        {
            using var bufPoints = _accelerator.Allocate1D<Point2D>(n);
            using var bufDist = _accelerator.Allocate2DDenseY<float>(new Index2D(n, n));

            bufPoints.CopyFromCPU(points);
            _kernel(new Index2D(n, n), bufPoints.View, bufDist.View);
            _accelerator.Synchronize();

            return bufDist.GetAsArray2D();
        }
    }

    private static void ComputeDistanceMatrixKernel(
        Index2D index,
        ArrayView1D<Point2D, Stride1D.Dense> points,
        ArrayView2D<float, Stride2D.DenseY> distances)
    {
        int i = index.X;
        int j = index.Y;
        var p1 = points[i];
        var p2 = points[j];
        float dx = p1.X - p2.X;
        float dy = p1.Y - p2.Y;
        distances[index] = MathF.Sqrt(dx * dx + dy * dy);
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
