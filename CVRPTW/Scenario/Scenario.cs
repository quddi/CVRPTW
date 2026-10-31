namespace CVRPTW;

public class Scenario
{
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;

    public ScenarioDepot? Depot { get; set; }

    public double AverageSpeedKmh { get; set; } = Constants.DefaultAverageSpeedKmh;

    // Точка далі за цю відстань від дороги вважається особливою.
    public double SnapThresholdMeters { get; set; } = Constants.DefaultSnapThresholdMeters;

    public List<ScenarioClient> Clients { get; set; } = new();

    public List<ScenarioCar> Cars { get; set; } = new();

    // Кожна група містить Id клієнтів, що є альтернативами один одному.
    public List<List<int>> AlternativeGroups { get; set; } = new();

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (AverageSpeedKmh <= 0)
            errors.Add("Середня швидкість має бути більшою за 0.");

        if (SnapThresholdMeters < 0)
            errors.Add("Поріг відстані до дороги не може бути від'ємним.");

        foreach (var duplicate in Clients.GroupBy(client => client.Id).Where(group => group.Count() > 1))
            errors.Add($"Id клієнта {duplicate.Key} повторюється.");

        foreach (var duplicate in Cars.GroupBy(car => car.Id).Where(group => group.Count() > 1))
            errors.Add($"Id автомобіля {duplicate.Key} повторюється.");

        foreach (var client in Clients)
        {
            if (client.Demand < 0)
                errors.Add($"Клієнт {client.Id}: попит не може бути від'ємним.");

            if (client.TimeWindowEnd < client.TimeWindowStart)
                errors.Add($"Клієнт {client.Id}: кінець часового вікна раніше за початок.");
        }

        foreach (var car in Cars)
        {
            if (car.Capacity <= 0)
                errors.Add($"Автомобіль {car.Id}: місткість має бути більшою за 0.");

            if (car.TimeWindowEnd < car.TimeWindowStart)
                errors.Add($"Автомобіль {car.Id}: кінець часового вікна раніше за початок.");
        }

        var clientIds = Clients.Select(client => client.Id).ToHashSet();

        foreach (var group in AlternativeGroups)
        foreach (var id in group.Where(id => !clientIds.Contains(id)))
            errors.Add($"Група альтернатив посилається на неіснуючого клієнта {id}.");

        return errors;
    }
}
