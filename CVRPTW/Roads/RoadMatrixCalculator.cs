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

    private class SearchBuffers(int vertexCount)
    {
        public readonly double[] Distance = new double[vertexCount];
        public readonly int[] Seen = new int[vertexCount];
        public readonly int[] Settled = new int[vertexCount];
        public readonly PriorityQueue<int, double> Queue = new();
        public int Stamp;
    }
}
