namespace CVRPTW;

public class Distances
{
    public Dictionary<int, Dictionary<int, Dictionary<int, double>>> Matrix { get; private set; } = new();

    public int MatricesCount => Matrix.Count;
    
    public double GetDistance(int matrixIndex, int firstPointId, int secondPointId)
    {
        return Matrix[matrixIndex][firstPointId][secondPointId];
    }

    public void AddDistance(int matrixIndex, int firstPointId, int secondPointId, double distance)
    {
        if (!Matrix.ContainsKey(matrixIndex)) 
            Matrix[matrixIndex] = new();
        
        if (!Matrix[matrixIndex].ContainsKey(firstPointId)) 
            Matrix[matrixIndex][firstPointId] = new();
        
        Matrix[matrixIndex][firstPointId][secondPointId] = distance;
    }

    public int GetNearestPointId(int matrixIndex, int fromPointId)
    {
        var toPointsDistances = Matrix[matrixIndex][fromPointId];

        return toPointsDistances.MinBy(pair => pair.Value).Key;
    }
}