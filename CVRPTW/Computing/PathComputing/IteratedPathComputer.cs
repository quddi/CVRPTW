using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class IteratedPathComputer(MainData mainData, MainResultEstimator mainResultEstimator) : PathComputer(mainData)
{
    public override MainResult Compute()
    {
        var result = new MainResult();
        
        foreach (var car in _mainData!.Cars)
        {
            result.Results.Add(car, GetCarResult(car));
        }

        result.ReEstimate(mainResultEstimator);
        
        return result;
    }

    protected abstract CarResult GetCarResult(Car car);
}