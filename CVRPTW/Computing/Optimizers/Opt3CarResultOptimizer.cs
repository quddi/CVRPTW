using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class Opt3CarResultOptimizer(PathEstimator pathEstimator) : CarResultOptimizer(pathEstimator)
{
    private readonly PathEstimator _pathEstimator = pathEstimator;
    
    public override void Optimize(CarResult carResult)
    {
        
    }

    // a, b, c - ребра, aStart = i, aEnd = i +1
    private void CheckAllVariants(CarPath path, int aStart, int bStart, int cStart)
    {
        //1 (2-opt)
        path.Reverse(aStart + 1, bStart);
        
        //2 (2-opt)
        path.Reverse(bStart + 1, cStart);
        
        //3 (2-opt)
        path.Reverse(aStart + 1, cStart);
        
        //4
        //path.Reverse(aStart + 1, );
    }
}
