namespace CVRPTW.Data;

/*
 * id\capacity (* demand)\tw open\tw close\overload penalty\
 * (start point id; *; end point id)\-\driver overtime penalty\
 * can start late (-?)\max capacity (Ukraine only)\max capacity
 * penalty\тариф\max trip count (always 1)\wait between trips
 * (always 0)\-\-\-\-\-\-\-\-\-
 */
public class Car : IData
{
    public int Id { get; set; }
    
    public List<int> Capacities { get; set; }
    
    public TimeWindow TimeWindow { get; set; }
    
    public int OverloadPenalty { get; set; }
    
    public IdsList PointsIds { get; set; }
    
    public int DriverOvertimePenalty { get; set; }
    
    public bool CanStartLate { get; set; }
    
    public int MaxCapacity { get; set; }
    
    public int MaxCapacityPenalty { get; set; }
    
    public int Tariff { get; set; }
    
    public int MaxTripsCount { get; set; }
    
    public int WaitBetweenTrips { get; set; }
}