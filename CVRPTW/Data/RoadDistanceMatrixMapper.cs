namespace CVRPTW;

public static class RoadDistanceMatrixMapper
{
    public static Distances ToDistances(RoadDistanceMatrix roadMatrix, MainData mainData)
    {
        var size = roadMatrix.PointIds.Length;

        if (roadMatrix.Values.GetLength(0) != size || roadMatrix.Values.GetLength(1) != size)
            throw new ArgumentException($"Matrix must be {size}x{size} to match the number of point ids.");

        var pointIndexes = roadMatrix.PointIds.Select(pointId => GetPointIndex(pointId, mainData)).ToArray();

        if (pointIndexes.Distinct().Count() != size)
            throw new ArgumentException("Point ids in the matrix must be unique.");

        var distances = new Distances(mainData.Points.Count);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var distance = roadMatrix.Values[i, j];

                // Недосяжний перехід не записується, щоб TryGetDistance повернув false.
                if (distance is null) continue;

                if (!double.IsFinite(distance.Value) || distance.Value < 0)
                    throw new ArgumentException($"Invalid distance {distance} from point {roadMatrix.PointIds[i]} to point {roadMatrix.PointIds[j]}.");

                distances.AddDistance(Constants.DefaultMatrixId, pointIndexes[i], pointIndexes[j], distance.Value);
            }
        }

        return distances;
    }

    private static int GetPointIndex(int pointId, MainData mainData)
    {
        var points = pointId.IsDepoId() ? mainData.DepoPointsByIds : mainData.PointsByIds;

        if (!points.TryGetValue(pointId, out var point))
            throw new ArgumentException($"Point {pointId} is not defined in POINTS.");

        return point.Index;
    }
}
