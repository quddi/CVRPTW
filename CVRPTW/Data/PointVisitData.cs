namespace CVRPTW;

public struct PointVisitData(int? id)
{
    public int? Id { get; set; } = id;

    public bool IsAnyPoint => Id == null;
}