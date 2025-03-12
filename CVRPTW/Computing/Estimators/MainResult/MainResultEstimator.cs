namespace CVRPTW.Computing.Estimators;

public abstract class MainResultEstimator(IPathCostEstimator pathCostEstimator)
{
   public readonly IPathCostEstimator PathCostEstimator = pathCostEstimator;

   public abstract double Estimate(MainResult mainResult);
}