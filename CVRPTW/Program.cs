namespace CVRPTW;

static class Program
{
    private static void Main()
    {
        var arr = Enumerable.Range(0, 16).ToList();
        
        arr.Insert(arr.Count, 100);
        
        foreach (var i in arr)
        {
            Console.Write(i + " ");
        }

        Console.WriteLine();
        
        arr.SwapSegments(1, 5, 10, 12);
        
        foreach (var i in arr)
        {
            Console.Write(i + " ");
        }
        /*var path = @"C:\Users\Admin\Desktop\Диплом\original.txt";
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

        CarResultOptimizer optimizer = new Opt2CarResultOptimizer(estimator);

        foreach (var result in results)
        {
            if (result.Path.Length <= 2) continue;

            optimizer.Optimize(result);
        }

        Console.WriteLine(results.ToString<CarResult>());

        Console.WriteLine("\n===========================================\n");

        optimizer = new Opt3NewCarResultOptimizer(estimator);

        foreach (var result in results)
        {
            if (result.Path.Length <= 2) continue;

            optimizer.Optimize(result);
        }

        Console.WriteLine(results.ToString<CarResult>());*/
    }
}