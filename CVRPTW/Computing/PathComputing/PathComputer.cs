namespace CVRPTW.Computing;

public abstract class PathComputer
{
    protected MainData? _mainData;

    public virtual List<CarResult> Compute(MainData mainData)
    {
        _mainData = mainData;

        var result = Compute();
        
        ClearState();

        return result;
    }

    protected abstract List<CarResult> Compute();

    protected virtual void ClearState()
    {
        _mainData = null;
    }
}