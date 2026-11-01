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

    // Ребра, що виходять з вершини дорожньої мережі n: NodeOutEdges[NodeOutStart[n] .. NodeOutStart[n + 1]).
    public required int[] NodeOutStart { get; init; }

    public required int[] NodeOutEdges { get; init; }

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

        return new RoadLineGraph { Offsets = offsets, Targets = targets.ToArray(), Network = network, NodeOutStart = outStart, NodeOutEdges = outEdges };
    }

    /// <summary>
    /// Вершини найбільшої сильно зв'язної компоненти: з будь-якої можна доїхати до будь-якої з урахуванням
    /// односторонніх доріг і заборонених поворотів. Точки поза нею (острівці доріг, глухі односторонні ділянки)
    /// недосяжні з основної мережі, тому прив'язувати точки слід лише сюди.
    /// </summary>
    public bool[] GetLargestStronglyConnectedComponent()
    {
        // Ітеративний алгоритм Тар'яна: рекурсія на мільйонах вершин переповнила б стек.
        var count = VertexCount;
        var order = new int[count];
        var low = new int[count];
        var component = new int[count];
        var onStack = new bool[count];
        var nextTransition = new int[count];
        var tarjanStack = new Stack<int>();
        var callStack = new Stack<int>();
        var componentSizes = new List<int>();
        var counter = 0;

        Array.Fill(order, -1);
        Array.Fill(component, -1);

        for (var root = 0; root < count; root++)
        {
            if (order[root] != -1) continue;

            callStack.Push(root);
            order[root] = low[root] = counter++;
            nextTransition[root] = Offsets[root];
            tarjanStack.Push(root);
            onStack[root] = true;

            while (callStack.Count > 0)
            {
                var vertex = callStack.Peek();

                if (nextTransition[vertex] < Offsets[vertex + 1])
                {
                    var next = Targets[nextTransition[vertex]++];

                    if (order[next] == -1)
                    {
                        order[next] = low[next] = counter++;
                        nextTransition[next] = Offsets[next];
                        tarjanStack.Push(next);
                        onStack[next] = true;
                        callStack.Push(next);
                    }
                    else if (onStack[next])
                    {
                        low[vertex] = Math.Min(low[vertex], order[next]);
                    }

                    continue;
                }

                callStack.Pop();

                if (callStack.Count > 0)
                {
                    var parent = callStack.Peek();
                    low[parent] = Math.Min(low[parent], low[vertex]);
                }

                if (low[vertex] != order[vertex]) continue;

                var size = 0;
                int member;

                do
                {
                    member = tarjanStack.Pop();
                    onStack[member] = false;
                    component[member] = componentSizes.Count;
                    size++;
                } while (member != vertex);

                componentSizes.Add(size);
            }
        }

        var largest = componentSizes.IndexOf(componentSizes.Max());
        var mask = new bool[count];

        for (var vertex = 0; vertex < count; vertex++) mask[vertex] = component[vertex] == largest;

        return mask;
    }
}
