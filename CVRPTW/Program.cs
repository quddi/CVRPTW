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

        var optimizedResult = computer.Compute(mainData, estimator);
        var startResult = optimizedResult.Clone();

        Console.WriteLine(optimizedResult);

        Console.WriteLine("====================== Opt2 ======================\n");

        CarResultOptimizer optimizer = new Opt2CarResultOptimizer(estimator);

        foreach (var carResult in optimizedResult.Results.Values)
        {
            if (carResult.Path.Count <= 2) continue;

            optimizer.Optimize(carResult);
        }

        Console.WriteLine(optimizedResult);
        
        Console.WriteLine("====================== Opt 3 ======================\n");
        
        optimizer = new Opt3CarResultOptimizer(estimator);

        foreach (var carResult in optimizedResult.Results.Values)
        {
            if (carResult.Path.Count <= 2) continue;

            optimizer.Optimize(carResult);
        }

        Console.WriteLine(optimizedResult);
        
        Console.WriteLine("====================== ReEstimation ======================\n");
        
        foreach (var (car, carResult) in optimizedResult.Results)
        {
            if (carResult.Path.Count <= 2) continue;
            
            carResult.PathCost = estimator.Estimate(carResult.Path);
        }

        Console.WriteLine(optimizedResult);

        if (AreResultsPathsPermutations(optimizedResult, startResult))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Paths are permutations!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Paths are not permutations!");
        }
    }

    private static bool AreResultsPathsPermutations(MainResult optimizedResult, MainResult startResult)
    {
        return optimizedResult.Results.Select(pair => pair.Value.Path)
            .Zip(startResult.Results.Select(pair => pair.Value.Path))
            .All(pair => ExtensionsMethods.ArePermutations(pair.First, pair.Second));
    }

    private static void Write<T>(T[] arr)
    {
        foreach (var x in arr)
        {
            Console.Write(x + " ");
        }

        Console.WriteLine();
    }

    private static void Write(CarPath path)
    {
        foreach (var point in path)
        {
            Console.Write(point + " ");
        }

        Console.WriteLine();
    }

    private static void Write<T>(List<T> list)
    {
        foreach (var x in list)
        {
            Console.Write(x + " ");
        }

        Console.WriteLine();
    }
}