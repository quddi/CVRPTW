namespace CVRPTW.Computing;

public abstract class IteratedPathComputer : PathComputer
{
    protected override MainResult Compute()
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