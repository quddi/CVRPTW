namespace CVRPTW.Computing.Optimizers;

public abstract class CarResultOptimizer : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        foreach (var car in mainResult.Results.Keys)
        {
            Optimize(mainResult, car);
        }
    }

    protected abstract void Optimize(MainResult mainResult, Car car);
}