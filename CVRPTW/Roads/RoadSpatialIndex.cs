namespace CVRPTW;

/// <summary>
/// Рівномірна сітка ребер для вибору доріг у межах видимої області та пошуку найближчої дороги до точки.
/// Безпечна для виклику з різних потоків.
/// </summary>
public class RoadSpatialIndex
{
    // 0,01° ≈ 1,1 км по широті.
    private const int CellSize = 100_000;
    private const double MetersPerDegree = 111_320;

    private readonly RoadNetwork _network;
    private readonly Dictionary<long, List<int>> _cells = new();
    private readonly int[] _visited;
    private readonly int _minCellX, _maxCellX, _minCellY, _maxCellY;
    private int _stamp;

    public RoadSpatialIndex(RoadNetwork network)
    {
        _network = network;
        _visited = new int[network.EdgeCount];
        _minCellX = _minCellY = int.MaxValue;
        _maxCellX = _maxCellY = int.MinValue;

        for (var edge = 0; edge < network.EdgeCount; edge++)
        {
            int minLat = int.MaxValue, maxLat = int.MinValue, minLon = int.MaxValue, maxLon = int.MinValue;

            for (var i = network.EdgeGeometryStart[edge]; i < network.EdgeGeometryStart[edge + 1]; i++)
            {
                minLat = Math.Min(minLat, network.GeometryLatitudes[i]);
                maxLat = Math.Max(maxLat, network.GeometryLatitudes[i]);
                minLon = Math.Min(minLon, network.GeometryLongitudes[i]);
                maxLon = Math.Max(maxLon, network.GeometryLongitudes[i]);
            }

            for (var x = GetCell(minLon); x <= GetCell(maxLon); x++)
            for (var y = GetCell(minLat); y <= GetCell(maxLat); y++)
            {
                var key = GetKey(x, y);
                if (!_cells.TryGetValue(key, out var list)) _cells[key] = list = new List<int>();
                list.Add(edge);
            }

            _minCellX = Math.Min(_minCellX, GetCell(minLon));
            _maxCellX = Math.Max(_maxCellX, GetCell(maxLon));
            _minCellY = Math.Min(_minCellY, GetCell(minLat));
            _maxCellY = Math.Max(_maxCellY, GetCell(maxLat));
        }
    }

    private static int GetCell(int coordinate) => (int)Math.Floor(coordinate / (double)CellSize);

    private static long GetKey(int cellX, int cellY) => ((long)cellX << 32) | (uint)cellY;

    // Скільки ребер (з повторами) лежить у комірках області; дешевша оцінка перед QueryEdges.
    public int EstimateEdgeCount(int minLat, int minLon, int maxLat, int maxLon)
    {
        var count = 0;

        lock (_cells)
        {
            foreach (var list in GetCellLists(minLat, minLon, maxLat, maxLon))
                count += list.Count;
        }

        return count;
    }

    public List<int> QueryEdges(int minLat, int minLon, int maxLat, int maxLon)
    {
        var result = new List<int>();

        lock (_cells)
        {
            var stamp = ++_stamp;

            foreach (var list in GetCellLists(minLat, minLon, maxLat, maxLon))
            foreach (var edge in list)
            {
                if (_visited[edge] == stamp) continue;

                _visited[edge] = stamp;
                result.Add(edge);
            }
        }

        return result;
    }

    private IEnumerable<List<int>> GetCellLists(int minLat, int minLon, int maxLat, int maxLon)
    {
        var fromX = Math.Max(GetCell(minLon), _minCellX);
        var toX = Math.Min(GetCell(maxLon), _maxCellX);
        var fromY = Math.Max(GetCell(minLat), _minCellY);
        var toY = Math.Min(GetCell(maxLat), _maxCellY);

        for (var x = fromX; x <= toX; x++)
        for (var y = fromY; y <= toY; y++)
        {
            if (_cells.TryGetValue(GetKey(x, y), out var list)) yield return list;
        }
    }

    public bool TryFindNearest(double latitude, double longitude, out RoadSnap snap)
    {
        snap = default;

        if (_network.EdgeCount == 0) return false;

        var latE7 = latitude * RoadNetwork.CoordinateScale;
        var lonE7 = longitude * RoadNetwork.CoordinateScale;
        var centerX = GetCell((int)lonE7);
        var centerY = GetCell((int)latE7);

        var metersPerLon = MetersPerDegree * Math.Cos(latitude * Math.PI / 180);
        var minCellMeters = CellSize / RoadNetwork.CoordinateScale * Math.Min(MetersPerDegree, metersPerLon);
        var maxRing = Math.Max(Math.Max(Math.Abs(centerX - _minCellX), Math.Abs(centerX - _maxCellX)),
            Math.Max(Math.Abs(centerY - _minCellY), Math.Abs(centerY - _maxCellY)));

        var best = double.MaxValue;

        lock (_cells)
        {
            var stamp = ++_stamp;

            for (var ring = 0; ring <= maxRing; ring++)
            {
                foreach (var list in GetRingLists(centerX, centerY, ring))
                foreach (var edge in list)
                {
                    if (_visited[edge] == stamp) continue;

                    _visited[edge] = stamp;

                    if (TryGetNearestOnEdge(edge, latitude, longitude, metersPerLon, out var candidate) && candidate.DistanceMeters < best)
                    {
                        best = candidate.DistanceMeters;
                        snap = candidate;
                    }
                }

                // Комірки наступного кільця лежать щонайменше на ring комірок від точки.
                if (best <= ring * minCellMeters) break;
            }
        }

        return best < double.MaxValue;
    }

    private IEnumerable<List<int>> GetRingLists(int centerX, int centerY, int ring)
    {
        for (var x = centerX - ring; x <= centerX + ring; x++)
        for (var y = centerY - ring; y <= centerY + ring; y++)
        {
            if (Math.Max(Math.Abs(x - centerX), Math.Abs(y - centerY)) != ring) continue;

            if (_cells.TryGetValue(GetKey(x, y), out var list)) yield return list;
        }
    }

    private bool TryGetNearestOnEdge(int edge, double latitude, double longitude, double metersPerLon, out RoadSnap snap)
    {
        snap = default;
        var best = double.MaxValue;
        var scale = RoadNetwork.CoordinateScale;

        for (var i = _network.EdgeGeometryStart[edge]; i + 1 < _network.EdgeGeometryStart[edge + 1]; i++)
        {
            // Координати відрізка в метрах відносно точки (рівнопроміжна проєкція достатня для малих відстаней).
            var ax = (_network.GeometryLongitudes[i] / scale - longitude) * metersPerLon;
            var ay = (_network.GeometryLatitudes[i] / scale - latitude) * MetersPerDegree;
            var bx = (_network.GeometryLongitudes[i + 1] / scale - longitude) * metersPerLon;
            var by = (_network.GeometryLatitudes[i + 1] / scale - latitude) * MetersPerDegree;

            var dx = bx - ax;
            var dy = by - ay;
            var lengthSquared = dx * dx + dy * dy;
            var t = lengthSquared == 0 ? 0 : Math.Clamp(-(ax * dx + ay * dy) / lengthSquared, 0, 1);

            var px = ax + t * dx;
            var py = ay + t * dy;
            var distance = Math.Sqrt(px * px + py * py);

            if (distance >= best) continue;

            best = distance;
            snap = new RoadSnap(edge, latitude + py / MetersPerDegree, longitude + px / metersPerLon, distance);
        }

        return best < double.MaxValue;
    }
}
