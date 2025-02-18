using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class CompositeMainResultOptimizer(List<IOptimizer> optimizers, MainResultEstimator mainResultEstimator, bool report) : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        foreach (var optimizer in optimizers)
        {
            mainResult.Optimize(optimizer, mainResultEstimator);

            if (!report) continue;
            
            Console.WriteLine($"===================== {optimizer.GetType().Name} =====================\n");
            Console.WriteLine(mainResult);
        }
    }
}