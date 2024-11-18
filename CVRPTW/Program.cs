using CVRPTW.Computing;
using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Optimizers;

namespace CVRPTW;

static class Program
{
    private static void Main()
    {
        //1) Ignore alternative points, nearest neighbour method
        //2opt) Покращити 1, 
        var path = @"C:\Users\Admin\Desktop\Диплом\original.txt";
        MainData mainData;
        using (var streamReader = new StreamReader(path))
        {
            mainData = Singleton.OfType<MainParser>().Parse(streamReader);
        }

        var results = Singleton.OfType<NearestPathComputer>().Compute(mainData);
        var optimizer = new CarResultOptimizer(new DistancePathEstimator(mainData));

        foreach (var carResult in results)
        {
            Console.WriteLine($"Default: {carResult}");
            
            optimizer.Optimize(carResult);
            
            Console.WriteLine($"Optimized: {carResult}");

            Console.WriteLine();
        }
    }
}