namespace DistanceMatrixComputing;

public interface IDistanceMatrixCalculator
{
    float[,] ComputeDistanceMatrix(IReadOnlyList<Point2D> points);

    float[,] ComputeDistanceMatrix(IReadOnlyList<(float X, float Y)> points);

    float[,] ComputeDistanceMatrix(IReadOnlyList<(double X, double Y)> points);

    float[,] ComputeDistanceMatrix<T>(IReadOnlyList<T> items, Func<T, Point2D> pointSelector);

    float[,] ComputeDistanceMatrix<T>(IReadOnlyList<T> items, Func<T, (float X, float Y)> coordinateSelector);

    float[,] ComputeDistanceMatrix<T>(IReadOnlyList<T> items, Func<T, (double X, double Y)> coordinateSelector);
}
