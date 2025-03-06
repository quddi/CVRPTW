using CVRPTW.Computing.Optimizers;

namespace CVRPTW.Computing.Estimators;

public class SumMainResultEstimator(PathCostEstimator pathCostEstimator) : MainResultEstimator(pathCostEstimator)
{
    public override double Estimate(MainResult mainResult)
    {
        foreach (var (_, carResult) in mainResult.Results)
        {
            carResult.ReEstimate(PathCostEstimator);
        }

        return mainResult.Results.Values.Sum(x => x.Estimation);
    }
}