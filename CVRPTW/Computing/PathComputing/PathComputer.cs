using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class PathComputer
{
    protected MainData? _mainData;
    protected PathEstimator? _pathEstimator;

    public virtual MainResult Compute(MainData mainData, PathEstimator pathEstimator)
    {
        _mainData = mainData;
        _pathEstimator = pathEstimator;

        var result = Compute();
        
        ClearState();

        return result;
    }

    protected abstract MainResult Compute();

    protected virtual void ClearState()
    {
        _mainData = null;
    }
}