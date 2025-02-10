using System.Collections;

namespace CVRPTW;

public class AlternativePoints : IEnumerable<KeyValuePair<int, int>>
{
    public readonly Dictionary<int, int> AlternativePoint = new();
    public readonly Dictionary<int, int> InverseAlternativePoint = new();

    public int? GetAlternativePoint(int point)
    {
        if (AlternativePoint.TryGetValue(point, out var value1))
            return value1;

        if (InverseAlternativePoint.TryGetValue(point, out var value2))
            return value2;
        
        return null;
    }

    public void AddAlternativePoint(int firstPointId, int secondPointId)
    {
        AlternativePoint.Add(firstPointId, secondPointId);
        InverseAlternativePoint.Add(secondPointId, firstPointId);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<int, int>> GetEnumerator() => AlternativePoint.GetEnumerator();
}