namespace CVRPTW;

public class Distances
{
    public Dictionary<int, Dictionary<int, Dictionary<int, double>>> Matrix { get; private set; } = new();

    public int MatricesCount => Matrix.Count;
    
    public double GetDistance(int matrixIndex, int firstPointIndex, int secondPointIndex)
    {
        return Matrix[matrixIndex][firstPointIndex][secondPointIndex];
    }

    public void AddDistance(int matrixIndex, int firstPointIndex, int secondPointIndex, double distance)
    {
        if (!Matrix.ContainsKey(matrixIndex)) 
            Matrix[matrixIndex] = new();
        
        if (!Matrix[matrixIndex].ContainsKey(firstPointIndex)) 
            Matrix[matrixIndex][firstPointIndex] = new();
        
        Matrix[matrixIndex][firstPointIndex][secondPointIndex] = distance;
    }

    public int GetNearestPointId(int matrixIndex, int fromPointIndex)
    {
        var toPointsDistances = Matrix[matrixIndex][fromPointIndex];

        return toPointsDistances.MinBy(pair => pair.Value).Key;
    }
}