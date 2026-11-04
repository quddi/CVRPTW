using CVRPTW;
using CVRPTW.Computing.Optimizers;
using Xunit;
using CVRPTW.Computing;
using CVRPTW.Computing.Estimators;
using CVRPTW.Computing.Estimators.Time;

namespace Tests;

// Світ із трьох точок: склад (-1), клієнти 1 і 2. Кожен перехід — 1000 м і 100 с; усі штрафи тут — за секунду.
public class EstimationTests
{
    private const int Depot = -1;

    [Fact]
    public void TimeEstimator_AddsWaitingAndServiceTime()
    {
        var (data, car) = CreateWorld();
        data.PointsByIds[1].TimeWindows = [new TimeWindow { Start = 500, End = 1000 }];
        data.PointsByIds[1].ServiceTime = 60;
        data.PointsByIds[2].ServiceTime = 30;
        var path = new CarPath(new(Depot), new(1), new(2), new(Depot));

        var end = new SimpleTimeEstimator(data).Estimate(path, car);

        // Прибуття в 1: 100, очікування до 500, обслуговування 60 → виїзд 560; прибуття в 2: 660, обслуговування 30 → 690; склад: 790.
        Assert.Equal([0, 100, 660, 790], path.Select(visit => visit.VisitTime));
        Assert.Equal(790, end);
    }

    [Fact]
    public void Estimator_ChargesWaitAndLatePenaltiesPerSecond()
    {
        var (data, car) = CreateWorld();
        data.PointsByIds[1].TimeWindows = [new TimeWindow { Start = 500, End = 1000 }];
        data.PointsByIds[1].WaitPenalty = 0.1;
        data.PointsByIds[1].ServiceTime = 60;
        data.PointsByIds[2].TimeWindows = [new TimeWindow { Start = 0, End = 600 }];
        data.PointsByIds[2].LatePenalty = 0.5;
        data.PointsByIds[2].ServiceTime = 30;

        var estimation = CreateEstimator(data).Estimate(CreateResult(car, 1, 2));

        // Відстань 3000 + очікування в 1 (500 − 100) × 0.1 + запізнення в 2 (660 − 600) × 0.5.
        Assert.Equal(3000 + 40 + 30, estimation, 6);
    }

    [Fact]
    public void Estimator_ChargesOvertimeAfterShiftEnd()
    {
        var (data, car) = CreateWorld();
        car.TimeWindow.End = 250;
        car.DriverOvertimePenalty = 2;

        var estimation = CreateEstimator(data).Estimate(CreateResult(car, 1, 2));

        // Повернення на склад о 300, зміна до 250: 50 с × 2.
        Assert.Equal(3000 + 100, estimation, 6);
    }

    [Fact]
    public void Estimator_PenalizesUnservedClient()
    {
        var (data, car) = CreateWorld();
        var estimator = CreateEstimator(data);

        var served = estimator.Estimate(CreateResult(car, 1, 2));
        var partial = estimator.Estimate(CreateResult(car, 1));

        // Штраф за необслуженого: макс. відстань 1000 × (3 точки + 1).
        Assert.Equal(2000 + 4000, partial, 6);
        Assert.True(partial > served);
    }

    [Fact]
    public void Estimator_CountsAlternativeGroupAsOneClient()
    {
        var (data, car) = CreateWorld();
        data.AlternativePoints = new AlternativePoints();
        data.AlternativePoints.AddAlternativePoints(1, 2);
        var estimator = CreateEstimator(data);

        var oneOfGroup = estimator.Estimate(CreateResult(car, 1));
        var noneOfGroup = estimator.Estimate(CreateResult(car));

        Assert.Equal(2000, oneOfGroup, 6);
        Assert.Equal(4000, noneOfGroup, 6);
    }

    [Fact]
    public void GetLoad_SumsDemandsOfCurrentRoute()
    {
        var (data, car) = CreateWorld();
        data.PointsByIds[1].Demand = 3;
        data.PointsByIds[2].Demand = 4;

        Assert.Equal(7, data.GetLoad(CreateResult(car, 1, 2).Results[car]));
        Assert.Equal(3, data.GetLoad(CreateResult(car, 1).Results[car]));
    }

    [Fact]
    public void AlternativesOptimizer_LeavesExactlyOnePointPerGroup()
    {
        var (data, car) = CreateWorld(3);
        data.AlternativePoints = new AlternativePoints();
        data.AlternativePoints.AddAlternativePoints(1, 2, 3);
        var estimator = CreateEstimator(data);
        var result = CreateResult(car, 1, 2, 3);
        estimator.Estimate(result);

        new AlternativePointsMainResultOptimizer(estimator, data).Optimize(result);

        Assert.Single(result.Results[car].Path, visit => visit.Id > 0);
    }

