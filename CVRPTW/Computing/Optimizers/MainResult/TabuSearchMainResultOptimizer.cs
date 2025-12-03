using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class TabuSearchMainResultOptimizer : MainResultOptimizer
{
    private readonly IMainResultEstimator _estimator;
    private readonly MainData _mainData;
    private readonly int _maxIterations;
    private readonly int _tabuTenure;
    private readonly int _neighborhoodSize;
    
    private readonly Dictionary<string, int> _tabuList = new();
    private MainResult? _bestSolution;
    private double _bestEstimation;

    public TabuSearchMainResultOptimizer(IMainResultEstimator estimator, MainData mainData, int maxIterations = 100,
        int tabuTenure = 10, int neighborhoodSize = 20)
    {
        _estimator = estimator;
        _mainData = mainData;
        _maxIterations = maxIterations;
        _tabuTenure = tabuTenure;
        _neighborhoodSize = neighborhoodSize;
    }

    public override void Optimize(MainResult mainResult)
    {
        _bestSolution = mainResult.Clone();
        _bestEstimation = _estimator.Estimate(_bestSolution);
        _tabuList.Clear();

        var currentSolution = mainResult.Clone();
        var currentEstimation = _bestEstimation;

        for (int iteration = 0; iteration < _maxIterations; iteration++)
        {
            var neighbors = GenerateNeighborhood(currentSolution);
            
            if (neighbors.Count == 0) break;

            var (bestNeighbor, bestMove, bestNeighborEstimation) = SelectBestNeighbor(neighbors, currentEstimation);

            var aspirationCriterion = bestNeighborEstimation < _bestEstimation;
            
            if (!IsTabu(bestMove) || aspirationCriterion)
            {
                currentSolution = bestNeighbor;
                currentEstimation = bestNeighborEstimation;
                
                AddToTabuList(bestMove, iteration);

                if (currentEstimation < _bestEstimation)
                {
                    _bestSolution = currentSolution.Clone();
                    _bestEstimation = currentEstimation;
                }
            }
            
            CleanTabuList(iteration);
        }

        CopySolution(_bestSolution, mainResult);
        
        _estimator.Estimate(mainResult);
    }

    private List<(MainResult solution, Move move)> GenerateNeighborhood(MainResult current)
    {
        var neighbors = new List<(MainResult, Move)>();
        var random = new Random();
        var attempts = 0;
        var maxAttempts = _neighborhoodSize * 3;

        while (neighbors.Count < _neighborhoodSize && attempts < maxAttempts)
        {
            attempts++;
            
            var moveType = random.Next(4);
            var neighbor = current.Clone();
            Move? move = null;

            move = moveType switch
            {
                0 => TrySwapMove(neighbor, random),
                1 => TryInsertMove(neighbor, random),
                2 => TryReverseMove(neighbor, random),
                3 => TryTransferMove(neighbor, random),
                _ => null
            };

            if (move != null)
            {
                neighbors.Add((neighbor, move));
            }
        }

        return neighbors;
    }

    private Move? TrySwapMove(MainResult solution, Random random)
    {
        var cars = solution.Results.Keys.ToList();
        var car = cars[random.Next(cars.Count)];
        var path = solution.Results[car].Path;

        if (path.Count <= 3) return null;

        var i = random.Next(1, path.Count - 1);
        var j = random.Next(1, path.Count - 1);
        
        if (i == j) return null;

        (path[i], path[j]) = (path[j], path[i]);

        return new Move
        {
            Type = MoveType.Swap,
            CarId = car.Id,
            Index1 = i,
            Index2 = j
        };
    }

    private Move? TryInsertMove(MainResult solution, Random random)
    {
        var cars = solution.Results.Keys.ToList();
        var car = cars[random.Next(cars.Count)];
        var path = solution.Results[car].Path;

        if (path.Count <= 3) return null;

        var fromIndex = random.Next(1, path.Count - 1);
        var toIndex = random.Next(1, path.Count);
        
        if (fromIndex == toIndex || fromIndex == toIndex - 1) return null;

        var point = path.TakeAt(fromIndex);
        path.Insert(toIndex > fromIndex ? toIndex - 1 : toIndex, point);

        return new Move
        {
            Type = MoveType.Insert,
            CarId = car.Id,
            Index1 = fromIndex,
            Index2 = toIndex
        };
    }

    private Move? TryReverseMove(MainResult solution, Random random)
    {
        var cars = solution.Results.Keys.ToList();
        var car = cars[random.Next(cars.Count)];
        var path = solution.Results[car].Path;

        if (path.Count <= 4) return null;

        var start = random.Next(1, path.Count - 2);
        var end = random.Next(start + 1, path.Count - 1);

        path.Reverse(start, end);

        return new Move
        {
            Type = MoveType.Reverse,
            CarId = car.Id,
            Index1 = start,
            Index2 = end
        };
    }

    private Move? TryTransferMove(MainResult solution, Random random)
    {
        if (_mainData.Cars.Count < 2) return null;

        var cars = solution.Results.Keys.ToList();
        var sourceCar = cars[random.Next(cars.Count)];
        var sourcePath = solution.Results[sourceCar].Path;

        if (sourcePath.Count <= 2) return null;

        var targetCar = cars[random.Next(cars.Count)];
        while (targetCar == sourceCar)
            targetCar = cars[random.Next(cars.Count)];

        var targetPath = solution.Results[targetCar].Path;
        var sourceIndex = random.Next(1, sourcePath.Count - 1);
        var targetIndex = random.Next(1, targetPath.Count);

        var point = sourcePath.TakeAt(sourceIndex);
        targetPath.Insert(targetIndex, point);

        return new Move
        {
            Type = MoveType.Transfer,
            CarId = sourceCar.Id,
            CarId2 = targetCar.Id,
            Index1 = sourceIndex,
            Index2 = targetIndex
        };
    }

    private (MainResult best, Move move, double estimation) SelectBestNeighbor(
        List<(MainResult solution, Move move)> neighbors,
        double currentEstimation)
    {
        MainResult? bestNeighbor = null;
        Move? bestMove = null;
        var bestEstimation = double.MaxValue;

        foreach (var (neighbor, move) in neighbors)
        {
            var estimation = _estimator.Estimate(neighbor);

            if (estimation < bestEstimation)
            {
                bestEstimation = estimation;
                bestNeighbor = neighbor;
                bestMove = move;
            }
        }

        return (bestNeighbor!, bestMove!, bestEstimation);
    }

    private bool IsTabu(Move move)
    {
        return _tabuList.ContainsKey(move.GetKey());
    }

    private void AddToTabuList(Move move, int iteration)
    {
        _tabuList[move.GetKey()] = iteration + _tabuTenure;
    }

    private void CleanTabuList(int currentIteration)
    {
        var expiredMoves = _tabuList
            .Where(kvp => kvp.Value <= currentIteration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var move in expiredMoves)
        {
            _tabuList.Remove(move);
        }
    }

    private void CopySolution(MainResult source, MainResult target)
    {
        foreach (var car in source.Results.Keys)
        {
            var sourcePath = source.Results[car].Path;
            var targetPath = target.Results[car].Path;

            targetPath.Clear();
            targetPath.SetPath(sourcePath.ToArray());
        }
    }

    private enum MoveType
    {
        Swap,
        Insert,
        Reverse,
        Transfer
    }

    private class Move
    {
        public MoveType Type { get; set; }
        public int CarId { get; set; }
        public int? CarId2 { get; set; }
        public int Index1 { get; set; }
        public int Index2 { get; set; }

        public string GetKey()
        {
            return CarId2.HasValue
                ? $"{Type}_{CarId}_{CarId2}_{Index1}_{Index2}"
                : $"{Type}_{CarId}_{Index1}_{Index2}";
        }
    }
}