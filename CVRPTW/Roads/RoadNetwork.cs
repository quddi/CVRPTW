namespace CVRPTW;

/// <summary>
/// Дорожня мережа для автомобіля: вершини (перехрестя й кінці доріг) і напрямлені ребра з геометрією.
/// Координати зберігаються в градусах, помножених на 1e7. Заборонений поворот — пара ребер (звідки, куди).
/// </summary>
public class RoadNetwork
{
    public const double CoordinateScale = 1e7;

    public required int[] NodeLatitudes { get; init; }

    public required int[] NodeLongitudes { get; init; }

    public required int[] EdgeFrom { get; init; }

    public required int[] EdgeTo { get; init; }

    // Довжина ребра в метрах.
    public required float[] EdgeLength { get; init; }

    // Ребро в протилежному напрямку тієї самої ділянки або -1 для односторонніх.
    public required int[] EdgeReverse { get; init; }

    // Ребро i має геометрію GeometryLatitudes/GeometryLongitudes[EdgeGeometryStart[i] .. EdgeGeometryStart[i + 1]).
    public required int[] EdgeGeometryStart { get; init; }

    public required int[] GeometryLatitudes { get; init; }

    public required int[] GeometryLongitudes { get; init; }

    // Відсортований масив ключів (fromEdge << 32 | toEdge).
    public required long[] ForbiddenTurns { get; init; }

    public int RestrictionsApplied { get; init; }

    // Обмеження, яких не підтримуємо: через дорогу, no_entry, no_exit.
    public int RestrictionsUnsupported { get; init; }

    public int NodeCount => NodeLatitudes.Length;

    public int EdgeCount => EdgeFrom.Length;

    // Межі вершин у градусах: (мінімальна широта, мінімальна довгота, максимальна широта, максимальна довгота).
    public (double MinLatitude, double MinLongitude, double MaxLatitude, double MaxLongitude) GetBounds() =>
        (NodeLatitudes.Min() / CoordinateScale, NodeLongitudes.Min() / CoordinateScale,
            NodeLatitudes.Max() / CoordinateScale, NodeLongitudes.Max() / CoordinateScale);

    public static long GetTurnKey(int fromEdge, int toEdge) => ((long)fromEdge << 32) | (uint)toEdge;

    public bool IsTurnForbidden(int fromEdge, int toEdge) =>
        Array.BinarySearch(ForbiddenTurns, GetTurnKey(fromEdge, toEdge)) >= 0;
}
