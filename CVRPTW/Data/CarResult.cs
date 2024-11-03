namespace CVRPTW;

public class CarResult(Car car)
{
    public Car Car { get; set; } = car;

    public double PathCost { get; set; }

    public CarPath Path { get; set; } = new();

    public override string ToString()
    {
        return $"CarResult: {nameof(Car)}: {Car.Id}, {nameof(PathCost)}: {PathCost}";
    }
}