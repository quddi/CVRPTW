using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class PathComputer(MainData mainData, PathEstimator pathEstimator)
{
    protected MainData _mainData = mainData;
    protected PathEstimator _pathEstimator = pathEstimator;

    public abstract MainResult Compute();
}