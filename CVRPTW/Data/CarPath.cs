namespace CVRPTW;

public class CarPath
{
    public int StartPointId { get; set; }
    
    public int EndPointId { get; set; }
    
    public List<int> PathPointsIds { get; set; } = new();

    public int Length => PathPointsIds.Count + 2;

    public int this[int index]
    {
        get
        {
            if (index == 0) return StartPointId;
            
            if (index == Length - 1) return EndPointId;
            
            return PathPointsIds[index - 1];
        }
        set
        {
            if (index == 0) StartPointId = value; 
            else if (index == Length - 1) EndPointId = value;
            else PathPointsIds[index - 1] = value;
        }
    }

    public CarPath Clone()
    {
        return new CarPath
        {
            StartPointId = this.StartPointId,
            EndPointId = this.EndPointId,
            PathPointsIds = [..PathPointsIds]
        };
    }

    public override string ToString()
    {
        return $"[{StartPointId} [{string.Join(", ", PathPointsIds)}] {EndPointId}]";
    }
}