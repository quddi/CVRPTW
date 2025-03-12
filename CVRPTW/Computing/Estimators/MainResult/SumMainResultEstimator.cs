using CVRPTW.Computing.Optimizers;

namespace CVRPTW.Computing.Estimators;

public class SumMainResultEstimator(IPathCostEstimator pathCostEstimator) : MainResultEstimator(pathCostEstimator)
{
    public override double Estimate(MainResult mainResult)
    {
        foreach (var (_, carResult) in mainResult.Results)
        {
            carResult.ReEstimateCost(PathCostEstimator);
        }

        return mainResult.Results.Values.Sum(x => x.Estimation);
    }
}