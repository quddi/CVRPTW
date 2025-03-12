namespace CVRPTW;

public class Point
{
    public int Comp { get; set; }
    
    public int Id { get; set; }
    
    public int Index { get; set; }
    
    public Coordinates Coordinates { get; set; }
    
    public double Demand { get; set; }
    
    public List<TimeWindow>? TimeWindows { get; set; }

    public TimeWindow? TimeWindow => TimeWindows?[0];
    
    public int ServiceTime { get; set; }
    
    public int LatePenalty { get; set; }
    
    public int WaitPenalty { get; set; }

    public override string ToString()
    {
        return $"{nameof(Point)}: {Id}, {Index}, {Coordinates}";
    }
}