namespace CVRPTW.Computing.Estimators;

public class DistancePathCostEstimator(MainData mainData) : IPathCostEstimator
{
    public double Estimate(CarPath path)
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

    private double Estimate(int firstPointId, int secondPointId)
    {
        var firstPointIndex = mainData.GetPoint(firstPointId).Index;
        var secondPointIndex = mainData.GetPoint(secondPointId).Index;
        
        return mainData.Distances!.GetDistance(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex);
    }
}