namespace CVRPTW.Computing.Optimizers;

public class SwapSegmentsPathOptimizerCommand : IPathOptimizerCommand
{
    public void Do(CarPath path, int aStart, int bStart, int cStart)
    {
        path.SwapSegments(aStart + 1, bStart, bStart+1, cStart);
    }

    public void Undo(CarPath path, int aStart, int bStart, int cStart)
    {
        var midStart = cStart + aStart - bStart;
        
        path.SwapSegments(aStart + 1, midStart, midStart + 1, cStart);
    }
}