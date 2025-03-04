namespace CVRPTW.Computing.Estimators;

//TODO: Test, no triangles

public class EuclidesPathEstimator(MainData mainData) : PathEstimator(mainData)
{
    public override double Estimate(CarPath path)
    {
        var sum = 0d;

        for (int i = 0; i < path.Count - 1; i++)
        {
            var firstPointResult = path[i];
            var secondPointResult = path[i + 1];

            sum += Estimate(firstPointResult.Id, secondPointResult.Id);
        }

        return sum;
    }

    public override double Estimate(int[] path)
    {
        var sum = 0d;

        for (int i = 0; i < path.Length - 1; i++)
        {
            sum += Estimate(path[i], path[i + 1]);
        }

        return sum;
    }

    public override double Estimate(int firstPointId, int secondPointId)
    {
        var firstPoint = _mainData.GetPoint(firstPointId);
        var secondPoint = _mainData.GetPoint(secondPointId);

        return firstPoint.Coordinates.DistanceTo(secondPoint.Coordinates);
    }
}