    [Fact]
    public void AlternativesOptimizer_KeepsLastPointDespiteHugeTimePenalty()
    {
        var (data, car) = CreateWorld();
        data.AlternativePoints = new AlternativePoints();
        data.AlternativePoints.AddAlternativePoints(1, 2);
        data.PointsByIds[1].TimeWindows = [new TimeWindow { Start = 0, End = 0 }];
        data.PointsByIds[1].LatePenalty = 1_000_000;
        data.PointsByIds[2].TimeWindows = [new TimeWindow { Start = 0, End = 0 }];
        data.PointsByIds[2].LatePenalty = 1_000_000;
        var estimator = CreateEstimator(data);
        var result = CreateResult(car, 1);
        estimator.Estimate(result);

        new AlternativePointsMainResultOptimizer(estimator, data).Optimize(result);

        Assert.Single(result.Results[car].Path, visit => visit.Id > 0);
    }

    [Fact]
    public void CarPath_RemoveOfMissingPointKeepsDepots()
    {
        var path = new CarPath(new(Depot), new(1), new(Depot));

        Assert.Equal(-1, path.IndexOf(2));
        Assert.Equal(-1, path.IndexOf(new PointVisitResult(2)));
        Assert.False(path.Remove(new PointVisitResult(2)));
        Assert.Equal(3, path.Count);
        Assert.Equal(Depot, path[0].Id);
    }

    [Fact]
    public void Scenario_RejectsNegativeServiceTimeAndPenalties()
    {
        var scenario = new Scenario
        {
            Clients = [new ScenarioClient { Id = 1, ServiceTime = -1, LatePenalty = -1 }],
            Cars = [new ScenarioCar { Id = 1, DriverOvertimePenalty = -1 }]
        };

        var errors = scenario.Validate();

        Assert.Equal(3, errors.Count);
    }

    [Fact]
    public void StartComputer_ServesOnlyOnePointPerAlternativeGroup()
    {
        var (data, _) = CreateWorld(3);
        data.AlternativePoints = new AlternativePoints();
        data.AlternativePoints.AddAlternativePoints(1, 2, 3);

        var result = new DistanceMainComputer(data, CreateEstimator(data)).Compute();

        Assert.Single(result.Results.Values.Single().Path, visit => visit.Id > 0);
    }

    [Fact]
    public void CarEstimation_IncludesTimePenalties()
    {
        var (data, car) = CreateWorld();
        data.PointsByIds[2].TimeWindows = [new TimeWindow { Start = 0, End = 100 }];
        data.PointsByIds[2].LatePenalty = 0.5;
        var result = CreateResult(car, 1, 2);

        var total = CreateEstimator(data).Estimate(result);

        // Відстань 3000 + запізнення в 2 (200 − 100) × 0.5.
        Assert.Equal(3050, result.Results[car].Estimation, 6);
        Assert.Equal(total, result.Results[car].Estimation, 6);
    }

    [Fact]
    public void UnservedPenalty_IsPositiveEvenWhenAllDistancesAreZero()
    {
        var (data, car) = CreateWorld();
        for (var i = 0; i < 3; i++)
        for (var j = 0; j < 3; j++)
            data.Distances!.AddDistance(Constants.DefaultMatrixId, i, j, 0);

        Assert.True(CreateEstimator(data).Estimate(CreateResult(car, 1)) > 0);
    }

    [Fact]
    public void Scenario_RejectsNonFiniteNumbers()
    {
        var scenario = new Scenario
        {
            AverageSpeedKmh = double.NaN,
            Clients = [new ScenarioClient { Id = 1, TimeWindowStart = double.PositiveInfinity }],
            Cars = [new ScenarioCar { Id = 1, TimeWindowEnd = double.NaN }]
        };

        Assert.Contains(scenario.Validate(), error => error.Contains("скінченн"));
        Assert.True(scenario.Validate().Count >= 3);
    }

    [Fact]
    public void AlternativesOptimizer_SwapsToCheaperAlternative()
    {
        var (data, car) = CreateWorld();
        data.AlternativePoints = new AlternativePoints();
        data.AlternativePoints.AddAlternativePoints(1, 2);
        SetDistanceBothWays(data, 0, 1, 100);
        var estimator = CreateEstimator(data);
        var result = CreateResult(car, 2);
        estimator.Estimate(result);

        new AlternativePointsMainResultOptimizer(estimator, data).Optimize(result);

        Assert.Equal([Depot, 1, Depot], result.Results[car].Path.Select(visit => visit.Id));
    }

    [Fact]
    public void InsertUnserved_InsertsClientWhenCapacityAllows()
    {
        var (data, car) = CreateWorld();
        var estimator = CreateEstimator(data);
        var result = CreateResult(car, 1);
        estimator.Estimate(result);

        new InsertUnservedMainResultOptimizer(estimator, data).Optimize(result);

        Assert.Equal(0, data.CountUnserved(result));
    }

