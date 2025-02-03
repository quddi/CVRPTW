namespace CVRPTW.Computing.Optimizers;

public class ACReversePathOptimizerCommand : SymmetricPathOptimizerCommand
{
    protected override void SymmetricDo(CarPath path, int aStart, int bStart, int cStart)
    {
        path.Reverse(aStart + 1, cStart);
    }
}