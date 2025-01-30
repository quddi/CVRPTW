namespace CVRPTW;

public class CarResult(Car car)
{
    public Car Car { get; set; } = car;

    public double PathCost { get; set; }

    public double RemainedFreeSpace { get; set; }
    
    public CarPath Path { get; set; } = new();
    
    public override string ToString()
    {
        return $"CarResult: {nameof(Car)}: {Car.Id}, Capacity: {Car.Capacities[0]}, {nameof(PathCost)}: {PathCost.ToFormattedString()}, {nameof(Path.Count)}: {Path.Count}";
    }
    
    public CarResult Clone()
    {
        return new CarResult(Car)
        {
            PathCost = this.PathCost,
            Path = this.Path.Clone(),
            Car = this.Car
        };
    }
}