using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt3CarResultOptimizer(IMainResultEstimator mainResultEstimator) : CarResultOptimizer
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

    protected override void Optimize(MainResult mainResult, Car car)
    {
        var carResult = mainResult.Results[car];
        
        var pathLength = carResult.Path.Count;

        for (int aStart = 0; aStart < pathLength - 5; aStart++)
        {
            for (int bStart = aStart + 2; bStart < pathLength - 3; bStart++)
            {
                for (int cStart = bStart + 2; cStart < pathLength - 1; cStart++)
                {
                    CheckAllVariants(mainResult, carResult, aStart, bStart, cStart);
                }
            }
        }
    }

    // a, b, c - edges

    private void CheckAllVariants(MainResult mainResult, CarResult carResult, int aStart, int bStart, int cStart)
    {
        var bestCommand = -1;
        var path = carResult.Path;
        var minEstimation = mainResultEstimator.Estimate(mainResult);
        
        for (var i = 0; i < Commands.Count; i++)
        {
            var command = Commands[i];
            
            command.Do(path, aStart, bStart, cStart);

            var newEstimation = mainResultEstimator.Estimate(mainResult);

            if (newEstimation < minEstimation)
            {
                minEstimation = newEstimation;
                bestCommand = i;
            }
            
            command.Undo(path, aStart, bStart, cStart);
        }

        if (bestCommand != -1) Commands[bestCommand].Do(path, aStart, bStart, cStart);
    }
}
