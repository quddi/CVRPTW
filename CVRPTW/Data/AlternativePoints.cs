namespace CVRPTW;

public class AlternativePoints
{
    private Dictionary<int, int> _alternativePoint;

    public int? GetAlternativePoint(int point)
    {
        return _alternativePoint.TryGetValue(point, out var value) ? value : null;
    }
}