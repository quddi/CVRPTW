using CVRPTW.Computing.Estimators;

namespace CVRPTW.Computing.Optimizers;

/// <summary>
/// Вставляє необслужених клієнтів у найдешевшу позицію, де вистачає місткості. Для необслуженої групи альтернатив
/// пробує кожного її члена. Якщо місця немає ніде, звільняє його заміною обраної точки іншої групи альтернатив
/// на легшу. Зміна приймається, лише коли загальна оцінка (разом зі штрафом за необслужених) зменшується
/// і не з'являються нові недосяжні переходи (їхній штраф дорівнює штрафу за необслуженого й інакше «обмінявся» б на нього).
/// </summary>
public class InsertUnservedMainResultOptimizer(IMainResultEstimator mainResultEstimator, MainData mainData) : MainResultOptimizer
{
    private readonly record struct Insertion(double Estimation, Car? Car, int Index, int PointId);

    private readonly record struct Replacement(Car Car, int Index, int NewPointId);

    public override void Optimize(MainResult mainResult)
    {
        mainResultEstimator.Estimate(mainResult);

        foreach (var candidates in GetUnservedUnits(mainResult).OrderByDescending(units => units.Min(id => mainData.PointsByIds[id].Demand)))
            TryServe(mainResult, candidates);
    }

    // Кожен елемент — клієнт або група альтернатив (будь-який із членів закриває вимогу), що не обслуговується.
    private List<int[]> GetUnservedUnits(MainResult mainResult)
    {
        var served = mainResult.Results.Values
            .SelectMany(result => result.Path.Select(visit => visit.Id))
            .Where(id => !id.IsDepoId())
            .ToHashSet();

        var units = mainData.PointsByIds.Keys
            .Where(id => !served.Contains(id) && mainData.AlternativePoints?.GetAlternativePoints(id) == null)
            .Select(id => new[] { id })
            .ToList();

        if (mainData.AlternativePoints != null)
            units.AddRange(mainData.AlternativePoints.AllCorteges.Where(group => !group.Any(served.Contains)).Select(group => group.ToArray()));

        return units;
    }

    private void TryServe(MainResult mainResult, int[] candidates)
    {
        var baseline = mainResult.Estimation;
        var unreachable = mainResult.UnreachableEdgesCount;
        var best = FindBestInsertion(mainResult, candidates, unreachable);
        Replacement? bestReplacement = null;

        // Звільняємо місце: замінюємо обрану точку групи альтернатив на легшу й дивимось, чи вдається вставити клієнта.
        foreach (var (car, index, lighterId) in GetLighterReplacements(mainResult))
        {
            var path = mainResult.Results[car].Path;
            var original = path[index];

            path[index] = new PointVisitResult(lighterId);
            var insertion = FindBestInsertion(mainResult, candidates, unreachable);
            path[index] = original;

            if (insertion.Car == null || insertion.Estimation >= (best.Car == null ? baseline : best.Estimation)) continue;

            best = insertion;
            bestReplacement = new Replacement(car, index, lighterId);
        }

        if (best.Car == null || best.Estimation >= baseline)
        {
            mainResultEstimator.Estimate(mainResult);
            return;
        }

        if (bestReplacement is { } replacement)
            mainResult.Results[replacement.Car].Path[replacement.Index] = new PointVisitResult(replacement.NewPointId);

        mainResult.Results[best.Car].Path.Insert(best.Index, new PointVisitResult(best.PointId));
        mainResultEstimator.Estimate(mainResult);
    }

    // Найкраща вставка одного з кандидатів у поточний стан; Car == null, якщо вставити ніде.
    private Insertion FindBestInsertion(MainResult mainResult, int[] candidates, int maxUnreachable)
    {
        var best = new Insertion(double.MaxValue, null, 0, 0);

        foreach (var (car, carResult) in mainResult.Results)
        {
            var load = mainData.GetLoad(carResult);

            foreach (var pointId in candidates)
            {
                if (load + mainData.PointsByIds[pointId].Demand > car.Capacity) continue;

                for (var index = 1; index < carResult.Path.Count; index++)
                {
                    carResult.Path.Insert(index, new PointVisitResult(pointId));
                    var estimation = mainResultEstimator.Estimate(mainResult);
                    var unreachableCount = mainResult.UnreachableEdgesCount;
                    carResult.Path.RemoveAt(index);

                    if (unreachableCount <= maxUnreachable && estimation < best.Estimation) best = new Insertion(estimation, car, index, pointId);
                }
            }
        }

        mainResultEstimator.Estimate(mainResult);

        return best;
    }

    // Обрані точки груп альтернатив, для яких є легша необрана альтернатива: (авто, позиція в шляху, id легшої).
    private IEnumerable<(Car Car, int Index, int LighterId)> GetLighterReplacements(MainResult mainResult)
    {
        if (mainData.AlternativePoints == null) yield break;

        foreach (var (car, carResult) in mainResult.Results)
        for (var index = 1; index < carResult.Path.Count - 1; index++)
        {
            var current = carResult.Path[index].Id;
            var siblings = mainData.AlternativePoints.GetAlternativePoints(current);

            if (siblings == null) continue;

            foreach (var sibling in siblings.Where(id => mainData.PointsByIds[id].Demand < mainData.PointsByIds[current].Demand
                                                         && !mainResult.Results.Values.Any(result => result.Path.Contains(id))))
                yield return (car, index, sibling);
        }
    }
}
