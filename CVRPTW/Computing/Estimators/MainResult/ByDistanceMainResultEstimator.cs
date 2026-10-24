namespace CVRPTW.Computing.Estimators;

public class ByDistanceMainResultEstimator(MainData mainData) : PathsIteratedMainResultEstimator
{
    private readonly double _unreachablePenalty = CalculateUnreachablePenalty(mainData);

    protected override double Estimate(Car _, CarResult carResult)
    {
        carResult.Estimation = Estimate(carResult.Path, out var unreachableEdgesCount);
        carResult.UnreachableEdgesCount = unreachableEdgesCount;

        return carResult.Estimation;
    }

    public double Estimate(CarPath path)
    {
        return Estimate(path, out _);
    }

    private double Estimate(CarPath path, out int unreachableEdgesCount)
    {
        unreachableEdgesCount = 0;

        if (path.Count <= 2) return 0d;

        var sum = 0d;

        for (int i = 0; i < path.Count - 1; i++)
        {
            var firstPointResult = path[i];
            var secondPointResult = path[i + 1];

            if (TryEstimate(firstPointResult.Id, secondPointResult.Id, out var distance))
            {
                sum += distance;
                continue;
            }

            sum += _unreachablePenalty;
            unreachableEdgesCount++;
        }

        return sum;
    }

    private bool TryEstimate(int firstPointId, int secondPointId, out double distance)
    {
        var firstPointIndex = mainData.GetPoint(firstPointId).Index;
        var secondPointIndex = mainData.GetPoint(secondPointId).Index;

        return mainData.Distances!.TryGetDistance(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex, out distance);
    }

    private static double CalculateUnreachablePenalty(MainData mainData)
    {
        if (mainData.Distances is null) return 0d;

        var maxDistance = mainData.Distances.Matrix.Values
            .SelectMany(firstPointsDistances => firstPointsDistances.Values)
            .SelectMany(secondPointsDistances => secondPointsDistances.Values)
            .DefaultIfEmpty(0d)
            .Max();

        return maxDistance * (mainData.Points.Count + 1);
    }
}