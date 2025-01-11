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
    
    public void SetPath(params int[] totalPath)
    {
        if (totalPath == null || totalPath.Length < 2)
        {
            throw new ArgumentException("Path must contain at least a start and an end point.");
        }

        StartPointId = totalPath[0];
        EndPointId = totalPath[totalPath.Length - 1];
        PathPointsIds = totalPath.Skip(1).Take(totalPath.Length - 2).ToList();
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
    
    public static implicit operator int[](CarPath carPath)
    {
        var result = new int[carPath.Length];
        for (int i = 0; i < carPath.Length; i++)
        {
            result[i] = carPath[i];
        }
        return result;
    }
}