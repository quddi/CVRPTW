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
            
            new AlternativePointsMainResultOptimizer(mainResultEstimator, mainData).Optimize(mainResult);

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