using System;

namespace VisualizationApplication.Avalonia.Map;

/// <summary>Веб-меркатор у метрах: форма карти збігається з картами OSM, а одиниці осей однакові.</summary>
public static class MapProjection
{
    private const double EarthRadius = 6378137;

    public static (double X, double Y) ToMap(double latitude, double longitude) =>
        (EarthRadius * longitude * Math.PI / 180,
            EarthRadius * Math.Log(Math.Tan(Math.PI / 4 + latitude * Math.PI / 360)));

    public static (double Latitude, double Longitude) ToLatLon(double x, double y) =>
        ((2 * Math.Atan(Math.Exp(y / EarthRadius)) - Math.PI / 2) * 180 / Math.PI,
            x / EarthRadius * 180 / Math.PI);
}
