namespace DistanceMatrixComputing;

public interface IDistanceMatrixCalculator
{
    double[,] ComputeDistanceMatrix(IReadOnlyList<Point2D> points);

    double[,] ComputeDistanceMatrix(IReadOnlyList<(double X, double Y)> points);

    double[,] ComputeDistanceMatrix<T>(IReadOnlyList<T> items, Func<T, Point2D> pointSelector);

    double[,] ComputeDistanceMatrix<T>(IReadOnlyList<T> items, Func<T, (double X, double Y)> coordinateSelector);
}
