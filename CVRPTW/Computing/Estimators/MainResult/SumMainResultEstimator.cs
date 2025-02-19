using CVRPTW.Computing.Optimizers;

namespace CVRPTW.Computing.Estimators;

public class SumMainResultEstimator(MainData mainData, PathEstimator pathEstimator) : MainResultEstimator(mainData, pathEstimator)
{
    public override double Estimate(MainResult mainResult)
    {
        foreach (var (_, carResult) in mainResult.Results)
        {
            carResult.ReEstimate(pathEstimator);
        }

        return mainResult.Results.Values.Sum(x => x.Estimation);
    }
}