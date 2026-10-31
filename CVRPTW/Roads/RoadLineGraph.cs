namespace CVRPTW;

/// <summary>
/// Граф M1 зі специфікації: вершини — напрямлені ребра дорожньої мережі (номер вершини = номер ребра),
/// а перехід e → f існує, якщо ребро f виходить із кінця e і поворот e → f не заборонений.
/// Вага переходу дорівнює довжині ребра e, тому для вершини вага зберігається в <see cref="RoadNetwork.EdgeLength"/>.
/// </summary>
public class RoadLineGraph
{
    // Переходи вершини v: Targets[Offsets[v] .. Offsets[v + 1]).
    public required int[] Offsets { get; init; }

    public required int[] Targets { get; init; }

    public required RoadNetwork Network { get; init; }

    public int VertexCount => Network.EdgeCount;

    public int TransitionCount => Targets.Length;

    public static RoadLineGraph Build(RoadNetwork network)
    {
        var edgeCount = network.EdgeCount;

        // Ребра, що виходять з кожної вершини дорожньої мережі (CSR за початком ребра).
        var outStart = new int[network.NodeCount + 1];
        foreach (var node in network.EdgeFrom) outStart[node + 1]++;
        for (var i = 0; i < network.NodeCount; i++) outStart[i + 1] += outStart[i];

        var outEdges = new int[edgeCount];
        var fill = (int[])outStart.Clone();
        for (var edge = 0; edge < edgeCount; edge++) outEdges[fill[network.EdgeFrom[edge]]++] = edge;

        var offsets = new int[edgeCount + 1];
        var targets = new List<int>(edgeCount * 2);

        for (var edge = 0; edge < edgeCount; edge++)
        {
            var end = network.EdgeTo[edge];

            for (var i = outStart[end]; i < outStart[end + 1]; i++)
            {
                var next = outEdges[i];
                if (!network.IsTurnForbidden(edge, next)) targets.Add(next);
            }

            offsets[edge + 1] = targets.Count;
        }

        return new RoadLineGraph { Offsets = offsets, Targets = targets.ToArray(), Network = network };
    }
}
