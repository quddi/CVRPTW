using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class DeterministicAnnealingOptimizer : MainResultOptimizer
{
    private readonly IMainResultEstimator _estimator;
    private readonly MainData _mainData;
    private readonly Random _random;
    private readonly DeterministicAnnealingConfig _config;

    public DeterministicAnnealingOptimizer(
        IMainResultEstimator estimator,
        MainData mainData,
        DeterministicAnnealingConfig? config = null)
    {
        _estimator = estimator;
        _mainData = mainData;
        _random = new Random();
        _config = config ?? new DeterministicAnnealingConfig();
    }

    public override void Optimize(MainResult mainResult)
    {
        var currentSolution = mainResult;
        var bestSolution = CloneSolution(mainResult);
        var bestCost = _estimator.Estimate(bestSolution);

        var temperature = _config.InitialTemperature;
        var iteration = 0;

        while (temperature > _config.FinalTemperature && iteration < _config.MaxIterations)
        {
            for (int attempt = 0; attempt < _config.AttemptsPerTemperature; attempt++)
            {
                var neighbor = GenerateNeighbor(currentSolution);
                if (neighbor == null) continue;
                    
                var neighborCost = _estimator.Estimate(neighbor);
                var currentCost = _estimator.Estimate(currentSolution);

                var deltaE = neighborCost - currentCost;
                var acceptanceProbability = CalculateAcceptanceProbability(deltaE, temperature);

                if (!(acceptanceProbability > _random.NextDouble())) continue;
                    
                currentSolution = neighbor;

                if (!(neighborCost < bestCost)) continue;
                    
                bestSolution = CloneSolution(neighbor);
                bestCost = neighborCost;
            }

            temperature = CoolDown(temperature, iteration);
            iteration++;
        }

        CopySolution(bestSolution, mainResult);
        _estimator.Estimate(mainResult);
    }

    private double CalculateAcceptanceProbability(double deltaE, double temperature)
    {
        return deltaE <= 0 
            ? 1.0 
            : 1.0 / (1.0 + Math.Exp(deltaE / temperature));
    }

    private double CoolDown(double currentTemperature, int iteration)
    {
        return _config.CoolingSchedule switch
        {
            CoolingScheduleType.Geometric => currentTemperature * _config.CoolingRate,
            CoolingScheduleType.Linear => _config.InitialTemperature - (iteration * _config.LinearStep),
            CoolingScheduleType.Logarithmic => _config.InitialTemperature / Math.Log(2 + iteration),
            _ => currentTemperature * _config.CoolingRate
        };
    }

    private MainResult? GenerateNeighbor(MainResult current)
    {
        var neighbor = CloneSolution(current);
        var operatorType = _random.Next(0, 5);

        switch (operatorType)
        {
            case 0:
                if (!ApplyIntraRouteSwap(neighbor)) return null;
                break;
            case 1:
                if (!ApplyInterRouteSwap(neighbor)) return null;
                break;
            case 2:
                if (!ApplyRelocate(neighbor)) return null;
                break;
            case 3:
                if (!Apply2Opt(neighbor)) return null;
                break;
            case 4:
                if (!ApplyReverseSegment(neighbor)) return null;
                break;
        }

        return neighbor;
    }

    private bool ApplyIntraRouteSwap(MainResult solution)
    {
        var carsWithRoutes = solution.Results
            .Where(r => r.Value.Path.Count > 3)
            .Select(r => r.Key)
            .ToList();
                
        if (carsWithRoutes.Count == 0) return false;

        var car = carsWithRoutes[_random.Next(carsWithRoutes.Count)];
        var path = solution.Results[car].Path;

        var validIndices = new List<int>();
        
        for (int i = 1; i < path.Count - 1; i++)
        {
            if (!path[i].Id.IsDepoId()) validIndices.Add(i);
        }
            
        if (validIndices.Count < 2) return false;

        var k = validIndices[_random.Next(validIndices.Count)];
        var m = validIndices[_random.Next(validIndices.Count)];

        if (k == m) return true;
        
        (path[k], path[m]) = (path[m], path[k]);

        return true;
    }

    private bool ApplyInterRouteSwap(MainResult solution)
    {
        var carsWithRoutes = solution.Results
            .Where(r => r.Value.Path.Count > 2)
            .Select(r => r.Key)
            .ToList();
                
        if (carsWithRoutes.Count < 2) return false;

        var car1 = carsWithRoutes[_random.Next(carsWithRoutes.Count)];
        var car2 = carsWithRoutes[_random.Next(carsWithRoutes.Count)];

        if (car1 == car2) return false;

        var path1 = solution.Results[car1].Path;
        var path2 = solution.Results[car2].Path;

        if (path1.Count <= 2 || path2.Count <= 2) return false;

        var validIndices1 = new List<int>();
        
        for (int i = 1; i < path1.Count - 1; i++)
        {
            if (!path1[i].Id.IsDepoId()) validIndices1.Add(i);
        }
            
        var validIndices2 = new List<int>();
        
        for (int i = 1; i < path2.Count - 1; i++)
        {
            if (!path2[i].Id.IsDepoId()) validIndices2.Add(i);
        }
            
        if (validIndices1.Count == 0 || validIndices2.Count == 0) return false;

        var k = validIndices1[_random.Next(validIndices1.Count)];
        var m = validIndices2[_random.Next(validIndices2.Count)];

        (path1[k], path2[m]) = (path2[m], path1[k]);
            
        return true;
    }

    private bool ApplyRelocate(MainResult solution)
    {
        var carsWithRoutes = solution.Results
            .Where(r => r.Value.Path.Count > 2)
            .Select(r => r.Key)
            .ToList();
                
        if (carsWithRoutes.Count < 2) return false;

        var sourceCar = carsWithRoutes[_random.Next(carsWithRoutes.Count)];
        var targetCar = carsWithRoutes[_random.Next(carsWithRoutes.Count)];

        var sourcePath = solution.Results[sourceCar].Path;
        var targetPath = solution.Results[targetCar].Path;

        if (sourcePath.Count <= 2) return false;

        var validSourceIndices = new List<int>();
        
        for (int i = 1; i < sourcePath.Count - 1; i++)
        {
            if (!sourcePath[i].Id.IsDepoId()) validSourceIndices.Add(i);
        }
            
        if (validSourceIndices.Count == 0) return false;

        var sourceIndex = validSourceIndices[_random.Next(validSourceIndices.Count)];
        var targetIndex = _random.Next(1, targetPath.Count);
        var point = sourcePath.TakeAt(sourceIndex);
        
        targetPath.Insert(targetIndex, point);
            
        return true;
    }

    private bool Apply2Opt(MainResult solution)
    {
        var carsWithRoutes = solution.Results
            .Where(r => r.Value.Path.Count > 3)
            .Select(r => r.Key)
            .ToList();
                
        if (carsWithRoutes.Count == 0) return false;

        var car = carsWithRoutes[_random.Next(carsWithRoutes.Count)];
        var path = solution.Results[car].Path;

        if (path.Count <= 3) return false;

        var i = _random.Next(1, path.Count - 2);
        var j = _random.Next(i + 1, path.Count - 1);

        if (j - i <= 1) return true;
        
        path.Reverse(i, j - 1);

        return true;
    }

    private bool ApplyReverseSegment(MainResult solution)
    {
        var carsWithRoutes = solution.Results
            .Where(r => r.Value.Path.Count > 3)
            .Select(r => r.Key)
            .ToList();
                
        if (carsWithRoutes.Count == 0) return false;

        var car = carsWithRoutes[_random.Next(carsWithRoutes.Count)];
        var path = solution.Results[car].Path;

        if (path.Count <= 3) return false;

        var start = _random.Next(1, path.Count - 2);
        var end = _random.Next(start + 1, path.Count - 1);

        path.Reverse(start, end);
            
        return true;
    }

    private MainResult CloneSolution(MainResult original)
    {
        var clone = new MainResult
        {
            Estimation = original.Estimation,
            Results = new Dictionary<Car, CarResult>()
        };

        foreach (var (car, carResult) in original.Results)
        {
            clone.Results[car] = new CarResult(car)
            {
                Path = carResult.Path.Clone(),
                Estimation = carResult.Estimation
            };
        }

        return clone;
    }

    private void CopySolution(MainResult source, MainResult destination)
    {
        destination.Estimation = source.Estimation;

        foreach (var (car, sourceCarResult) in source.Results)
        {
            if (destination.Results.TryGetValue(car, out var destCarResult))
            {
                destCarResult.Path.SetPath(sourceCarResult.Path.ToArray());
                destCarResult.Estimation = sourceCarResult.Estimation;
            }
            else
            {
                destination.Results[car] = new CarResult(car)
                {
                    Path = sourceCarResult.Path.Clone(),
                    Estimation = sourceCarResult.Estimation
                };
            }
        }
    }
}