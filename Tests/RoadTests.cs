using CVRPTW;
using OsmSharp;
using OsmSharp.Tags;
using Xunit;

namespace Tests;

// Мініатюрна дорожня мережа (координати в градусах), усі дороги двосторонні:
//
//   D(2) ----- E(5) ---- F(6)      A(1)=(50.000, 30.000)  B(2)=(50.000, 30.001)  C(3)=(50.000, 30.002)
//    |          |         |        D(4)=(50.002, 30.000)  E(5)=(50.001, 30.001)  F(6)=(50.001, 30.002)
//    A -------- B ------- C
//
// Дороги: A–B, B–C, D–E–F, A–D, B–E, C–F; окремо лежить ізольована дорога G–H далеко від решти.
// У вузлах A, C, D, F степінь 2, у B і E — 3, у G і H — 1. Ребер 16 (8 ділянок × 2 напрями), переходів Σ степінь² = 36.
public class RoadTests
{
    private const double R = 6371000;

    private static readonly RoadMatrixPoint A = new(1, 50.000, 30.000);
    private static readonly RoadMatrixPoint E = new(2, 50.001, 30.001);
    private static readonly RoadMatrixPoint F = new(3, 50.001, 30.002);

    [Fact]
    public void M1_BuildsExpectedEdgesAndTransitions()
    {
        var graph = RoadLineGraph.Build(BuildNetwork(withRestriction: false));

        Assert.Equal(16, graph.VertexCount);
        Assert.Equal(36, graph.TransitionCount);
    }

    [Fact]
    public void M1_RemovesForbiddenTurn()
    {
        var network = BuildNetwork(withRestriction: true);
        var graph = RoadLineGraph.Build(network);

        // Заборона «з A–B у B–E» прибирає рівно один перехід.
        Assert.Equal(1, network.RestrictionsApplied);
        Assert.Equal(1, network.ForbiddenTurns.Length);
        Assert.Equal(35, graph.TransitionCount);
    }

    [Fact]
    public void M2_MatrixRespectsDistancesAndForbiddenTurn()
    {
        var points = new[] { A, E, F };

        var free = CreateCalculator(withRestriction: false).Calculate(points);
        var restricted = CreateCalculator(withRestriction: true).Calculate(points);

        var direct = Meters(50.000, 30.000, 50.000, 30.001) + Meters(50.000, 30.001, 50.001, 30.001);
        // Без заборони A → B → E; із забороною повороту B → E з боку A: A → B → C → F → E.
        var detour = Meters(50.000, 30.000, 50.000, 30.002) + Meters(50.000, 30.002, 50.001, 30.002) + Meters(50.001, 30.002, 50.001, 30.001);

        Assert.Equal(direct, free.Values[0, 1]!.Value, 0.2);
        Assert.Equal(detour, restricted.Values[0, 1]!.Value, 0.2);
        Assert.Equal(0, free.Values[1, 1]!.Value, 0.001);
        // Зворотний напрям E → B → A заборона не зачіпає (поворот з B–E у A–B дозволений).
        Assert.Equal(direct, restricted.Values[1, 0]!.Value, 0.2);
    }

    [Fact]
    public void M2_PointInsideEdge_UsesFractionOfEdge()
    {
        var points = new[] { new RoadMatrixPoint(1, 50.000, 30.0005), new RoadMatrixPoint(2, 50.000, 30.0015) };

        var matrix = CreateCalculator(withRestriction: false).Calculate(points);

        // Півребра A–B плюс півребра B–C = 0,001° довготи.
        Assert.Equal(Meters(50.000, 30.0005, 50.000, 30.0015), matrix.Values[0, 1]!.Value, 0.2);
    }

    [Fact]
    public void M2_UnreachablePairIsNull()
    {
        var points = new[] { A, new RoadMatrixPoint(2, 51.0005, 31.000) };

        var matrix = CreateCalculator(withRestriction: false).Calculate(points);

        Assert.Null(matrix.Values[0, 1]);
        Assert.Null(matrix.Values[1, 0]);
    }

    [Fact]
    public void M3_PathMatchesMatrixAndEndsAtPoints()
    {
        var calculator = CreateCalculator(withRestriction: true);
        var matrix = calculator.Calculate([A, E]);

        var path = calculator.FindPath(A, E);

        Assert.NotNull(path);
        Assert.Equal(matrix.Values[0, 1]!.Value, path.DistanceMeters, 0.5);
        Assert.Equal(A.Latitude, path.Latitudes[0], 6);
        Assert.Equal(A.Longitude, path.Longitudes[0], 6);
        Assert.Equal(E.Latitude, path.Latitudes[^1], 6);
        Assert.Equal(E.Longitude, path.Longitudes[^1], 6);
        // Об'їзд через C і F: шлях проходить через C.
        Assert.Contains(path.Longitudes.Zip(path.Latitudes), p => Math.Abs(p.First - 30.002) < 1e-6 && Math.Abs(p.Second - 50.000) < 1e-6);
    }

    private static RoadMatrixCalculator CreateCalculator(bool withRestriction)
    {
        var network = BuildNetwork(withRestriction);

        return new RoadMatrixCalculator(RoadLineGraph.Build(network), new RoadSpatialIndex(network));
    }

    private static RoadNetwork BuildNetwork(bool withRestriction)
    {
        var elements = new List<OsmGeo>
        {
            Node(1, 50.000, 30.000), Node(2, 50.000, 30.001), Node(3, 50.000, 30.002),
            Node(4, 50.002, 30.000), Node(5, 50.001, 30.001), Node(6, 50.001, 30.002),
            Node(7, 51.000, 31.000), Node(8, 51.001, 31.000),
            Way(10, 1, 2), Way(15, 2, 3), Way(11, 4, 5, 6), Way(12, 1, 4), Way(13, 2, 5), Way(14, 3, 6), Way(20, 7, 8)
        };

        if (withRestriction)
        {
            elements.Add(new Relation
            {
                Id = 100,
                Tags = new TagsCollection(new Tag("type", "restriction"), new Tag("restriction", "no_left_turn")),
                Members =
                [
                    new RelationMember { Id = 10, Role = "from", Type = OsmGeoType.Way },
                    new RelationMember { Id = 2, Role = "via", Type = OsmGeoType.Node },
                    new RelationMember { Id = 13, Role = "to", Type = OsmGeoType.Way }
                ]
            });
        }

        return RoadNetworkBuilder.Build(() => elements);
    }

    private static Node Node(long id, double latitude, double longitude) => new() { Id = id, Latitude = latitude, Longitude = longitude };

    private static Way Way(long id, params long[] nodes) => new() { Id = id, Nodes = nodes, Tags = new TagsCollection(new Tag("highway", "residential")) };

    private static double Meters(double lat1, double lon1, double lat2, double lon2)
    {
        var p1 = lat1 * Math.PI / 180;
        var p2 = lat2 * Math.PI / 180;
        var dLat = p2 - p1;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(p1) * Math.Cos(p2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return 2 * R * Math.Asin(Math.Sqrt(a));
    }
}
