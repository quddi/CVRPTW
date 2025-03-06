namespace CVRPTW.Computing.Estimators;

public abstract class MainResultEstimator(PathCostEstimator pathCostEstimator)
{
   public readonly PathCostEstimator PathCostEstimator = pathCostEstimator;

   public abstract double Estimate(MainResult mainResult);
}