namespace CVRPTW;

public class RoadDistanceMatrix
{
    // Значення null означає недосяжний перехід. Помилки отримання матриці сюди не потрапляють.
    public required int[] PointIds { get; init; }

    public required double?[,] Values { get; init; }

    public required RoadDistanceMetadata Metadata { get; init; }
}
