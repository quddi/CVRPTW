namespace CVRPTW;

public struct PointInfo(int? id)
{
    public int? Id { get; set; } = id;

    public bool IsAnyPoint => Id == null;
}