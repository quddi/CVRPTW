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

        Console.WriteLine(mainData);
    }
}