namespace CVRPTW;

public class ScenarioClient
{
    public int Id { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double Demand { get; set; } = Constants.MissingPointDemand;

    // Хвилини від початку доби, як у файлах вводу.
    public double TimeWindowStart { get; set; }

    public double TimeWindowEnd { get; set; } = Constants.MinutesInDay;

    public int ServiceTime { get; set; }

    // Штрафи за одну хвилину, як у файлах вводу.
    public int LatePenalty { get; set; }

    public int WaitPenalty { get; set; }
}
