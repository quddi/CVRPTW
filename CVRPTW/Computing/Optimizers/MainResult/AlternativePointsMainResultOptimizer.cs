using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

public class AlternativePointsMainResultOptimizer(IMainResultEstimator mainResultEstimator, MainData mainData) : MainResultOptimizer
{
    public override void Optimize(MainResult mainResult)
    {
        if (mainData?.AlternativePoints == null) return;
        
        foreach (var currentAlternativePoints in mainData.AlternativePoints!.AllCorteges)
        {
            // У групі лишається рівно одна точка; останню не видаляємо ніколи, незалежно від штрафів.
            while (currentAlternativePoints.Count(id => GetContainingPath(mainResult, id).car != null) > 1)
            {
                var minEstimation = double.MaxValue;
                var bestPointId = -1;

                foreach (var alternativePointId in currentAlternativePoints)
                {
                    var (car, inPathIndex) = GetContainingPath(mainResult, alternativePointId);

                    if (car == null || inPathIndex == null) continue;

                    var taken = mainResult.Results[car].Path.TakeAt(inPathIndex!.Value);

                    mainResult.ReEstimateCost(mainResultEstimator);

                    if (mainResult.Estimation < minEstimation)
                    {
                        minEstimation = mainResult.Estimation;
                        bestPointId = taken.Id;
                    }

                    mainResult.Results[car].Path.Insert(inPathIndex!.Value, taken);
                    mainResult.ReEstimateCost(mainResultEstimator);
                }

                var (bestCar, bestInPathIndex) = GetContainingPath(mainResult, bestPointId);
                mainResult.Results[bestCar!].Path.RemoveAt(bestInPathIndex!.Value);
                mainResult.ReEstimateCost(mainResultEstimator);
            }

            TrySwapToBetterAlternative(mainResult, currentAlternativePoints);
        } 
    }

    // Замінює обрану точку групи на іншу альтернативу на тому ж місці, якщо вистачає місткості й оцінка зменшується.
    private void TrySwapToBetterAlternative(MainResult mainResult, HashSet<int> group)
    {
        var chosenId = group.FirstOrDefault(id => GetContainingPath(mainResult, id).car != null);
        var (car, index) = GetContainingPath(mainResult, chosenId);

        if (car == null || index == null) return;

        var carResult = mainResult.Results[car];
        var best = mainResultEstimator.Estimate(mainResult);

        foreach (var otherId in group.Where(id => id != chosenId).ToList())
        {
            var load = mainData.GetLoad(carResult) - mainData.PointsByIds[chosenId].Demand + mainData.PointsByIds[otherId].Demand;

            if (load > car.Capacity) continue;

            var original = carResult.Path[index.Value];
            carResult.Path[index.Value] = new PointVisitResult(otherId);
            var estimation = mainResultEstimator.Estimate(mainResult);

            if (estimation < best)
            {
                best = estimation;
                chosenId = otherId;
                continue;
            }

            carResult.Path[index.Value] = original;
            mainResultEstimator.Estimate(mainResult);
        }
    }
    
    private (Car? car, int? inPathIndex) GetContainingPath(MainResult mainResult, int pointId)
    {
        var car = mainResult.Results
            .FirstOrDefault(pair => pair.Value.Path.Contains(pointId)).Key;
        
        return car == null 
            ? (null, null) 
            : (car, mainResult.Results[car].Path.IndexOf(pointId));
    }
}