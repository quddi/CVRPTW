using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing;

public class DistanceMainComputer(MainData mainData, IMainResultEstimator mainResultEstimator) : ByPairsMainComputer(mainData, mainResultEstimator)
{
    protected override double EstimatePair(int checkedPointId, int currentPointId)
    {
        var firstPointIndex = _mainData.GetPoint(currentPointId).Index;
        var secondPointIndex = _mainData.GetPoint(checkedPointId).Index;
        return _mainData.Distances!.GetDistance(Constants.DefaultMatrixId, firstPointIndex, secondPointIndex);
    }
}