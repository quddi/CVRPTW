using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class PathComputer
{
    protected MainData? _mainData;
    protected PathEstimator? _pathEstimator;

    public virtual List<CarResult> Compute(MainData mainData, PathEstimator pathEstimator)
    {
        _mainData = mainData;
        _pathEstimator = pathEstimator;

        var result = Compute();
        
        ClearState();

        return result;
    }

    protected abstract List<CarResult> Compute();

    protected virtual void ClearState()
    {
        _mainData = null;
    }
}