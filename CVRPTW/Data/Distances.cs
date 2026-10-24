namespace CVRPTW;

public class Distances(int pointsCount)
{
    private readonly Dictionary<int, DistanceMatrix> _matrices = new();

    public int MatricesCount => _matrices.Count;

    public double GetDistance(int matrixIndex, int firstPointIndex, int secondPointIndex)
    {
        if (!TryGetDistance(matrixIndex, firstPointIndex, secondPointIndex, out var distance))
            throw new KeyNotFoundException($"Distance from {firstPointIndex} to {secondPointIndex} in matrix {matrixIndex} is not defined.");

        return distance;
    }

    public bool TryGetDistance(int matrixIndex, int firstPointIndex, int secondPointIndex, out double distance)
    {
        distance = 0d;

        if (!_matrices.TryGetValue(matrixIndex, out var matrix)) return false;

        var position = GetPosition(firstPointIndex, secondPointIndex);

        if (!matrix.IsReachable[position]) return false;

        distance = matrix.Values[position];

        return true;
    }

    public void AddDistance(int matrixIndex, int firstPointIndex, int secondPointIndex, double distance)
    {
        if (!_matrices.TryGetValue(matrixIndex, out var matrix))
        {
            matrix = new DistanceMatrix(pointsCount);
            _matrices[matrixIndex] = matrix;
        }

        var position = GetPosition(firstPointIndex, secondPointIndex);

        matrix.Values[position] = distance;
        matrix.IsReachable[position] = true;
    }

    public double GetMaxDistance()
    {
        var maxDistance = 0d;

        foreach (var matrix in _matrices.Values)
        {
            for (int position = 0; position < matrix.Values.Length; position++)
            {
                if (matrix.IsReachable[position] && matrix.Values[position] > maxDistance)
                    maxDistance = matrix.Values[position];
            }
        }

        return maxDistance;
    }

    private int GetPosition(int firstPointIndex, int secondPointIndex)
    {
        if (firstPointIndex < 0 || firstPointIndex >= pointsCount || secondPointIndex < 0 || secondPointIndex >= pointsCount)
            throw new ArgumentOutOfRangeException($"Point indexes {firstPointIndex}, {secondPointIndex} are outside of 0..{pointsCount - 1}. POINTS section must be parsed before DISTANCE.");

        return firstPointIndex * pointsCount + secondPointIndex;
    }

    private sealed class DistanceMatrix(int size)
    {
        public readonly double[] Values = new double[size * size];

        public readonly bool[] IsReachable = new bool[size * size];
    }
}
