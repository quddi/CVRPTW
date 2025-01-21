namespace CVRPTW.Computing.Estimators;

public abstract class PathEstimator(MainData mainData)
{
    protected MainData _mainData = mainData;

    public abstract double Estimate(CarPath path);

    public abstract double Estimate(int[] path);

    public abstract double Estimate(int firstPointId, int secondPointId);
}