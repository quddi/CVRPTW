namespace CVRPTW;


public class Car
{
    public int Id { get; set; }
    
    public List<int> Capacities { get; set; }
    
    public TimeWindow TimeWindow { get; set; }
    
    public int OverloadPenalty { get; set; }
    
    public List<PointVisitInfo> PointsInfos { get; set; }
    
    public int DriverOvertimePenalty { get; set; }
    
    public int MaxCapacity { get; set; }
    
    public int MaxCapacityPenalty { get; set; }
    
    public int Tariff { get; set; }
    
    public int MaxTripsCount { get; set; }
    
    public int WaitBetweenTrips { get; set; }
}