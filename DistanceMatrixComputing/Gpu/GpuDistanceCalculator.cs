using DistanceMatrixComputing.Metal;

namespace DistanceMatrixComputing.Gpu;

public sealed class GpuDistanceCalculator : DistanceMatrixCalculatorBase, IDisposable
{
    private static readonly Lazy<GpuDistanceCalculator> LazyShared = new(() => new GpuDistanceCalculator());
    public static GpuDistanceCalculator Shared => LazyShared.Value;

    private readonly IDistanceMatrixCalculator _underlying;
    private readonly IDisposable? _disposable;
    private bool _disposed;

    public GpuBackend ActiveBackend { get; }

    public string DeviceName
    {
        get
        {
            if (OperatingSystem.IsMacOS() && _underlying is MetalDistanceCalculator metal)
                return metal.DeviceName;
            if (_underlying is IlgpuDistanceCalculator ilgpu)
                return ilgpu.DeviceName;
            return "GPU Device";
        }
    }

    public GpuDistanceCalculator(GpuBackend backend = GpuBackend.Auto)
    {
        if (backend == GpuBackend.Metal || (backend == GpuBackend.Auto && OperatingSystem.IsMacOS()))
        {
            if (!OperatingSystem.IsMacOS())
                throw new PlatformNotSupportedException("Metal backend is only supported on macOS.");

            var metal = new MetalDistanceCalculator();
            _underlying = metal;
            _disposable = metal;
            ActiveBackend = GpuBackend.Metal;
            return;
        }

        var ilgpu = new IlgpuDistanceCalculator();
        _underlying = ilgpu;
        _disposable = ilgpu;
        ActiveBackend = GpuBackend.Ilgpu;
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

        return _underlying.ComputeDistanceMatrix(points);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _disposable?.Dispose();
    }
}
