namespace CVRPTW;

public class MainData
{
    public int DemandSize { get; set; }
    
    public int TimeWindows { get; set; }
    
    public int MaxOverload { get; set; }
    
    public int MaxCompsOverload { get; set; }
    
    public Distances Distances { get; set; }
    
    public Tariffs Tariffs { get; set; }
    
    public Times Times { get; set; }
    
    public Point DepoPoint { get; set; }
    
    public Dictionary<int, Point> Points { get; set; } = new();
    
    public AlternativePoints AlternativePoints { get; set; }

    public List<Car> Cars { get; set; } = new();
}