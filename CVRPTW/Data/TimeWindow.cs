namespace CVRPTW;

public class TimeWindow
{
    public int Start { get; set; }
    
    public int End { get; set; }
    
    public bool Contains(double time) => Start <= time && time <= End;
}