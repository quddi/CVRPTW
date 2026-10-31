using System.Text.Json;

namespace CVRPTW;

public static class ScenarioSerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static void Save(Scenario scenario, Stream stream)
    {
        JsonSerializer.Serialize(stream, scenario, Options);
    }

    public static Scenario Load(Stream stream)
    {
        var scenario = JsonSerializer.Deserialize<Scenario>(stream, Options)
                       ?? throw new InvalidDataException("Файл сценарію порожній.");

        if (scenario.Version > Scenario.CurrentVersion)
            throw new InvalidDataException($"Версія файлу {scenario.Version} новіша за підтримувану {Scenario.CurrentVersion}.");

        return scenario;
    }
}
