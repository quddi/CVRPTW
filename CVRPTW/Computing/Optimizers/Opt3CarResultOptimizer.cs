using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt3CarResultOptimizer(PathEstimator pathEstimator) : CarResultOptimizer(pathEstimator)
{
    private readonly PathEstimator _pathEstimator = pathEstimator;
    
    public override void Optimize(CarResult carResult)
    {
        var bestPathArray = carResult.Path.Clone();
        var bestCost = carResult.PathCost;
        var improved = true;
        var newPath = new CarPath();

        while (improved)
        {
            improved = false;
            for (int i = 0; i < bestPathArray.Length - 2; i++)
            {
                for (int j = i + 1; j < bestPathArray.Length - 1; j++)
                {
                    for (int k = j + 1; k <= bestPathArray.Length; k++)
                    {
                        newPath.SetPath(Apply3OptSwap(bestPathArray, i, j, k));

                        var estimate = _pathEstimator.Estimate(newPath);
                        
                        if (!(estimate < bestCost)) continue;
                        
                        bestPathArray = newPath;
                        bestCost = estimate;
                        improved = true;
                    }
                }
            }
        }

        carResult.Path = bestPathArray;
        carResult.PathCost = bestCost;
    }

  #region Default
    private int[] Apply3OptSwap(int[] path, int i, int j, int k)
    {
        var segment1 = path.Take(i).ToArray();
        var segment2 = path.Skip(i).Take(j - i).ToArray();
        var segment3 = path.Skip(j).Take(k - j).ToArray();
        var segment4 = path.Skip(k).ToArray();

        int[][] newPaths =
        [
            segment1.Concat(segment2).Concat(segment3).Concat(segment4).ToArray(),
            segment1.Concat(segment2.Reverse()).Concat(segment3).Concat(segment4).ToArray(),
            segment1.Concat(segment2).Concat(segment3.Reverse()).Concat(segment4).ToArray(),
            segment1.Concat(segment2.Reverse()).Concat(segment3.Reverse()).Concat(segment4).ToArray(),
            segment1.Concat(segment3).Concat(segment2).Concat(segment4).ToArray(),
            segment1.Concat(segment3.Reverse()).Concat(segment2).Concat(segment4).ToArray(),
            segment1.Concat(segment3).Concat(segment2.Reverse()).Concat(segment4).ToArray(),
            segment1.Concat(segment3.Reverse()).Concat(segment2.Reverse()).Concat(segment4).ToArray()
        ];
        
        return newPaths.MinBy(_pathEstimator.Estimate)!;
    }
  #endregion
    
  /*#region Span
    private int[] Apply3OptSwap(int[] path, int i, int j, int k)
    {
        ReadOnlySpan<int> span = path;
        ReadOnlySpan<int> segment1 = span[..i];
        Span<int> segment2 = path.AsSpan(i, j - i);
        Span<int> segment3 = path.AsSpan(j, k - j);
        ReadOnlySpan<int> segment4 = span[k..];

        int[][] newPaths =
        [
            CombineSegments(segment1, segment2, segment3, segment4),
            CombineSegments(segment1, ReverseInPlace(segment2), segment3, segment4),
            CombineSegments(segment1, segment2, ReverseInPlace(segment3), segment4),
            CombineSegments(segment1, ReverseInPlace(segment2), ReverseInPlace(segment3), segment4),
            CombineSegments(segment1, segment3, segment2, segment4),
            CombineSegments(segment1, ReverseInPlace(segment3), segment2, segment4),
            CombineSegments(segment1, segment3, ReverseInPlace(segment2), segment4),
            CombineSegments(segment1, ReverseInPlace(segment3), ReverseInPlace(segment2), segment4)
        ];
        
        return newPaths.MinBy(_pathEstimator.Estimate)!;
    }

    private Span<int> ReverseInPlace(Span<int> span)
    {
        int left = 0;
        int right = span.Length - 1;
        while (left < right)
        {
            (span[left], span[right]) = (span[right], span[left]);
            left++;
            right--;
        }
        return span;
    }

    private int[] CombineSegments(ReadOnlySpan<int> seg1, Span<int> seg2, Span<int> seg3, ReadOnlySpan<int> seg4)
    {
        int[] result = new int[seg1.Length + seg2.Length + seg3.Length + seg4.Length];
        int offset = 0;

        seg1.CopyTo(result.AsSpan(offset));
        offset += seg1.Length;
        seg2.CopyTo(result.AsSpan(offset));
        offset += seg2.Length;
        seg3.CopyTo(result.AsSpan(offset));
        offset += seg3.Length;
        seg4.CopyTo(result.AsSpan(offset));

        return result;
    }
  #endregion*/
}
