using BenchmarkDotNet.Attributes;
using Benchmarks.Common;
using DistanceMatrixComputing;
using DistanceMatrixComputing.Cpu;
using DistanceMatrixComputing.Gpu;
using DistanceMatrixComputing.Metal;

namespace Benchmarks.Cases;

public class DistanceMatrixBenchmark : BenchmarkBase
{
    private Point2D[] _points = null!;
    private CpuDistanceCalculator _cpuSequential = null!;
    private CpuDistanceCalculator _cpuParallel = null!;
    private GpuDistanceCalculator _gpu = null!;
    private IlgpuDistanceCalculator _ilgpu = null!;

    [Params(100, 500, 1000)]
    public int PointsCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _points = TestDataHelper.GeneratePoints(PointsCount);
        _cpuSequential = new CpuDistanceCalculator(useParallel: false);
        _cpuParallel = new CpuDistanceCalculator(useParallel: true);
        _gpu = new GpuDistanceCalculator();
        _ilgpu = new IlgpuDistanceCalculator();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _gpu.Dispose();
        _ilgpu.Dispose();
    }

    [Benchmark(Baseline = true)]
    public float[,] CpuSequential()
    {
        return _cpuSequential.ComputeDistanceMatrix(_points);
    }

    [Benchmark]
    public float[,] CpuParallel()
    {
        return _cpuParallel.ComputeDistanceMatrix(_points);
    }

    [Benchmark]
    public float[,] Gpu()
    {
        return _gpu.ComputeDistanceMatrix(_points);
    }

    [Benchmark]
    public float[,] Ilgpu()
    {
        return _ilgpu.ComputeDistanceMatrix(_points);
    }
}
