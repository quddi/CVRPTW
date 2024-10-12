namespace CVRPTW;

public struct PointVisitInfo(int? id)
{
    public int? Id { get; set; } = id;

    public bool IsAnyPoint => Id == null;
}