    [Fact]
    public void InsertUnserved_FreesCapacityBySwappingToLighterAlternative()
    {
        // Близька альтернатива 1 з попитом 8 займає авто (місткість 10), і обов'язковий клієнт 3 (попит 4) не вміщається;
        // легша альтернатива 2 (попит 2) дозволяє обслужити обох.
        var (data, car) = CreateWorld(3);
        data.AlternativePoints = new AlternativePoints();
        data.AlternativePoints.AddAlternativePoints(1, 2);
        data.PointsByIds[1].Demand = 8;
        data.PointsByIds[2].Demand = 2;
        data.PointsByIds[3].Demand = 4;
        SetDistanceBothWays(data, 0, 1, 100);
        var estimator = CreateEstimator(data);
        var result = new DistanceMainComputer(data, estimator).Compute();

        Assert.Equal([Depot, 1, Depot], result.Results[car].Path.Select(visit => visit.Id));
        Assert.Equal(1, data.CountUnserved(result));

        new InsertUnservedMainResultOptimizer(estimator, data).Optimize(result);

        var served = result.Results[car].Path.Select(visit => visit.Id).Where(id => id > 0).Order().ToArray();
        Assert.Equal([2, 3], served);
        Assert.Equal(0, data.CountUnserved(result));
    }

    [Fact]
    public void InsertUnserved_DoesNotTradeUnservedForUnreachableEdge()
    {
        // 1 → склад близько, але склад → 1 і 1 ↔ 2 недосяжні: вставка 1 після 2 зняла б штраф за необслуженого
        // і додала б штраф за недосяжний перехід 2 → 1 тієї самої величини, а дорога 2 → склад замінилась би на 1 → склад.
        var (data, car) = CreateWorld();
        data.Distances = new Distances(3);
        SetDistanceBothWays(data, 0, 2, 1000);
        data.Distances.AddDistance(Constants.DefaultMatrixId, 1, 0, 10);
        var estimator = CreateEstimator(data);
        var result = CreateResult(car, 2);
        estimator.Estimate(result);

        new InsertUnservedMainResultOptimizer(estimator, data).Optimize(result);

        Assert.Equal([Depot, 2, Depot], result.Results[car].Path.Select(visit => visit.Id));
        Assert.Equal(0, result.UnreachableEdgesCount);
    }

    private static void SetDistanceBothWays(MainData data, int first, int second, double distance)
    {
        data.Distances!.AddDistance(Constants.DefaultMatrixId, first, second, distance);
        data.Distances.AddDistance(Constants.DefaultMatrixId, second, first, distance);
    }

    private static ComplexMainResultEstimator CreateEstimator(MainData data) =>
        new(data, new ByDistanceMainResultEstimator(data), new SimpleTimeEstimator(data));

    private static MainResult CreateResult(Car car, params int[] clientIds)
    {
        var visits = new[] { new PointVisitResult(Depot) }
            .Concat(clientIds.Select(id => new PointVisitResult(id)))
            .Append(new PointVisitResult(Depot))
            .ToArray();

        return new MainResult { Results = { [car] = new CarResult(car) { Path = new CarPath(visits) } } };
    }

    private static (MainData data, Car car) CreateWorld(int clientsCount = 2)
    {
        var pointsCount = clientsCount + 1;
        var data = new MainData { Distances = new Distances(pointsCount), Times = new Times() };

        AddPoint(data, new Point { Id = Depot, TimeWindows = [] });
        for (var id = 1; id <= clientsCount; id++) AddPoint(data, new Point { Id = id, TimeWindows = [] });

        for (var i = 0; i < pointsCount; i++)
        for (var j = 0; j < pointsCount; j++)
        {
            // Діагональ є в реальній матриці (з нулями): порожній маршрут склад → склад звертається до неї.
            data.Distances.AddDistance(Constants.DefaultMatrixId, i, j, i == j ? 0 : 1000);
            data.Times.AddTime(Constants.DefaultMatrixId, i, j, i == j ? 0 : 100);
        }

        var car = new Car
        {
            Id = 1,
            Capacities = [10],
            TimeWindow = new TimeWindow { Start = 0, End = 100000 },
            PointsDatas = [new PointVisitData(Depot), new PointVisitData(Depot)]
        };
        data.Cars.Add(car);

        return (data, car);
    }

    private static void AddPoint(MainData data, Point point)
    {
        point.Index = data.Points.Count;
        data.Points.Add(point);

        if (point.IsDepo()) data.DepoPointsByIds.Add(point.Id, point);
        else data.PointsByIds.Add(point.Id, point);
    }
}
