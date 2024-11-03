namespace CVRPTW;

public class CarPath
{
    public int StartPointId { get; set; }
    
    public int EndPointId { get; set; }
    
    public List<int> PathPointsIds { get; set; } = new();

    public override string ToString()
    {
        return $"[{StartPointId} [{string.Join(", ", PathPointsIds)}] {EndPointId}]";
    }
}