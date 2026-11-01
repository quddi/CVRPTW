namespace CVRPTW;

/// <summary>
/// Найближча до точки позиція на дорозі. Fraction — частка довжини ребра (0..1) від його початку до цієї позиції.
/// </summary>
public readonly record struct RoadSnap(int Edge, double Latitude, double Longitude, double DistanceMeters, double Fraction);
