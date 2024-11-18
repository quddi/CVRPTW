namespace CVRPTW.Computing;

public abstract class IteratedPathComputer : PathComputer
{
    protected override List<CarResult> Compute()
    {
        var list = new List<CarResult>();
        
        foreach (var car in _mainData!.Cars)
        {
            list.Add(GetCarResult(car));
        }

        return list;
    }

    protected abstract CarResult GetCarResult(Car car);
}