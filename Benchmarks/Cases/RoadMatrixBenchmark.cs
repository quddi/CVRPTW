using BenchmarkDotNet.Attributes;
using Benchmarks.Common;
using CVRPTW;

namespace Benchmarks.Cases;

/// <summary>
/// Дорожня матриця (M2) і шляхи (M3) на реальній мережі OSM України: файл і кеш графа беруться з тієї самої папки,
/// що й у застосунку (~/Library/Application Support/CVRPTW). Точки випадкові, не далі 300 м від дороги.
/// </summary>
[SimpleJob(warmupCount: 0, iterationCount: 3, invocationCount: 1)]
public class RoadMatrixBenchmark : BenchmarkBase
{
    private const double MaxSnapMeters = 300;

    private static readonly Dictionary<string, (double MinLat, double MinLon, double MaxLat, double MaxLon)> Areas = new()
    {
        ["Kyiv"] = (50.2, 30.2, 50.7, 30.9),
        ["Ukraine"] = (44.4, 22.1, 52.4, 40.2)
    };

    private RoadMatrixCalculator _calculator = null!;
    private RoadMatrixPoint[] _points = null!;
    private RoadMatrixPoint[] _route = null!;

    [Params("Kyiv", "Ukraine")]
    public string Area { get; set; } = "";

    [Params(100, 500, 1000)]
    public int PointsCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CVRPTW");
        var pbfPath = Path.Combine(dataDirectory, "osm", "ukraine-latest.osm.pbf");

        var network = RoadNetworkCache.LoadOrBuild(pbfPath, dataDirectory);
        var graph = RoadLineGraph.Build(network);
        var index = new RoadSpatialIndex(network, edgeMask: graph.GetLargestStronglyConnectedComponent());

        _calculator = new RoadMatrixCalculator(graph, index);
        _points = GeneratePoints(index, Areas[Area], PointsCount);
        _route = BuildGreedyRoute(_points, _calculator.Calculate(_points));
    }

    [Benchmark]
    public RoadDistanceMatrix MatrixM2() => _calculator.Calculate(_points);

    // Шляхи між сусідніми точками жадібного маршруту (найближчий сусід): відрізки, що малює «Маршрути на карті».
    [Benchmark]
    public int PathsM3()
    {
        var found = 0;

        for (var i = 0; i < _route.Length - 1; i++)
            if (_calculator.FindPath(_route[i], _route[i + 1]) != null) found++;

        return found;
    }

    private static RoadMatrixPoint[] BuildGreedyRoute(RoadMatrixPoint[] points, RoadDistanceMatrix matrix)
    {
        var route = new List<RoadMatrixPoint> { points[0] };
        var visited = new bool[points.Length];
        visited[0] = true;
        var current = 0;

        for (var step = 1; step < points.Length; step++)
        {
            var next = -1;

            for (var candidate = 0; candidate < points.Length; candidate++)
            {
                if (visited[candidate] || matrix.Values[current, candidate] is not { } distance) continue;
                if (next == -1 || distance < matrix.Values[current, next]!.Value) next = candidate;
            }

            if (next == -1) break;

            visited[next] = true;
            route.Add(points[next]);
            current = next;
        }

        return route.ToArray();
    }

    private static RoadMatrixPoint[] GeneratePoints(RoadSpatialIndex index, (double MinLat, double MinLon, double MaxLat, double MaxLon) area, int count)
    {
        var random = new Random(42);
        var points = new List<RoadMatrixPoint>(count);

        while (points.Count < count)
        {
            var latitude = area.MinLat + random.NextDouble() * (area.MaxLat - area.MinLat);
            var longitude = area.MinLon + random.NextDouble() * (area.MaxLon - area.MinLon);

            if (index.TryFindNearest(latitude, longitude, out var snap) && snap.DistanceMeters <= MaxSnapMeters)
                points.Add(new RoadMatrixPoint(points.Count, latitude, longitude));
        }

        return points.ToArray();
    }
}
