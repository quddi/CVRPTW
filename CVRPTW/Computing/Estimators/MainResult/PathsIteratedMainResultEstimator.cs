namespace CVRPTW.Computing.Estimators;

public abstract class PathsIteratedMainResultEstimator : IMainResultEstimator
{
    public double Estimate(MainResult mainResult)
    {
        mainResult.Estimation = 0;
        
        foreach (var (car, carResult) in mainResult.Results)
        {
            mainResult.Estimation += Estimate(car, carResult);
        }
        
        return mainResult.Estimation;
    }

    protected abstract double Estimate(Car car, CarResult carResult);
}