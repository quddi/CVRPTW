namespace CVRPTW.Data;

public class AlternativePoints : IData
{
    private Dictionary<int, int> _alternativePoint;

    public int? GetAlternativePoint(int point)
    {
        return _alternativePoint.TryGetValue(point, out var value) ? value : null;
    }
}