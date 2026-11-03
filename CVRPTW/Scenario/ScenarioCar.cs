namespace CVRPTW;

public class ScenarioCar
{
    public int Id { get; set; }

    public int Capacity { get; set; } = Constants.MissingCarCapacity;

    // Хвилини від початку доби, як у файлах вводу.
    public double TimeWindowStart { get; set; }

    public double TimeWindowEnd { get; set; } = Constants.MinutesInDay;

    // Штраф за одну хвилину повернення в депо після кінця зміни.
    public int DriverOvertimePenalty { get; set; } = Constants.DefaultDriverOvertimePenalty;
}
