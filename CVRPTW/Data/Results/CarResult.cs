namespace CVRPTW;

public class CarResult(Car car)
{
    public Car Car { get; set; } = car;

    public double Estimation { get; set; }

    public double RemainedFreeSpace { get; set; }
    
    public CarPath Path { get; set; } = new();
    
    public override string ToString()
    {
        return $"{nameof(CarResult)}: {nameof(Car)}: {Car.Id}, Capacity: {Car.Capacity}, {nameof(Estimation)}: {Estimation.ToFormattedString()}, {nameof(Path.Count)}: {Path.Count}";
    }
    
    public CarResult Clone()
    {
        return new CarResult(Car)
        {
            Estimation = this.Estimation,
            Path = this.Path.Clone(),
            Car = this.Car
        };
    }
}