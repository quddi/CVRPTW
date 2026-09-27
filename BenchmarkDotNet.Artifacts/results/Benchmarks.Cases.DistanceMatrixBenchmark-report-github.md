```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.301
  [Host] : .NET 10.0.9 (10.0.9, 10.0.926.27113), Arm64 RyuJIT armv8.0-a
  Dry    : .NET 10.0.9 (10.0.9, 10.0.926.27113), Arm64 RyuJIT armv8.0-a

Job=Dry  IterationCount=1  LaunchCount=1  
RunStrategy=ColdStart  UnrollFactor=1  WarmupCount=1  

```
| Method        | PointsCount | Mean         | Error | Ratio | Rank | Allocated  | Alloc Ratio |
|-------------- |------------ |-------------:|------:|------:|-----:|-----------:|------------:|
| CpuSequential | 100         |     416.7 μs |    NA |  1.00 |    1 |   39.14 KB |        1.00 |
| CpuParallel   | 100         |     631.1 μs |    NA |  1.51 |    2 |   42.37 KB |        1.08 |
| Gpu           | 100         |   2,635.4 μs |    NA |  6.32 |    3 |    39.1 KB |        1.00 |
| Ilgpu         | 100         |   7,880.9 μs |    NA | 18.91 |    4 |   39.86 KB |        1.02 |
|               |             |              |       |       |      |            |             |
| CpuSequential | 500         |     999.0 μs |    NA |  1.00 |    1 |  976.64 KB |        1.00 |
| CpuParallel   | 500         |   1,494.2 μs |    NA |  1.50 |    2 |  980.44 KB |        1.00 |
| Gpu           | 500         |   2,757.1 μs |    NA |  2.76 |    3 |   976.6 KB |        1.00 |
| Ilgpu         | 500         |  35,437.0 μs |    NA | 35.47 |    4 |  977.34 KB |        1.00 |
|               |             |              |       |       |      |            |             |
| CpuSequential | 1000        |   1,583.2 μs |    NA |  1.00 |    1 | 3906.33 KB |        1.00 |
| CpuParallel   | 1000        |   2,353.0 μs |    NA |  1.49 |    2 | 3911.58 KB |        1.00 |
| Gpu           | 1000        |   5,493.9 μs |    NA |  3.47 |    3 | 3906.29 KB |        1.00 |
| Ilgpu         | 1000        | 109,984.1 μs |    NA | 69.47 |    4 | 3907.02 KB |        1.00 |
