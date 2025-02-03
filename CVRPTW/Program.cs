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

        var result = computer.Compute(mainData, estimator);
        var resultClone = result.Clone();

        Console.WriteLine(result);

        Console.WriteLine("===========================================\n");

        CarResultOptimizer optimizer = new Opt2CarResultOptimizer(estimator);

        foreach (var carResult in result.Results.Values)
        {
            if (carResult.Path.Count <= 2) continue;

            optimizer.Optimize(carResult);
        }

        Console.WriteLine(result);
        
        Console.WriteLine("===========================================\n");

        var firstPath = result.Results.First().Value.Path;
        var firstPathClone = resultClone.Results.First().Value.Path;
        
        
        /*optimizer = new Opt3CarResultOptimizer(estimator);

        foreach (var carResult in result.Results.Values)
        {
            if (carResult.Path.Count <= 2) continue;

            optimizer.Optimize(carResult);
        }

        Console.WriteLine(result);
        
        Console.WriteLine("===========================================\n");*/

        /*var mainOptimizer = new PointTransposeMainResultOptimizer(estimator);
        
        mainOptimizer.Optimize(result, mainData);*/
        
        foreach (var (car, carResult) in result.Results)
        {
            if (carResult.Path.Count <= 2) continue;
            
            carResult.PathCost = estimator.Estimate(carResult.Path);
        }

        Console.WriteLine(result);
    }
}