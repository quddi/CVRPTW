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
        using (var streamReader = new StreamReader(path))
        {
            mainData = Singleton.OfType<MainParser>().Parse(streamReader);
        }
        var estimator = new DistancePathEstimator(mainData);

        var results = Singleton.OfType<NearestPathComputer>().Compute(mainData, estimator);
        var optimizer2 = new Opt2CarResultOptimizer(estimator);
        var optimizer3 = new Opt3CarResultOptimizer(estimator);

        foreach (var carResult in results)
        {
            var clone = carResult.Clone();
            
            Console.WriteLine($"Default: {carResult}");
            
            optimizer2.Optimize(carResult);
            
            Console.WriteLine($"Optimized 2: {carResult}");
            
            optimizer3.Optimize(clone);
            
            Console.WriteLine($"Optimized 3: {clone}");

            Console.WriteLine();
        }
    }
}