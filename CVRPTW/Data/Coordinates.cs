namespace CVRPTW;

public struct Coordinates
{
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }

    public override string ToString()
    {
        return $"[{Latitude}, {Longitude}]";
    }
}