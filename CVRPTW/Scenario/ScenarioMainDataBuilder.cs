namespace CVRPTW;

/// <summary>
/// Перетворює сценарій на дані для оптимізаторів: рахує дорожню матрицю відстаней (метри) і час переїзду (секунди,
/// відстань / середня швидкість). Одиниці часу й штрафів такі самі, як у парсері файлів вводу.
/// </summary>
public static class ScenarioMainDataBuilder
{
    // Ідентифікатор складу: від'ємний, як вимагає Id.IsDepoId().
    public const int DepotId = -1;

    private const double MetersPerKilometer = 1000;
    private const double SecondsPerHour = 3600;

    public static MainData Build(Scenario scenario, RoadMatrixCalculator calculator)
    {
        var errors = scenario.Validate();
        if (scenario.Depot == null) errors.Add("Не задано склад.");
        if (scenario.Clients.Count == 0) errors.Add("Немає жодного клієнта.");
        if (scenario.Cars.Count == 0) errors.Add("Немає жодного автомобіля.");

        errors.AddRange(scenario.Clients.Where(client => client.Id < 0).Select(client => $"Клієнт {client.Id}: Id не може бути від'ємним."));

        foreach (var duplicate in scenario.AlternativeGroups.SelectMany(group => group.Distinct()).GroupBy(id => id).Where(group => group.Count() > 1))
            errors.Add($"Клієнт {duplicate.Key} входить у кілька груп альтернатив.");

        if (errors.Count > 0) throw new ArgumentException(string.Join("\n", errors));

        var mainData = new MainData { DemandSize = 1, TimeWindows = 1 };

        AddPoint(mainData, new Point
        {
            Id = DepotId,
            Coordinates = new Coordinates { Latitude = scenario.Depot!.Latitude, Longitude = scenario.Depot.Longitude },
            Demand = 0,
            TimeWindows = []
        });

        foreach (var client in scenario.Clients)
        {
            AddPoint(mainData, new Point
            {
                Id = client.Id,
                Coordinates = new Coordinates { Latitude = client.Latitude, Longitude = client.Longitude },
                Demand = client.Demand,
                TimeWindows = [new TimeWindow { Start = client.TimeWindowStart.FromMinutesToSeconds(), End = client.TimeWindowEnd.FromMinutesToSeconds() }],
                ServiceTime = client.ServiceTime,
                LatePenalty = client.LatePenalty.FromMinutesToSeconds(),
                WaitPenalty = client.WaitPenalty.FromMinutesToSeconds()
            });
        }

        foreach (var car in scenario.Cars)
        {
            mainData.Cars.Add(new Car
            {
                Id = car.Id,
                Capacities = [car.Capacity],
                TimeWindow = new TimeWindow { Start = car.TimeWindowStart.FromMinutesToSeconds(), End = car.TimeWindowEnd.FromMinutesToSeconds() },
                PointsDatas = [new PointVisitData(DepotId), new PointVisitData(DepotId)],
                MaxTripsCount = 1
            });
        }

        foreach (var group in scenario.AlternativeGroups.Where(group => group.Distinct().Count() > 1))
        {
            mainData.AlternativePoints ??= new AlternativePoints();
            mainData.AlternativePoints.AddAlternativePoints(group.Distinct().ToArray());
        }

        var roadMatrix = calculator.Calculate(mainData.Points.Select(point => new RoadMatrixPoint(point.Id, point.Coordinates.Latitude, point.Coordinates.Longitude)).ToList());

        mainData.Distances = RoadDistanceMatrixMapper.ToDistances(roadMatrix, mainData);
        mainData.Times = BuildTimes(roadMatrix, mainData, scenario.AverageSpeedKmh);

        return mainData;
    }

    private static void AddPoint(MainData mainData, Point point)
    {
        point.Index = mainData.Points.Count;
        mainData.Points.Add(point);

        if (point.IsDepo()) mainData.DepoPointsByIds.Add(point.Id, point);
        else mainData.PointsByIds.Add(point.Id, point);
    }

    // Для недосяжної пари час 0: такий перехід усе одно рахується штрафом недосяжності й робить розв'язок недопустимим.
    private static Times BuildTimes(RoadDistanceMatrix roadMatrix, MainData mainData, double averageSpeedKmh)
    {
        var times = new Times();
        var metersPerSecond = averageSpeedKmh * MetersPerKilometer / SecondsPerHour;

        for (var i = 0; i < roadMatrix.PointIds.Length; i++)
        for (var j = 0; j < roadMatrix.PointIds.Length; j++)
        {
            var seconds = roadMatrix.Values[i, j] is { } distance ? (int)Math.Round(distance / metersPerSecond) : 0;

            times.AddTime(Constants.DefaultMatrixId, mainData.GetPoint(roadMatrix.PointIds[i]).Index, mainData.GetPoint(roadMatrix.PointIds[j]).Index, seconds);
        }

        return times;
    }
}
