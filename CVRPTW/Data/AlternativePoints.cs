namespace CVRPTW;

public class AlternativePoints
{
    private readonly Dictionary<int, int> _alternativePoint = new();

    public int? GetAlternativePoint(int point)
    {
        return _alternativePoint.TryGetValue(point, out var value) ? value : null;
    }

    public void AddAlternativePoint(int firstPointId, int secondPointId)
    {
        _alternativePoint.Add(firstPointId, secondPointId);
        _alternativePoint.Add(secondPointId, firstPointId);
    }
}