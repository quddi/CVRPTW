namespace CVRPTW.Computing.Estimators;

public abstract class MainResultEstimator(MainData mainData, PathEstimator pathEstimator)
{
   protected readonly MainData _mainData = mainData;
   public readonly PathEstimator PathEstimator = pathEstimator;

   public abstract double Estimate(MainResult mainResult);
}