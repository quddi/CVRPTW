using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public class DistanceMainComputer(MainData mainData, IMainResultEstimator mainResultEstimator) : ByPairsMainComputer(mainData, mainResultEstimator)
{
    protected override bool TryEstimatePair(int checkedPointId, int currentPointId, out double estimation)
    {
        var firstPointIndex = _mainData.GetPoint(currentPointId).Index;
        var secondPointIndex = _mainData.GetPoint(checkedPointId).Index;
        return _mainData.Distances!.TryGetDistance(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex, out estimation);
    }
}