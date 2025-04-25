namespace CVRPTW;

public class TimeWindow
{
    public double Start { get; set; }
    
    public double End { get; set; }
    
    public bool Contains(double time) => Start <= time && time <= End;
}