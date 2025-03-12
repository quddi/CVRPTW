namespace CVRPTW.Computing.Estimators.Time;

public interface ITimeEstimator
{
    void Estimate(CarPath path, Car car);
}