using DistanceMatrixComputing;

namespace Benchmarks.Common;

public static class TestDataHelper
{
    public static Point2D[] GeneratePoints(int count, int seed = 42)
    {
        var random = new Random(seed);
        var points = new Point2D[count];
        for (int i = 0; i < count; i++)
        {
            points[i] = new Point2D((float)(random.NextDouble() * 100), (float)(random.NextDouble() * 100));
        }
        return points;
    }
}
