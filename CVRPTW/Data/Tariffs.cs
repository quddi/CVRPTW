namespace CVRPTW;

public class Tariffs
{
    private Dictionary<int, Dictionary<int, Dictionary<int, int>>> _tariffs = new();

    public int MatricesCount => _tariffs.Count;
    
    public int GetTariff(int matrixIndex, int firstPointId, int secondPointId)
    {
        return _tariffs[matrixIndex][firstPointId][secondPointId];
    }

    public void AddTariff(int matrixIndex, int firstPointId, int secondPointId, int tariff)
    {
        if (!_tariffs.ContainsKey(matrixIndex))
            _tariffs[matrixIndex] = new();

        if (!_tariffs[matrixIndex].ContainsKey(firstPointId))
            _tariffs[matrixIndex][firstPointId] = new();

        _tariffs[matrixIndex][firstPointId][secondPointId] = tariff;
    }
}