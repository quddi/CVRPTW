namespace CVRPTW;

public class MainData
{
    public int DemandSize { get; set; }
    
    public int TimeWindows { get; set; }
    
    public int MaxOverload { get; set; }
    
    public int MaxCompsOverload { get; set; }
    
    public List<Point> Points { get; set; }
    
    public List<Car> Cars { get; set; }
}