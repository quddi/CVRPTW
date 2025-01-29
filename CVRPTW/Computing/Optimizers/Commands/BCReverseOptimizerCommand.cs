namespace CVRPTW.Computing.Optimizers;

public class BCReverseOptimizerCommand : SymmetricOptimizerCommand
{
    protected override void SymmetricDo(CarPath path, int aStart, int bStart, int cStart)
    {
        path.Reverse(bStart + 1, cStart);
    }
}