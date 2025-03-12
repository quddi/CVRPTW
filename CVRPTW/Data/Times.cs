namespace CVRPTW;

public class Times
{
    private Dictionary<int, Dictionary<int, Dictionary<int, int>>> _times = new();

    public int MatricesCount => _times.Count;
    
    public int GetTime(int matrixIndex, int firstPointIndex, int secondPointIndex)
    {
        return _times[matrixIndex][firstPointIndex][secondPointIndex];
    }
    
    public void AddTime(int matrixIndex, int firstPointIndex, int secondPointIndex, int time)
    {
        if (!_times.ContainsKey(matrixIndex))
            _times[matrixIndex] = new();

        if (!_times[matrixIndex].ContainsKey(firstPointIndex))
            _times[matrixIndex][firstPointIndex] = new();

        _times[matrixIndex][firstPointIndex][secondPointIndex] = time;
    }
}