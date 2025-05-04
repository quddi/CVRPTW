using System.Collections;

namespace CVRPTW;

public class AlternativePoints : IEnumerable<KeyValuePair<int, HashSet<int>>>
{
    public readonly Dictionary<int, HashSet<int>> Value = new();
    public readonly List<HashSet<int>> AllCorteges = new();

    public HashSet<int>? GetAlternativePoints(int point)
    {
        return Value.GetValueOrDefault(point);
    }

    public void AddAlternativePoints(params int[] alternativePoints)
    {
        for (int i = 0; i < alternativePoints.Length; i++)
        {
            var pointId = alternativePoints[i];
            
            Value.Add(pointId, new());

            for (int j = 0; j < alternativePoints.Length; j++)
            {
                if (i == j) continue;

                Value[pointId].Add(alternativePoints[j]);
            }
        }   
        
        AllCorteges.Add(alternativePoints.ToHashSet());
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<int, HashSet<int>>> GetEnumerator() => Value.GetEnumerator();
}