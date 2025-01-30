using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt3CarResultOptimizer(PathEstimator pathEstimator) : CarResultOptimizer(pathEstimator)
{
    private readonly List<IOptimizerCommand> _commands =
    [
        new ABReverseOptimizerCommand(),
        new BCReverseOptimizerCommand(),
        new ACReverseOptimizerCommand(),
        new SwapSegmentsOptimizerCommand(),
        new SequentialOptimizerCommand
        (
            [
                new ABReverseOptimizerCommand(),
                new SwapSegmentsOptimizerCommand()
            ]
        ),
        new SequentialOptimizerCommand
        (
            [
                new BCReverseOptimizerCommand(),
                new SwapSegmentsOptimizerCommand()
            ]
        ),
        new SequentialOptimizerCommand
        (
            [
                new ABReverseOptimizerCommand(),
                new BCReverseOptimizerCommand(),
                new SwapSegmentsOptimizerCommand()
            ]
        )
    ];
    
    private readonly PathEstimator _pathEstimator = pathEstimator;
    
    public override void Optimize(CarResult carResult)
    {
        var pathLength = carResult.Path.Count;

        for (int aStart = 0; aStart < pathLength - 5; aStart++)
        {
            for (int bStart = aStart + 2; bStart < pathLength - 3; bStart++)
            {
                for (int cStart = bStart + 2; cStart < pathLength - 1; cStart++)
                {
                    CheckAllVariants(carResult, aStart, bStart, cStart);
                }
            }
        }
    }

    // a, b, c - edges
    private void CheckAllVariants(CarResult carResult, int aStart, int bStart, int cStart)
    {
        var bestCommand = -1;
        var path = carResult.Path;
        
        for (var i = 0; i < _commands.Count; i++)
        {
            var command = _commands[i];
            
            command.Do(path, aStart, bStart, cStart);
            
            var estimation = _pathEstimator.Estimate(path);

            if (estimation < carResult.PathCost)
            {
                carResult.PathCost = estimation;
                bestCommand = i;
            }
            
            command.Undo(path, aStart, bStart, cStart);
        }

        if (bestCommand != -1) _commands[bestCommand].Do(path, aStart, bStart, cStart);
    }
}
