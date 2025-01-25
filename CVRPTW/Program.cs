using CVRPTW.Computing;
using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Optimizers;

namespace CVRPTW;

static class Program
{
    private static void Main()
    {
        var path = @"C:\Users\Admin\Desktop\Диплом\original.txt";
        MainData mainData;
        var parser = new MainParser();
        
        using (var streamReader = new StreamReader(path))
        {
            mainData = parser.Parse(streamReader);
        }

        var estimator = new DistancePathEstimator(mainData);
        var computer = new StartPathComputer();

        var results = computer.Compute(mainData, estimator).ToList();

        Console.WriteLine(results.ToString<CarResult>());

        Console.WriteLine("\n===========================================\n");
        
        var optimizer = new Opt2CarResultOptimizer(estimator);
        
        foreach (var result in results)
        {
            if (result.Path.Length <= 2) continue;
            
            optimizer.Optimize(result);
        }
        
        Console.WriteLine(results.ToString<CarResult>());
    }
}