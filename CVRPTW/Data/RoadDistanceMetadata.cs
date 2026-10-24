namespace CVRPTW;

public class RoadDistanceMetadata
{
    public required string Source { get; init; }

    public required string Criterion { get; init; }

    public required string Profile { get; init; }

    public required DateTime RetrievedAt { get; init; }

    public Dictionary<string, string> Parameters { get; init; } = new();

    public string? DataVersion { get; init; }
}
