namespace CVRPTW.Computing.Optimizers;

public class ABReverseOptimizerCommand : SymmetricOptimizerCommand
{
    protected override void SymmetricDo(CarPath path, int aStart, int bStart, int cStart)
    {
        path.Reverse(aStart + 1, bStart);
    }
}