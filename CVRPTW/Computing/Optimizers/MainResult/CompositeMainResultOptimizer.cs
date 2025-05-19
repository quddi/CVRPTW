using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class CompositeMainResultOptimizer(List<MainResultOptimizer> optimizers, IMainResultEstimator mainResultEstimator, bool report) : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        foreach (var optimizer in optimizers)
        {
            optimizer.Optimize(mainResult);

            if (!report) continue;
            
            Console.WriteLine(mainResult);
        }
    }
}