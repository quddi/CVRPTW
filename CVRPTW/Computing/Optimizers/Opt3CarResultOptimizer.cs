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
    
    private int[] Apply3OptSwap(int[] path, int i, int j, int k)
    {
        var segment1 = path.Take(i).ToArray();
        var segment2 = path.Skip(i).Take(j - i).ToArray();
        var segment3 = path.Skip(j).Take(k - j).ToArray();
        var segment4 = path.Skip(k).ToArray();

        int[][] newPaths = {
            segment1.Concat(segment2).Concat(segment3).Concat(segment4).ToArray(),
            segment1.Concat(segment2.Reverse()).Concat(segment3).Concat(segment4).ToArray(),
            segment1.Concat(segment2).Concat(segment3.Reverse()).Concat(segment4).ToArray(),
            segment1.Concat(segment2.Reverse()).Concat(segment3.Reverse()).Concat(segment4).ToArray(),
            segment1.Concat(segment3).Concat(segment2).Concat(segment4).ToArray(),
            segment1.Concat(segment3.Reverse()).Concat(segment2).Concat(segment4).ToArray(),
            segment1.Concat(segment3).Concat(segment2.Reverse()).Concat(segment4).ToArray(),
            segment1.Concat(segment3.Reverse()).Concat(segment2.Reverse()).Concat(segment4).ToArray(),
        };

        return newPaths.MinBy(_pathEstimator.Estimate)!;
    }
}