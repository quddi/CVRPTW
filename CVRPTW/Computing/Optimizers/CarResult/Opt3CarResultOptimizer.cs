using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt3CarResultOptimizer(IPathCostEstimator pathCostEstimator) : CarResultOptimizer
{
    private static readonly List<IPathOptimizerCommand> Commands =
    [
        new ABReversePathOptimizerCommand(),
        new BCReversePathOptimizerCommand(),
        new ACReversePathOptimizerCommand(),
        new SwapSegmentsPathOptimizerCommand(),
        new SequentialPathOptimizerCommand
        (
            [
                new ABReversePathOptimizerCommand(),
                new SwapSegmentsPathOptimizerCommand()
            ]
        ),
        new SequentialPathOptimizerCommand
        (
            [
                new BCReversePathOptimizerCommand(),
                new SwapSegmentsPathOptimizerCommand()
            ]
        ),
        new SequentialPathOptimizerCommand
        (
            [
                new ABReversePathOptimizerCommand(),
                new BCReversePathOptimizerCommand(),
                new SwapSegmentsPathOptimizerCommand()
            ]
        )
    ];
    
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
        
        for (var i = 0; i < Commands.Count; i++)
        {
            var command = Commands[i];
            
            command.Do(path, aStart, bStart, cStart);
            
            var estimation = pathCostEstimator.Estimate(path);

            if (estimation < carResult.Estimation)
            {
                carResult.Estimation = estimation;
                bestCommand = i;
            }
            
            command.Undo(path, aStart, bStart, cStart);
        }

        if (bestCommand != -1) Commands[bestCommand].Do(path, aStart, bStart, cStart);
    }
}
