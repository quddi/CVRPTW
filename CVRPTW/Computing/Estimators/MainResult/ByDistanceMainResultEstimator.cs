namespace CVRPTW.Computing.Estimators;

public class ByDistanceMainResultEstimator(MainData mainData) : PathsIteratedMainResultEstimator
{
    protected override double Estimate(Car _, CarResult carResult)
    {
        carResult.Estimation = Estimate(carResult.Path);
        
        return carResult.Estimation;
    }

    public double Estimate(CarPath path)
    {
        if (path.Count <= 2) return 0d;
        
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