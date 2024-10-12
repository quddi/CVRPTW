namespace CVRPTW;

public class Times
{
    private Dictionary<int, Dictionary<int, Dictionary<int, double>>> _times;

    public int MatricesCount => _times.Count;
    
    public double GetTime(int matrixIndex, int firstPointId, int secondPointId)
    {
        return _times[matrixIndex][firstPointId][secondPointId];
    }
}