using CVRPTW.Computing;

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

        var result = Singleton.OfType<SimplePathComputer>().Compute(mainData);
        
        Console.WriteLine(string.Join("\n", result));
    }
}