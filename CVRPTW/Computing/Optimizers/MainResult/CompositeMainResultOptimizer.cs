namespace CVRPTW.Computing.Optimizers;

public class CompositeMainResultOptimizer(List<IOptimizer> optimizers, bool report) : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        foreach (var optimizer in optimizers)
        {
            switch (optimizer)
            {
                case CarResultOptimizer carResultOptimizer:
                    OptimizeCarResults(carResultOptimizer, mainResult);
                    break;
                case MainResultOptimizer mainResultOptimizer:
                    mainResultOptimizer.Optimize(mainResult);
                    break;
            }

            if (!report) continue;
            
            Console.WriteLine($"===================== {optimizer.GetType().Name} =====================\n");
            Console.WriteLine(mainResult);
        }
    }

    private void OptimizeCarResults(CarResultOptimizer carResultOptimizer, MainResult mainResult)
    {
        foreach (var (_, carResult) in mainResult.Results)
        {
            carResultOptimizer.Optimize(carResult);
        }
    }
}