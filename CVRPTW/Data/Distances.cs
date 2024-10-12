namespace CVRPTW;

public class Distances
{
    private Dictionary<int, Dictionary<int, Dictionary<int, double>>> _distances;

    public int MatricesCount => _distances.Count;
    
    public double GetDistance(int matrixIndex, int firstPointId, int secondPointId)
    {
        return _distances[matrixIndex][firstPointId][secondPointId];
    }
}