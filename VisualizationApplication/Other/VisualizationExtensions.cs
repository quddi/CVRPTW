namespace VisualizationApplication.Other;

public static class VisualizationExtensions
{
    public static ScottPlot.Coordinates ToScottCoordinates(this CVRPTW.Coordinates coordinates)
    {
        return new ScottPlot.Coordinates(coordinates.Latitude, coordinates.Longitude);
    }
}