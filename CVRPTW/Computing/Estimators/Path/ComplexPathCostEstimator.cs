using CVRPTW.Computing.Estimators.Time;

namespace CVRPTW.Computing.Estimators;

public class ComplexPathCostEstimator(MainData mainData, PathCostEstimator baseEstimator, 
    TimeEstimator timeEstimator) : PathCostEstimator(mainData)
{
    public override double Estimate(CarPath path)
    {
        return 0;
    }
}