namespace CVRPTW.Computing.Estimators.Time;

public interface ITimeEstimator
{
    double Estimate(CarPath path, Car car);
}