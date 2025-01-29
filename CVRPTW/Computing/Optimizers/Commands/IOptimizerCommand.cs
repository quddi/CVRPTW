namespace CVRPTW.Computing.Optimizers;

public interface IOptimizerCommand
{
    void Do(CarPath path, int aStart, int bStart, int cStart);
    void Undo(CarPath path, int aStart, int bStart, int cStart);
}