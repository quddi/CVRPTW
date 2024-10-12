namespace CVRPTW;

public class Tariffs
{
    private Dictionary<int, Dictionary<int, Dictionary<int, double>>> _tariffs;

    public int MatricesCount => _tariffs.Count;
    
    public double GetTariff(int matrixIndex, int firstPointId, int secondPointId)
    {
        return _tariffs[matrixIndex][firstPointId][secondPointId];
    }
}