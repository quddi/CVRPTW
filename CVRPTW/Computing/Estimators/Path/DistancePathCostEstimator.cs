namespace CVRPTW.Computing.Estimators;

public class DistancePathCostEstimator(MainData mainData) : PathCostEstimator(mainData)
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

    private double Estimate(int[] path)
    {
        var sum = 0d;

        for (int i = 0; i < path.Length - 1; i++)
        {
            sum += Estimate(path[i], path[i + 1]);
        }

        return sum;
    }

    private double Estimate(int firstPointId, int secondPointId)
    {
        var firstPointIndex = _idToIndex[firstPointId];
        var secondPointIndex = _idToIndex[secondPointId];
        
        return _mainData.Distances!.GetDistance(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex);
    }
}