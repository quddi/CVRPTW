namespace CVRPTW.Computing.Optimizers;

public interface IPathOptimizerCommand
{
    void Do(CarPath path, int aStart, int bStart, int cStart);
    void Undo(CarPath path, int aStart, int bStart, int cStart);
}