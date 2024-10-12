namespace CVRPTW;

public class Distances
{
    private Dictionary<int, Dictionary<int, Dictionary<int, double>>> _distances = new();

    public int MatricesCount => _distances.Count;
    
    public double GetDistance(int matrixIndex, int firstPointId, int secondPointId)
    {
        return _distances[matrixIndex][firstPointId][secondPointId];
    }

    public void AddDistance(int matrixIndex, int firstPointId, int secondPointId, double distance)
    {
        if (!_distances.ContainsKey(matrixIndex)) 
            _distances[matrixIndex] = new();
        
        if (!_distances[matrixIndex].ContainsKey(firstPointId)) 
            _distances[matrixIndex][firstPointId] = new();
        
        _distances[matrixIndex][firstPointId][secondPointId] = distance;
    }
}