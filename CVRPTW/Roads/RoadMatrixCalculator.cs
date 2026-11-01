namespace CVRPTW;

/// <summary>Точка для матриці: ідентифікатор і координати, поставлені користувачем.</summary>
public readonly record struct RoadMatrixPoint(int Id, double Latitude, double Longitude);

/// <summary>
/// Метод M2: прив'язує точки до найближчих ребер і рахує матрицю відстаней у метрах найкоротшими шляхами
/// в графі M1. Точка всередині ребра не розрізає граф: початок шляху — залишок ребра, кінець — частка ребра
/// до точки, тому заборонені повороти на ребрах діють так само, як у M1. Відстань від поставленої точки
/// до дороги в матрицю не входить. Недосяжна пара — null.
/// </summary>
public class RoadMatrixCalculator(RoadLineGraph graph, RoadSpatialIndex index)
{
    // Точка ближче за цю відстань до кінця ребра вважається такою, що стоїть на перехресті.
    private const double NodeToleranceMeters = 0.001;

    private readonly RoadNetwork _network = graph.Network;

    // Позиція точки на напрямленому ребрі: частка довжини ребра від його початку.
    private readonly record struct Position(int Edge, double Fraction);

    public RoadDistanceMatrix Calculate(IReadOnlyList<RoadMatrixPoint> points, int maxDegreeOfParallelism = 0)
    {
        var count = points.Count;
        var positions = new Position[count][];
        var snapDistances = new double[count];

        for (var i = 0; i < count; i++)
        {
            if (!index.TryFindNearest(points[i].Latitude, points[i].Longitude, out var snap))
                throw new InvalidOperationException($"No road found for point {points[i].Id}.");

            positions[i] = GetPositions(snap);
            snapDistances[i] = snap.DistanceMeters;
        }

        // Вершини, у яких закінчується прибуття до цілей: вершина → (ціль, позиція).
        var arrivals = new Dictionary<int, List<(int Target, double Fraction)>>();

        for (var j = 0; j < count; j++)
        foreach (var position in positions[j])
        {
            if (!arrivals.TryGetValue(position.Edge, out var list)) arrivals[position.Edge] = list = new();

            list.Add((j, position.Fraction));
        }

        var values = new double?[count, count];
        var threads = maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism : Environment.ProcessorCount;
        using var buffers = new ThreadLocal<SearchBuffers>(() => new SearchBuffers(graph.VertexCount));

        Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = threads }, source =>
        {
            var row = FindDistances(source, positions, arrivals, buffers.Value!);

            for (var target = 0; target < count; target++) values[source, target] = row[target];
        });

        return new RoadDistanceMatrix
        {
            PointIds = points.Select(point => point.Id).ToArray(),
            Values = values,
            Metadata = new RoadDistanceMetadata
            {
                Source = "OSM",
                Criterion = "distance, meters",
                Profile = "car",
                RetrievedAt = DateTime.UtcNow
            }
        };
    }

    // Позиція на ребрі та, якщо дорога двостороння, на зворотному ребрі.
    private Position[] GetPositions(RoadSnap snap)
    {
        var reverse = _network.EdgeReverse[snap.Edge];

        return reverse < 0
            ? [new Position(snap.Edge, snap.Fraction)]
            : [new Position(snap.Edge, snap.Fraction), new Position(reverse, 1 - snap.Fraction)];
    }

    private double?[] FindDistances(int source, Position[][] positions, Dictionary<int, List<(int Target, double Fraction)>> arrivals, SearchBuffers buffers)
    {
        var count = positions.Length;
        var best = new double[count];
        Array.Fill(best, double.PositiveInfinity);

        var stamp = ++buffers.Stamp;
        var distance = buffers.Distance;
        var seen = buffers.Seen;
        var settled = buffers.Settled;
        var queue = buffers.Queue;
        queue.Clear();

        void Relax(int vertex, double value)
        {
            if (seen[vertex] == stamp && distance[vertex] <= value) return;

            seen[vertex] = stamp;
            distance[vertex] = value;
            queue.Enqueue(vertex, value);
        }

        // Старт: доїхати до кінця ребра, на якому стоїть точка, і перейти на дозволені наступні ребра.
        foreach (var start in positions[source])
        {
            var rest = (1 - start.Fraction) * _network.EdgeLength[start.Edge];

            if (rest < NodeToleranceMeters)
            {
                // Точка стоїть на перехресті: напрямку приїзду немає, тож виїзд на будь-яке ребро дозволений.
                var node = _network.EdgeTo[start.Edge];

                for (var i = graph.NodeOutStart[node]; i < graph.NodeOutStart[node + 1]; i++) Relax(graph.NodeOutEdges[i], 0);
            }
            else
            {
                // Точка на початку ребра (однобічна дорога): ребро можна починати одразу.
                if (start.Fraction * _network.EdgeLength[start.Edge] < NodeToleranceMeters) Relax(start.Edge, 0);

                for (var i = graph.Offsets[start.Edge]; i < graph.Offsets[start.Edge + 1]; i++)
                    Relax(graph.Targets[i], rest);
            }

            // Ціль попереду на тому самому ребрі: їдемо прямо без виїзду на перехрестя.
            foreach (var target in Enumerable.Range(0, count))
            foreach (var end in positions[target])
                if (end.Edge == start.Edge && end.Fraction >= start.Fraction)
                    best[target] = Math.Min(best[target], (end.Fraction - start.Fraction) * _network.EdgeLength[start.Edge]);
        }

        var unsettledVertices = arrivals.Count;

        while (unsettledVertices > 0 && queue.TryDequeue(out var vertex, out var value))
        {
            if (settled[vertex] == stamp || value > distance[vertex]) continue;

            settled[vertex] = stamp;

            if (arrivals.TryGetValue(vertex, out var list))
            {
                foreach (var (target, fraction) in list)
                    best[target] = Math.Min(best[target], value + fraction * _network.EdgeLength[vertex]);

                unsettledVertices--;
            }

            for (var i = graph.Offsets[vertex]; i < graph.Offsets[vertex + 1]; i++)
            {
                var next = graph.Targets[i];
                if (settled[next] != stamp) Relax(next, value + _network.EdgeLength[vertex]);
            }
        }

        var row = new double?[count];
        for (var target = 0; target < count; target++) row[target] = double.IsPositiveInfinity(best[target]) ? null : best[target];

        return row;
    }

    private readonly object _pathLock = new();
    private PathBuffers? _pathBuffers;

    /// <summary>
    /// Метод M3: найкоротший шлях між двома точками з геометрією доріг. Довжина збігається зі значенням матриці.
    /// Повертає null, якщо шляху немає.
    /// </summary>
    public RoadPath? FindPath(RoadMatrixPoint from, RoadMatrixPoint to)
    {
        if (!index.TryFindNearest(from.Latitude, from.Longitude, out var fromSnap) ||
            !index.TryFindNearest(to.Latitude, to.Longitude, out var toSnap)) return null;

        var sources = GetPositions(fromSnap);
        var targets = GetPositions(toSnap);

        lock (_pathLock)
        {
            var buffers = _pathBuffers ??= new PathBuffers(graph.VertexCount);
            var stamp = ++buffers.Stamp;
            var queue = buffers.Queue;
            queue.Clear();
            buffers.Seeds.Clear();

            void Relax(int vertex, double value, int parent, int pieceEdge = -1, double pieceFrom = 0)
            {
                if (buffers.Seen[vertex] == stamp && buffers.Distance[vertex] <= value) return;

                buffers.Seen[vertex] = stamp;
                buffers.Distance[vertex] = value;
                buffers.Parent[vertex] = parent;
                if (parent < 0) buffers.Seeds[vertex] = (pieceEdge, pieceFrom);
                queue.Enqueue(vertex, value);
            }

            var best = double.PositiveInfinity;
            (int Edge, double From, double To)? direct = null;
            var arrivalVertex = -1;
            var arrivalFraction = 0.0;

            foreach (var start in sources)
            {
                var length = _network.EdgeLength[start.Edge];
                var rest = (1 - start.Fraction) * length;

                if (rest < NodeToleranceMeters)
                {
                    var node = _network.EdgeTo[start.Edge];

                    for (var i = graph.NodeOutStart[node]; i < graph.NodeOutStart[node + 1]; i++)
                        Relax(graph.NodeOutEdges[i], 0, -1);
                }
                else
                {
                    if (start.Fraction * length < NodeToleranceMeters) Relax(start.Edge, 0, -1);

                    for (var i = graph.Offsets[start.Edge]; i < graph.Offsets[start.Edge + 1]; i++)
                        Relax(graph.Targets[i], rest, -1, start.Edge, start.Fraction);
                }

                foreach (var end in targets)
                {
                    if (end.Edge != start.Edge || end.Fraction < start.Fraction) continue;

                    var candidate = (end.Fraction - start.Fraction) * length;
                    if (candidate >= best) continue;

                    best = candidate;
                    direct = (start.Edge, start.Fraction, end.Fraction);
                }
            }

            while (queue.TryDequeue(out var vertex, out var value) && value < best)
            {
                if (buffers.Settled[vertex] == stamp || value > buffers.Distance[vertex]) continue;

                buffers.Settled[vertex] = stamp;

                foreach (var end in targets)
                {
                    if (end.Edge != vertex) continue;

                    var candidate = value + end.Fraction * _network.EdgeLength[vertex];
                    if (candidate >= best) continue;

                    best = candidate;
                    direct = null;
                    arrivalVertex = vertex;
                    arrivalFraction = end.Fraction;
                }

                for (var i = graph.Offsets[vertex]; i < graph.Offsets[vertex + 1]; i++)
                {
                    var next = graph.Targets[i];
                    if (buffers.Settled[next] != stamp) Relax(next, value + _network.EdgeLength[vertex], vertex);
                }
            }

            if (double.IsPositiveInfinity(best)) return null;

            var latitudes = new List<double> { fromSnap.Latitude };
            var longitudes = new List<double> { fromSnap.Longitude };

            if (direct is { } straight)
            {
                AppendEdge(straight.Edge, straight.From, straight.To, latitudes, longitudes);
            }
            else
            {
                var chain = new List<int>();
                for (var vertex = arrivalVertex; vertex >= 0; vertex = buffers.Parent[vertex]) chain.Add(vertex);
                chain.Reverse();

                var (pieceEdge, pieceFrom) = buffers.Seeds[chain[0]];
                if (pieceEdge >= 0) AppendEdge(pieceEdge, pieceFrom, 1, latitudes, longitudes);

                for (var i = 0; i < chain.Count; i++)
                    AppendEdge(chain[i], 0, i == chain.Count - 1 ? arrivalFraction : 1, latitudes, longitudes);
            }

            latitudes.Add(toSnap.Latitude);
            longitudes.Add(toSnap.Longitude);

            return new RoadPath(best, latitudes.ToArray(), longitudes.ToArray());
        }
    }

    // Додає частину геометрії ребра між частками довжини from і to (від початку ребра).
    private void AppendEdge(int edge, double from, double to, List<double> latitudes, List<double> longitudes)
    {
        var first = _network.EdgeGeometryStart[edge];
        var last = _network.EdgeGeometryStart[edge + 1] - 1;
        var total = 0.0;

        for (var i = first; i < last; i++) total += _network.GetSegmentLength(i);

        if (total == 0 || to <= from) return;

        var passed = 0.0;

        for (var i = first; i < last; i++)
        {
            var length = _network.GetSegmentLength(i);
            var segmentStart = passed / total;
            var segmentEnd = (passed + length) / total;
            passed += length;

            if (segmentEnd <= from || segmentStart >= to) continue;

            // Кінці відрізка, обрізані до діапазону [from, to].
            var a = Math.Max(from, segmentStart);
            var b = Math.Min(to, segmentEnd);

            AddInterpolated(i, segmentEnd == segmentStart ? 0 : (a - segmentStart) / (segmentEnd - segmentStart), latitudes, longitudes);
            AddInterpolated(i, segmentEnd == segmentStart ? 1 : (b - segmentStart) / (segmentEnd - segmentStart), latitudes, longitudes);
        }
    }

    private void AddInterpolated(int segment, double t, List<double> latitudes, List<double> longitudes)
    {
        var scale = RoadNetwork.CoordinateScale;

        latitudes.Add((_network.GeometryLatitudes[segment] + t * (_network.GeometryLatitudes[segment + 1] - _network.GeometryLatitudes[segment])) / scale);
        longitudes.Add((_network.GeometryLongitudes[segment] + t * (_network.GeometryLongitudes[segment + 1] - _network.GeometryLongitudes[segment])) / scale);
    }

    private class PathBuffers(int vertexCount)
    {
        public readonly double[] Distance = new double[vertexCount];
        public readonly int[] Seen = new int[vertexCount];
        public readonly int[] Settled = new int[vertexCount];
        public readonly int[] Parent = new int[vertexCount];
        public readonly PriorityQueue<int, double> Queue = new();
        public readonly Dictionary<int, (int Edge, double From)> Seeds = new();
        public int Stamp;
    }

    private class SearchBuffers(int vertexCount)
    {
        public readonly double[] Distance = new double[vertexCount];
        public readonly int[] Seen = new int[vertexCount];
        public readonly int[] Settled = new int[vertexCount];
        public readonly PriorityQueue<int, double> Queue = new();
        public int Stamp;
    }
}
