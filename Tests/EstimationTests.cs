using CVRPTW;
using CVRPTW.Computing.Optimizers;
using Xunit;
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
