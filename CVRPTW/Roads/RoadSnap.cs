namespace CVRPTW;

/// <summary>Найближча до точки позиція на дорозі.</summary>
public readonly record struct RoadSnap(int Edge, double Latitude, double Longitude, double DistanceMeters);
