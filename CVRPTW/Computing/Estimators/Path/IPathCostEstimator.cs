namespace CVRPTW.Computing.Estimators;

public interface IPathCostEstimator
{
    double Estimate(CarPath path);
}