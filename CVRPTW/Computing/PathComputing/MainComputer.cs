using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class MainComputer(MainData mainData)
{
    protected MainData _mainData = mainData;

    public abstract MainResult Compute();
}