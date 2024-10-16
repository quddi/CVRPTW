﻿namespace CVRPTW;

public class Point
{
    public int Comp { get; set; }
    
    public int Id { get; set; }
    
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }
    
    public double Demand { get; set; }
    
    public List<TimeWindow> TimeWindows { get; set; }
    
    public int ServiceTime { get; set; }
    
    public int LatePenalty { get; set; }

    public int WaitPenalty { get; set; }
}