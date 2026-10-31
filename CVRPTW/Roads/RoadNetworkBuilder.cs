using OsmSharp;
using OsmSharp.Streams;

namespace CVRPTW;

public static class RoadNetworkBuilder
{
    private const double EarthRadiusMeters = 6371000;

    private enum RestrictionKind { No, Only }

    private enum RestrictionTurn { Other, UTurn }

    private readonly record struct RawWay(long Id, long[] Nodes, RoadDirection Direction);

    private readonly record struct RawRestriction(RestrictionKind Kind, RestrictionTurn Turn, long FromWay, long ViaNode, long ToWay);

    public static RoadNetwork BuildFromPbf(string path, IProgress<string>? progress = null)
    {
        return Build(() => ReadPbf(path), progress);
    }

    private static IEnumerable<OsmGeo> ReadPbf(string path)
    {
        using var stream = File.OpenRead(path);

        foreach (var geo in new PBFOsmStreamSource(stream))
            yield return geo;
    }

    // openSource викликається двічі: перший прохід збирає дороги й обмеження, другий — координати вузлів.
    public static RoadNetwork Build(Func<IEnumerable<OsmGeo>> openSource, IProgress<string>? progress = null)
    {
        progress?.Report("Читання доріг і заборон поворотів...");

        var ways = new List<RawWay>();
        var restrictions = new List<RawRestriction>();
        var unsupported = 0;

        foreach (var geo in openSource())
        {
            switch (geo)
            {
                case Way way when way.Id != null && way.Nodes is { Length: >= 2 }:
                    var direction = CarRoadProfile.GetDirection(way.Tags);
                    if (direction != RoadDirection.None) ways.Add(new RawWay(way.Id.Value, way.Nodes, direction));
                    break;
                case Relation relation:
                    var parsed = TryParseRestriction(relation, out var restriction);
                    if (parsed == ParseResult.Parsed) restrictions.Add(restriction);
                    else if (parsed == ParseResult.Unsupported) unsupported++;
                    break;
            }
        }

        progress?.Report("Підготовка вузлів...");

        var (nodeIds, nodeUses) = CollectNodes(ways);

        foreach (var restriction in restrictions)
        {
            // Вузол обмеження має бути перехрестям, щоб ділянки доріг закінчувалися в ньому.
            var index = Array.BinarySearch(nodeIds, restriction.ViaNode);
            if (index >= 0) nodeUses[index] = byte.MaxValue;
        }

        progress?.Report("Читання координат вузлів...");

        var latitudes = new int[nodeIds.Length];
        var longitudes = new int[nodeIds.Length];
        var hasCoordinates = new bool[nodeIds.Length];

        foreach (var geo in openSource())
        {
            if (geo is not Node { Id: not null, Latitude: not null, Longitude: not null } node) continue;

            var index = Array.BinarySearch(nodeIds, node.Id.Value);
            if (index < 0) continue;

            latitudes[index] = (int)Math.Round(node.Latitude.Value * RoadNetwork.CoordinateScale);
            longitudes[index] = (int)Math.Round(node.Longitude.Value * RoadNetwork.CoordinateScale);
            hasCoordinates[index] = true;
        }

        progress?.Report("Побудова графа доріг...");

        return BuildGraph(ways, restrictions, unsupported, nodeIds, nodeUses, latitudes, longitudes, hasCoordinates);
    }

    private enum ParseResult { Ignored, Parsed, Unsupported }

    private static ParseResult TryParseRestriction(Relation relation, out RawRestriction restriction)
    {
        restriction = default;

        if (!relation.Tags.TryGetValue("type", out var type) || type != "restriction")
            return ParseResult.Ignored;

        string? value = null;
        foreach (var key in new[] { "restriction", "restriction:motorcar", "restriction:motor_vehicle" })
        {
            if (relation.Tags.TryGetValue(key, out value)) break;
        }

        if (value == null || !CarRoadProfile.AppliesToCars(relation.Tags) || relation.Members == null)
            return ParseResult.Ignored;

        var kind = value.StartsWith("no_") ? RestrictionKind.No : value.StartsWith("only_") ? RestrictionKind.Only : (RestrictionKind?)null;
        var supported = value is "no_left_turn" or "no_right_turn" or "no_straight_on" or "no_u_turn"
            or "only_left_turn" or "only_right_turn" or "only_straight_on" or "only_u_turn";

        if (kind == null || !supported) return ParseResult.Unsupported;

        long? from = null, via = null, to = null;
        var viaIsWay = false;

        foreach (var member in relation.Members)
        {
            switch (member.Role)
            {
                case "from" when member.Type == OsmGeoType.Way: from = member.Id; break;
                case "to" when member.Type == OsmGeoType.Way: to = member.Id; break;
                case "via" when member.Type == OsmGeoType.Node: via = member.Id; break;
                case "via": viaIsWay = true; break;
            }
        }

        if (viaIsWay) return ParseResult.Unsupported;

        if (from == null || via == null || to == null) return ParseResult.Ignored;

        restriction = new RawRestriction(kind.Value, value.EndsWith("u_turn") ? RestrictionTurn.UTurn : RestrictionTurn.Other, from.Value, via.Value, to.Value);

        return ParseResult.Parsed;
    }

