namespace DistanceMatrixComputing;

public static class MatrixExtensions
{
    public static float[][] ToJagged(this float[,] matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var result = new float[rows][];
        for (int i = 0; i < rows; i++)
        {
            var row = new float[cols];
            for (int j = 0; j < cols; j++)
                row[j] = matrix[i, j];
            result[i] = row;
        }
        return result;
    }
}
