using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public abstract class IteratedPathComputer(MainData mainData, PathEstimator pathEstimator) : PathComputer(mainData, pathEstimator)
{
    public override MainResult Compute()
    {
        var result = new MainResult();
        
        foreach (var car in _mainData!.Cars)
        {
            result.Results.Add(car, GetCarResult(car));
        }

        return result;
    }

    protected abstract CarResult GetCarResult(Car car);
}