    // Повертає відсортовані унікальні id вузлів доріг і кількість їх використань (з обмеженням у 255).
    private static (long[] Ids, byte[] Uses) CollectNodes(List<RawWay> ways)
    {
        var all = new long[ways.Sum(way => way.Nodes.Length)];
        var position = 0;

        foreach (var way in ways)
        {
            way.Nodes.CopyTo(all, position);
            position += way.Nodes.Length;
        }

        Array.Sort(all);

        var ids = new List<long>();
        var uses = new List<byte>();

        foreach (var id in all)
        {
            if (ids.Count > 0 && ids[^1] == id)
                uses[^1] = (byte)Math.Min(uses[^1] + 1, byte.MaxValue);
            else
            {
                ids.Add(id);
                uses.Add(1);
            }
        }

        return (ids.ToArray(), uses.ToArray());
    }

    private static RoadNetwork BuildGraph(List<RawWay> ways, List<RawRestriction> restrictions, int unsupported,
        long[] nodeIds, byte[] nodeUses, int[] latitudes, int[] longitudes, bool[] hasCoordinates)
    {
        var restrictionWays = restrictions.SelectMany(restriction => new[] { restriction.FromWay, restriction.ToWay }).ToHashSet();
        var wayEdges = new Dictionary<long, List<int>>();

        var graphNodeByIndex = new Dictionary<int, int>();
        var nodeLatitudes = new List<int>();
        var nodeLongitudes = new List<int>();

        var edgeFrom = new List<int>();
        var edgeTo = new List<int>();
        var edgeLength = new List<float>();
        var edgeReverse = new List<int>();
        var geometryStart = new List<int>();
        var geometryLatitudes = new List<int>();
        var geometryLongitudes = new List<int>();

        int GetGraphNode(int index)
        {
            if (graphNodeByIndex.TryGetValue(index, out var node)) return node;

            node = nodeLatitudes.Count;
            graphNodeByIndex[index] = node;
            nodeLatitudes.Add(latitudes[index]);
            nodeLongitudes.Add(longitudes[index]);

            return node;
        }

        int AddEdge(int from, int to, List<int> points, bool reversed, long wayId)
        {
            var edge = edgeFrom.Count;
            var length = 0.0;

            geometryStart.Add(geometryLatitudes.Count);

            for (var i = 0; i < points.Count; i++)
            {
                var point = points[reversed ? points.Count - 1 - i : i];
                geometryLatitudes.Add(latitudes[point]);
                geometryLongitudes.Add(longitudes[point]);

                if (i > 0)
                {
                    var previous = points[reversed ? points.Count - i : i - 1];
                    length += GetDistanceMeters(latitudes[previous], longitudes[previous], latitudes[point], longitudes[point]);
                }
            }

            edgeFrom.Add(from);
            edgeTo.Add(to);
            edgeLength.Add((float)length);
            edgeReverse.Add(-1);

            if (restrictionWays.Contains(wayId))
            {
                if (!wayEdges.TryGetValue(wayId, out var list)) wayEdges[wayId] = list = new List<int>();
                list.Add(edge);
            }

            return edge;
        }

        void AddSegment(List<int> points, RoadDirection direction, long wayId)
        {
            if (points.Count < 2) return;

            var from = GetGraphNode(points[0]);
            var to = GetGraphNode(points[^1]);
            var forward = direction is RoadDirection.Both or RoadDirection.Forward ? AddEdge(from, to, points, false, wayId) : -1;
            var backward = direction is RoadDirection.Both or RoadDirection.Backward ? AddEdge(to, from, points, true, wayId) : -1;

            if (forward < 0 || backward < 0) return;

            edgeReverse[forward] = backward;
            edgeReverse[backward] = forward;
        }

        foreach (var way in ways)
        {
            var segment = new List<int>();

            foreach (var nodeId in way.Nodes)
            {
                var index = Array.BinarySearch(nodeIds, nodeId);

                // Вузол поза витягом розриває дорогу.
                if (index < 0 || !hasCoordinates[index])
                {
                    segment.Clear();
                    continue;
                }

                segment.Add(index);

                var isEnd = nodeUses[index] >= 2 || nodeId == way.Nodes[^1];
                if (!isEnd || segment.Count < 2) continue;

                AddSegment(segment, way.Direction, way.Id);
                segment = new List<int> { index };
            }
        }

        geometryStart.Add(geometryLatitudes.Count);

        var (forbidden, applied) = ResolveRestrictions(restrictions, wayEdges, graphNodeByIndex, nodeIds, edgeFrom, edgeTo, edgeReverse);

        return new RoadNetwork
        {
            NodeLatitudes = nodeLatitudes.ToArray(),
            NodeLongitudes = nodeLongitudes.ToArray(),
            EdgeFrom = edgeFrom.ToArray(),
            EdgeTo = edgeTo.ToArray(),
            EdgeLength = edgeLength.ToArray(),
            EdgeReverse = edgeReverse.ToArray(),
            EdgeGeometryStart = geometryStart.ToArray(),
            GeometryLatitudes = geometryLatitudes.ToArray(),
            GeometryLongitudes = geometryLongitudes.ToArray(),
            ForbiddenTurns = forbidden,
            RestrictionsApplied = applied,
            RestrictionsUnsupported = unsupported
        };
    }

