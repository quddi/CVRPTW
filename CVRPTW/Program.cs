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
        var computer = new StartPathComputer(mainData, estimator);

        var optimizedResult = computer.Compute();
        var startResult = optimizedResult.Clone();

        Console.WriteLine(optimizedResult);


        Opt2(estimator, optimizedResult);
        
        Console.WriteLine(optimizedResult);

        Opt3(estimator, optimizedResult);
        
        Console.WriteLine(optimizedResult);

        CheckPermutations(optimizedResult, startResult);
        
        PointTranspose(estimator, optimizedResult, mainData);

        Console.WriteLine(optimizedResult);
        
        ReEstimate(optimizedResult, estimator);

        Console.WriteLine(optimizedResult);
    }

    private static void ReEstimate(MainResult optimizedResult, DistancePathEstimator estimator)
    {
        Console.WriteLine("====================== ReEstimation ======================\n");

        foreach (var (car, carResult) in optimizedResult.Results)
        {
            if (carResult.Path.Count <= 2) continue;
            
            carResult.ReEstimate(estimator);
        }
    }

    private static void CheckPermutations(MainResult optimizedResult, MainResult startResult)
    {
        Console.WriteLine("====================== Check permutations ======================\n");
        
        if (AreResultsPathsPermutations(optimizedResult, startResult))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Paths are permutations!\n");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Paths are not permutations!\n");
        }

        Console.ResetColor();
    }

    private static void PointTranspose(DistancePathEstimator estimator, MainResult optimizedResult, MainData mainData)
    {
        Console.WriteLine("====================== Point Transpose ======================\n");
        
        var mainResultOptimizer = new PointTransposeMainResultOptimizer(estimator, mainData);
        
        mainResultOptimizer.Optimize(optimizedResult);
    }

    private static void Opt2(DistancePathEstimator estimator, MainResult optimizedResult)
    {
        Console.WriteLine("====================== Opt 2 ======================\n");

        var optimizer = new Opt2CarResultOptimizer(estimator);

        foreach (var carResult in optimizedResult.Results.Values)
        {
            if (carResult.Path.Count <= 4) continue;

            optimizer.Optimize(carResult);
        }
    }

    private static void Opt3(DistancePathEstimator estimator, MainResult optimizedResult)
    {
        Console.WriteLine("====================== Opt 3 ======================\n");
        
        var optimizer = new Opt3CarResultOptimizer(estimator);

        foreach (var carResult in optimizedResult.Results.Values)
        {
            if (carResult.Path.Count <= 6) continue;

            optimizer.Optimize(carResult);
        }
    }

    private static bool AreResultsPathsPermutations(MainResult optimizedResult, MainResult startResult)
    {
        return optimizedResult.Results.Select(pair => pair.Value.Path)
            .Zip(startResult.Results.Select(pair => pair.Value.Path))
            .All(pair => ExtensionsMethods.ArePermutations(pair.First, pair.Second));
    }
}