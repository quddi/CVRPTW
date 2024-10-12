namespace CVRPTW;

public class MainData
{
    public int DemandSize { get; set; }
    
    public int TimeWindows { get; set; }
    
    public List<Point> Points { get; set; }
    
    public List<Car> Cars { get; set; }
    
    public MainData(StreamReader streamReader)
    {
        
    }
}