    private static (long[] Forbidden, int Applied) ResolveRestrictions(List<RawRestriction> restrictions,
        Dictionary<long, List<int>> wayEdges, Dictionary<int, int> graphNodeByIndex, long[] nodeIds,
        List<int> edgeFrom, List<int> edgeTo, List<int> edgeReverse)
    {
        var forbidden = new HashSet<long>();
        var applied = 0;

        var viaNodes = restrictions.Where(restriction => restriction.Kind == RestrictionKind.Only)
            .Select(restriction => Array.BinarySearch(nodeIds, restriction.ViaNode))
            .Where(index => index >= 0 && graphNodeByIndex.ContainsKey(index))
            .Select(index => graphNodeByIndex[index])
            .ToHashSet();

        var outgoing = new Dictionary<int, List<int>>();

        for (var edge = 0; edge < edgeFrom.Count; edge++)
        {
            if (!viaNodes.Contains(edgeFrom[edge])) continue;

            if (!outgoing.TryGetValue(edgeFrom[edge], out var list)) outgoing[edgeFrom[edge]] = list = new List<int>();
            list.Add(edge);
        }

        foreach (var restriction in restrictions)
        {
            var viaIndex = Array.BinarySearch(nodeIds, restriction.ViaNode);
            if (viaIndex < 0 || !graphNodeByIndex.TryGetValue(viaIndex, out var via)) continue;

            if (!wayEdges.TryGetValue(restriction.FromWay, out var fromWayEdges) ||
                !wayEdges.TryGetValue(restriction.ToWay, out var toWayEdges)) continue;

            var fromEdges = fromWayEdges.Where(edge => edgeTo[edge] == via).ToList();
            var toEdges = toWayEdges.Where(edge => edgeFrom[edge] == via).ToList();

            if (fromEdges.Count == 0 || toEdges.Count == 0) continue;

            foreach (var fromEdge in fromEdges)
            {
                if (restriction.Kind == RestrictionKind.No)
                {
                    var targets = restriction.Turn == RestrictionTurn.UTurn
                        ? toEdges.Where(edge => edge == edgeReverse[fromEdge])
                        : toEdges;

                    foreach (var toEdge in targets) forbidden.Add(RoadNetwork.GetTurnKey(fromEdge, toEdge));

                    continue;
                }

                var allowed = restriction.Turn == RestrictionTurn.UTurn
                    ? toEdges.Where(edge => edge == edgeReverse[fromEdge]).ToList()
                    : toEdges;

                if (allowed.Count == 0) continue;

                foreach (var other in outgoing[via].Where(edge => !allowed.Contains(edge)))
                    forbidden.Add(RoadNetwork.GetTurnKey(fromEdge, other));
            }

            applied++;
        }

        var result = forbidden.ToArray();
        Array.Sort(result);

        return (result, applied);
    }

    private static double GetDistanceMeters(int latitudeA, int longitudeA, int latitudeB, int longitudeB)
    {
        var lat1 = latitudeA / RoadNetwork.CoordinateScale * Math.PI / 180;
        var lat2 = latitudeB / RoadNetwork.CoordinateScale * Math.PI / 180;
        var deltaLat = lat2 - lat1;
        var deltaLon = (longitudeB - longitudeA) / RoadNetwork.CoordinateScale * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        return 2 * EarthRadiusMeters * Math.Asin(Math.Sqrt(a));
    }
}
