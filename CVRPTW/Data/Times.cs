namespace CVRPTW;

public class Times
{
    private Dictionary<int, Dictionary<int, Dictionary<int, int>>> _times = new();

    public int MatricesCount => _times.Count;
    
    public int GetTime(int matrixIndex, int firstPointId, int secondPointId)
    {
        return _times[matrixIndex][firstPointId][secondPointId];
    }
    
    public void AddTime(int matrixIndex, int firstPointId, int secondPointId, int time)
    {
        if (!_times.ContainsKey(matrixIndex))
            _times[matrixIndex] = new();

        if (!_times[matrixIndex].ContainsKey(firstPointId))
            _times[matrixIndex][firstPointId] = new();

        _times[matrixIndex][firstPointId][secondPointId] = time;
    }
}