namespace CVRPTW;

/// <summary>Шлях по дорогах між двома точками: довжина в метрах і полілінія (від прив'язаної позиції першої точки до другої).</summary>
public record RoadPath(double DistanceMeters, double[] Latitudes, double[] Longitudes);
