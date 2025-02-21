namespace CVRPTW;

public class Point
{
    public int Comp { get; set; }
    
    public int Id { get; set; }
    
    public int Index { get; set; }
    
    public Coordinates Coordinates { get; set; }
    
    public double Demand { get; set; }
    
    public List<TimeWindow> TimeWindows { get; set; }
    
    public int ServiceTime { get; set; }
    
    //TODO : per minute, transform to seconds
    public int LatePenalty { get; set; }
    
    //TODO : per minute, transform to seconds
    public int WaitPenalty { get; set; }

    public override string ToString()
    {
        return $"{nameof(Point)}: {Id}, {Index}, {Coordinates}";
    }
}