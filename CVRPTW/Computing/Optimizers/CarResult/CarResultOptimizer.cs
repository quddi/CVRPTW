namespace CVRPTW.Computing.Optimizers;

public abstract class CarResultOptimizer : IOptimizer
{
    public string Name { get; set; } = string.Empty;
    
    public abstract void Optimize(CarResult carResult);
}