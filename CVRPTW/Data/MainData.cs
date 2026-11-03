namespace CVRPTW;

public class MainData
{
    public int DemandSize { get; set; }
    
    public int TimeWindows { get; set; }
    
    public long MaxOverload { get; set; }
    
    public long MaxCompsOverload { get; set; }
    
    public Distances? Distances { get; set; }
    
    public Tariffs? Tariffs { get; set; }
    
    public Times? Times { get; set; }
    
    public List<Point> Points { get; set; } = new();

    public Dictionary<int, Point> DepoPointsByIds { get; set; } = new();
    public Dictionary<int, Point> PointsByIds { get; set; } = new();
    
    public AlternativePoints? AlternativePoints { get; set; }

    public List<Car> Cars { get; set; } = new();

    public Point GetPoint(int id)
    {
        return id.IsDepoId() ? DepoPointsByIds[id] : PointsByIds[id];
    }

    // Група альтернатив рахується за одного клієнта: необслугованою вона вважається, лише якщо в маршрутах немає жодної її точки.
    public int CountUnserved(MainResult mainResult)
    {
        var served = mainResult.Results.Values
            .SelectMany(result => result.Path.Select(visit => visit.Id))
            .Where(id => !id.IsDepoId())
            .ToHashSet();

        var unserved = PointsByIds.Keys.Count(id => !served.Contains(id) && AlternativePoints?.GetAlternativePoints(id) == null);

        if (AlternativePoints != null)
            unserved += AlternativePoints.AllCorteges.Count(group => !group.Any(served.Contains));

        return unserved;
    }

    // Поточне завантаження авто за його маршрутом (а не за станом початкової побудови).
    public double GetLoad(CarResult carResult)
    {
        return carResult.Path
            .Select(visit => visit.Id)
            .Where(id => !id.IsDepoId())
            .Sum(id => PointsByIds[id].Demand);
    }

    // Сумарне перевищення місткості по всіх авто; 0 — усі авто в межах місткості.
    public double GetExcessLoad(MainResult mainResult)
    {
        var excess = 0d;

        foreach (var (car, carResult) in mainResult.Results)
        {
            var load = 0d;

            for (var i = 0; i < carResult.Path.Count; i++)
            {
                var id = carResult.Path[i].Id;

                if (!id.IsDepoId()) load += PointsByIds[id].Demand;
            }

            excess += Math.Max(0, load - car.Capacity);
        }

        return excess;
    }
}