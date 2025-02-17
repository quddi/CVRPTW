using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class PathComputer(MainData mainData)
{
    protected MainData _mainData = mainData;

    public abstract MainResult Compute();
}