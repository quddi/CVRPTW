namespace CVRPTW.Computing.Optimizers;

public abstract class SymmetricOptimizerCommand : IOptimizerCommand
{
    public void Do(CarPath path, int aStart, int bStart, int cStart) => SymmetricDo(path, aStart, bStart, cStart);
    public void Undo(CarPath path, int aStart, int bStart, int cStart) => SymmetricDo(path, cStart, bStart, aStart);
    
    protected abstract void SymmetricDo(CarPath path, int aStart, int bStart, int cStart);
}