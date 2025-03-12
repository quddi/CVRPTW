namespace CVRPTW;

public class Tariffs
{
    private Dictionary<int, Dictionary<int, Dictionary<int, int>>> _tariffs = new();

    public int MatricesCount => _tariffs.Count;
    
    public int GetTariff(int matrixIndex, int firstPointIndex, int secondPointIndex)
    {
        return _tariffs[matrixIndex][firstPointIndex][secondPointIndex];
    }

    public void AddTariff(int matrixIndex, int firstPointIndex, int secondPointIndex, int tariff)
    {
        if (!_tariffs.ContainsKey(matrixIndex))
            _tariffs[matrixIndex] = new();

        if (!_tariffs[matrixIndex].ContainsKey(firstPointIndex))
            _tariffs[matrixIndex][firstPointIndex] = new();

        _tariffs[matrixIndex][firstPointIndex][secondPointIndex] = tariff;
    }
}