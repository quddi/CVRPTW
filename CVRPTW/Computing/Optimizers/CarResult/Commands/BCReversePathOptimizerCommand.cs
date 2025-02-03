namespace CVRPTW.Computing.Optimizers;

public class BCReversePathOptimizerCommand : SymmetricPathOptimizerCommand
{
    protected override void SymmetricDo(CarPath path, int aStart, int bStart, int cStart)
    {
        path.Reverse(bStart + 1, cStart);
    }
}