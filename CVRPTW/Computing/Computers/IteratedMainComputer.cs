using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class IteratedMainComputer(MainData mainData, MainResultEstimator mainResultEstimator) : MainComputer(mainData)
{
    public override MainResult Compute()
    {
        var result = new MainResult();
        
        foreach (var car in _mainData!.Cars)
        {
            result.Results.Add(car, GetCarResult(car));
        }

        result.ReEstimateCost(mainResultEstimator);
        
        return result;
    }

    protected abstract CarResult GetCarResult(Car car);
}