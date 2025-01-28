using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt3NewCarResultOptimizer(PathEstimator pathEstimator) : CarResultOptimizer(pathEstimator)
{
    private readonly PathEstimator _pathEstimator = pathEstimator;

    public override void Optimize(CarResult carResult)
    {
        if (carResult.Path.Count < 6)
            throw new ArgumentException("Path too short for 3-opt optimization.");

        bool improved;
        int pathLength = carResult.Path.Count;

        do
        {
            improved = false;
            // Iterate through all possible triplets (i, j, k)
            for (int i = 0; i <= pathLength - 5; i++)
            {
                for (int j = i + 2; j <= pathLength - 3; j++)
                {
                    for (int k = j + 2; k <= pathLength - 1; k++)
                    {
                        if (TryOptimize3(carResult, i, j, k))
                        {
                            improved = true;
                            // Restart the search after any improvement to avoid stale indices
                            goto RestartSearch;
                        }
                    }
                }
            }
        RestartSearch:;
        } while (improved);
    }

    private bool TryOptimize3(CarResult result, int i, int j, int k)
    {
        // Original edges: A-B, C-D, E-F
        int A = result.Path[i];
        int B = result.Path[i + 1];
        int C = result.Path[j];
        int D = result.Path[j + 1];
        int E = result.Path[k];
        int F = result.Path[k + 1];

        double originalCost = _pathEstimator.Estimate(A, B) +
                              _pathEstimator.Estimate(C, D) +
                              _pathEstimator.Estimate(E, F);

        // Evaluate all 7 possible reconnection cases
        (double cost, Action<CarPath> action)[] cases = new (double, Action<CarPath>)[7];

        // Case 1: A-C-B-E-D-F (reverse B-C and D-E)
        cases[0] = (
            _pathEstimator.Estimate(A, C) + _pathEstimator.Estimate(B, E) + _pathEstimator.Estimate(D, F),
            path => { path.Invert(i + 1, j); path.Invert(j + 1, k); }
        );

        // Case 2: A-D-C-B-E-F (reverse C-D and B-C-D)
        cases[1] = (
            _pathEstimator.Estimate(A, D) + _pathEstimator.Estimate(C, B) + _pathEstimator.Estimate(E, F),
            path => { path.Invert(i + 1, j); path.Invert(i + 1, k); }
        );

        // Case 3: A-E-D-C-B-F (reverse B-C-D-E)
        cases[2] = (
            _pathEstimator.Estimate(A, E) + _pathEstimator.Estimate(D, C) + _pathEstimator.Estimate(B, F),
            path => path.Invert(i + 1, k)
        );

        // Case 4: A-C-D-E-B-F (reverse B-C and E-D)
        cases[3] = (
            _pathEstimator.Estimate(A, C) + _pathEstimator.Estimate(D, E) + _pathEstimator.Estimate(B, F),
            path => { path.Invert(i + 1, j); path.Invert(j + 1, k); path.Invert(i + 1, k); }
        );

        // Case 5: A-D-E-C-B-F (reverse C-D-E)
        cases[4] = (
            _pathEstimator.Estimate(A, D) + _pathEstimator.Estimate(E, C) + _pathEstimator.Estimate(B, F),
            path => { path.Invert(j + 1, k); path.Invert(i + 1, k); }
        );

        // Case 6: A-E-B-C-D-F (reverse B-E)
        cases[5] = (
            _pathEstimator.Estimate(A, E) + _pathEstimator.Estimate(B, C) + _pathEstimator.Estimate(D, F),
            path => { path.Invert(i + 1, k); path.Invert(i + 1, j); }
        );

        // Case 7: A-E-C-D-B-F (reverse B-C and E-D-B)
        cases[6] = (
            _pathEstimator.Estimate(A, E) + _pathEstimator.Estimate(C, D) + _pathEstimator.Estimate(B, F),
            path => { path.Invert(i + 1, j); path.Invert(j + 1, k); path.Invert(i + 1, j); }
        );

        // Find the best valid improvement
        int bestCase = -1;
        double minCost = originalCost;
        for (int idx = 0; idx < cases.Length; idx++)
        {
            if (cases[idx].cost < minCost)
            {
                minCost = cases[idx].cost;
                bestCase = idx;
            }
        }

        if (bestCase == -1) return false;

        // Apply the best inversion sequence
        cases[bestCase].action.Invoke(result.Path);
        result.PathCost -= (originalCost - minCost);
        return true;
    }
}