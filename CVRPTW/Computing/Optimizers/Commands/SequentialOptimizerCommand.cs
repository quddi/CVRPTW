namespace CVRPTW.Computing.Optimizers;

public class SequentialOptimizerCommand(List<IOptimizerCommand> commands) : IOptimizerCommand
{
    public void Do(CarPath path, int aStart, int bStart, int cStart)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < commands.Count; i++)
            commands[i].Do(path, aStart, bStart, cStart);
    }

    public void Undo(CarPath path, int aStart, int bStart, int cStart)
    {
        for (var i = commands.Count - 1; i >= 0; i--)
            commands[i].Undo(path, aStart, bStart, cStart);
    }
}