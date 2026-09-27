using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SharpMetal.Foundation;
using SharpMetal.Metal;

namespace DistanceMatrixComputing.Metal;

[SupportedOSPlatform("macos")]
public sealed class MetalDistanceCalculator : DistanceMatrixCalculatorBase, IDisposable
{
    private static readonly Lazy<MetalDistanceCalculator> LazyShared = new(() => new MetalDistanceCalculator());
    public static MetalDistanceCalculator Shared => LazyShared.Value;

    public static bool IsSupported => OperatingSystem.IsMacOS();

    private readonly MTLDevice _device;
    private readonly MTLComputePipelineState _pipelineState;
    private readonly MTLCommandQueue _commandQueue;
    private readonly object _lock = new();
    private bool _disposed;

    private MTLBuffer? _pointsBuffer;
    private MTLBuffer? _matrixBuffer;
    private MTLBuffer? _nBuffer;
    private int _allocatedCapacity;

    public string DeviceName => _device.Name.ToString() ?? "Apple Metal Device";

    private const string ShaderSource = @"
#include <metal_stdlib>
using namespace metal;

struct Point2D {
    float x;
    float y;
};

kernel void compute_distances(
    device const Point2D* points [[buffer(0)]],
    device float* matrix [[buffer(1)]],
    constant uint& n [[buffer(2)]],
    uint2 id [[thread_position_in_grid]])
{
    if (id.x >= n || id.y >= n) return;
    Point2D p1 = points[id.x];
    Point2D p2 = points[id.y];
    float dx = p1.x - p2.x;
    float dy = p1.y - p2.y;
    matrix[id.y * n + id.x] = sqrt(dx * dx + dy * dy);
}
";

    public MetalDistanceCalculator()
    {
        if (!OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("MetalDistanceCalculator is only supported on macOS.");

        _device = MTLDevice.CreateSystemDefaultDevice();
        if (_device.NativePtr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get default Metal device.");

        NSString source = ShaderSource;
        var compileOptions = new MTLCompileOptions();
        var error = new NSError();
        var library = _device.NewLibrary(source, compileOptions, ref error);
        if (library.NativePtr == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to compile Metal shader: {error.LocalizedDescription}");

        NSString functionName = "compute_distances";
        var function = library.NewFunction(functionName);
        _pipelineState = _device.NewComputePipelineState(function, ref error);
        if (_pipelineState.NativePtr == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to create Metal compute pipeline: {error.LocalizedDescription}");

        _commandQueue = _device.NewCommandQueue();
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
        ulong pointsByteLength = (ulong)n * sizeof(float) * 2;
        ulong matrixByteLength = (ulong)n * (ulong)n * sizeof(float);

        lock (_lock)
        {
            EnsureBuffers(n);

            var pointsBuffer = _pointsBuffer!.Value;
            var matrixBuffer = _matrixBuffer!.Value;
            var nBuffer = _nBuffer!.Value;

            unsafe
            {
                fixed (Point2D* pPoints = points)
                {
                    Buffer.MemoryCopy(pPoints, (void*)pointsBuffer.Contents, pointsByteLength, pointsByteLength);
                }
            }
            Marshal.WriteInt32(nBuffer.Contents, n);

            var commandBuffer = _commandQueue.CommandBuffer();
            var encoder = commandBuffer.ComputeCommandEncoder();

            encoder.SetComputePipelineState(_pipelineState);
            encoder.SetBuffer(pointsBuffer, 0, 0);
            encoder.SetBuffer(matrixBuffer, 0, 1);
            encoder.SetBuffer(nBuffer, 0, 2);

            var gridSize = new MTLSize { width = (ulong)n, height = (ulong)n, depth = 1 };
            ulong tgW = Math.Min((ulong)n, 16);
            ulong tgH = Math.Min((ulong)n, 16);
            var threadgroupSize = new MTLSize { width = tgW, height = tgH, depth = 1 };

            encoder.DispatchThreads(gridSize, threadgroupSize);
            encoder.EndEncoding();

            commandBuffer.Commit();
            commandBuffer.WaitUntilCompleted();

            var matrix = new float[n, n];
            unsafe
            {
                fixed (float* pMatrix = matrix)
                {
                    Buffer.MemoryCopy((void*)matrixBuffer.Contents, pMatrix, matrixByteLength, matrixByteLength);
                }
            }

            return matrix;
        }
    }

    private void EnsureBuffers(int n)
    {
        if (_allocatedCapacity >= n && _pointsBuffer.HasValue && _matrixBuffer.HasValue && _nBuffer.HasValue)
            return;

        if (_pointsBuffer.HasValue) _pointsBuffer.Value.Dispose();
        if (_matrixBuffer.HasValue) _matrixBuffer.Value.Dispose();
        if (_nBuffer.HasValue) _nBuffer.Value.Dispose();

        _allocatedCapacity = Math.Max(n, 256);
        ulong pointsByteLength = (ulong)_allocatedCapacity * sizeof(float) * 2;
        ulong matrixByteLength = (ulong)_allocatedCapacity * (ulong)_allocatedCapacity * sizeof(float);

        _pointsBuffer = _device.NewBuffer(pointsByteLength, MTLResourceOptions.ResourceStorageModeShared);
        _matrixBuffer = _device.NewBuffer(matrixByteLength, MTLResourceOptions.ResourceStorageModeShared);
        _nBuffer = _device.NewBuffer(sizeof(uint), MTLResourceOptions.ResourceStorageModeShared);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        lock (_lock)
        {
            if (_pointsBuffer.HasValue) _pointsBuffer.Value.Dispose();
            if (_matrixBuffer.HasValue) _matrixBuffer.Value.Dispose();
            if (_nBuffer.HasValue) _nBuffer.Value.Dispose();
            _commandQueue.Dispose();
            _pipelineState.Dispose();
            _device.Dispose();
        }
    }
}
