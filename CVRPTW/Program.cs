using System.Diagnostics;
using CVRPTW.Computing;
using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Estimators.Time;
using CVRPTW.Computing.Optimizers;

namespace CVRPTW;

static class Program
{
    private static readonly string[] _paths =
    [
        @"C:\Users\Admin\Desktop\Диплом\original1.txt",
        @"C:\Users\Admin\Desktop\Диплом\WC_0202_14_11092024AlternativePoints (2).txt",
        @"C:\Users\Admin\Desktop\Диплом\0060_319 (3).txt",
        @"C:\Users\Admin\Desktop\Диплом\WC_0027_51 (4).txt",
        @"C:\Users\Admin\Desktop\Диплом\WC_0080_test_3542_926_v2 (5).txt",
        @"C:\Users\Admin\Desktop\Диплом\0065_196 (6).txt",
    ];
    
    private static void Main()
    {
        EstimateIterationsEfficiency();

    }

    private static void GetAlternativePoints(int pathIndex)
    {
        var path = _paths[pathIndex];
        
        var mainParser = new MainParser();
        var mainData = mainParser.Parse(new StreamReader(path));

        var alternativePoints = mainData.Cars.Select(_ => new List<int>()).ToList();
        var notUsedPoints = mainData.Points.Select(point => point.Id).Where(id => id > 0).ToList();
        var currentCarIndex = 0;
        var maxAlternativePointsCount = 4;

        while (notUsedPoints.Count != 0)
        {
            if (alternativePoints[currentCarIndex].Count > maxAlternativePointsCount) break;
            
            alternativePoints[currentCarIndex].Add(notUsedPoints.SnatchRandom());
            
            currentCarIndex = (currentCarIndex + 1) % mainData.Cars.Count;
        }
        
        Console.WriteLine("======ALTERNATIVE_POINTS=====");
        
        foreach (var alternativePointsList in alternativePoints)
        {
            for (var i = 0; i < alternativePointsList.Count; i++)
            {
                Console.Write(alternativePointsList[i]);
                
                if (i != alternativePointsList.Count - 1) Console.Write(",");
            }
            
            Console.WriteLine();
        }
    }
    
    private static void EstimateOneIterationEfficiency()
    {
        var mainParser = new MainParser();

        for (var i = 0; i < _paths.Length; i++)
        {
            var path = _paths[i];
            var mainData = mainParser.Parse(new StreamReader(path));

            var pathEstimator = new ByDistanceMainResultEstimator(mainData!);
            var timeEstimator = new SimpleTimeEstimator(mainData!);
            var mainResultEstimator = new ComplexMainResultEstimator(mainData!, pathEstimator, timeEstimator);
            var startMainComputer = new DistanceMainComputer(mainData!, mainResultEstimator);
            var optimizer = new Opt3CarResultOptimizer(mainResultEstimator);

            var mainResult = startMainComputer.Compute();

            var stopwatch = Stopwatch.StartNew();
            
            optimizer.Optimize(mainResult);
            
            stopwatch.Stop();

            Console.WriteLine($"=== {i} {stopwatch.ElapsedMilliseconds} ===");
        }
    }

    private static void EstimateIterationsEfficiency()
    {
        var mainParser = new MainParser();

        for (var i = 0; i < _paths.Length; i++)
        {
            var path = _paths[i];
            var mainData = mainParser.Parse(new StreamReader(path));

            var pathEstimator = new ByDistanceMainResultEstimator(mainData!);
            var timeEstimator = new SimpleTimeEstimator(mainData!);
            var mainResultEstimator = new ComplexMainResultEstimator(mainData!, pathEstimator, timeEstimator);
            var startMainComputer = new DistanceMainComputer(mainData!, mainResultEstimator);
            var optimizer = ExtensionsMethods.GetAlternativeOptimizer(mainResultEstimator, mainData);
            optimizer.Optimizers.RemoveLast();
            
            var mainResult = startMainComputer.Compute();
            
            var estimation = EstimateIterationsEfficiency(mainResult, optimizer);

            Console.WriteLine($"=== {i} {estimation} ===");
        }
    }

    private static int EstimateIterationsEfficiency(MainResult result, MainResultOptimizer optimizer)
    {
        var current = result.Estimation;
        var iterations = 0;

        for (int i = 0; i < 100; i++)
        {
            optimizer.Optimize(result);

            if (current - result.Estimation < 1) break;
            
            iterations++;
            current = result.Estimation;

            Console.WriteLine(iterations);
        }
        
        return iterations;
    }
}