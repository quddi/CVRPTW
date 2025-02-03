namespace CVRPTW.Computing.Optimizers;

public class SwapSegmentsPathOptimizerCommand : SymmetricPathOptimizerCommand
{
    protected override void SymmetricDo(CarPath path, int aStart, int bStart, int cStart)
    {
        path.SwapSegments(aStart + 1, bStart, bStart + 1, cStart);
    }
}