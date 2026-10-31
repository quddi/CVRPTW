using OsmSharp.Tags;

namespace CVRPTW;

public enum RoadDirection
{
    None,
    Both,
    Forward,
    Backward
}

/// <summary>
/// Правила доступності доріг для автомобіля. Умовні обмеження (за часом, типом авто) ігноруються.
/// </summary>
public static class CarRoadProfile
{
    private static readonly HashSet<string> DrivableHighways =
    [
        "motorway", "motorway_link", "trunk", "trunk_link", "primary", "primary_link",
        "secondary", "secondary_link", "tertiary", "tertiary_link", "unclassified",
        "residential", "living_street", "service", "road"
    ];

    private static readonly string[] AccessKeys = ["motorcar", "motor_vehicle", "vehicle", "access"];

    private static readonly string[] ClosedAccessValues = ["no", "private"];

    private static readonly string[] CarVehicleClasses = ["motorcar", "motor_vehicle", "vehicle"];

    public static RoadDirection GetDirection(TagsCollectionBase tags)
    {
        if (!tags.TryGetValue("highway", out var highway) || !DrivableHighways.Contains(highway))
            return RoadDirection.None;

        if (tags.TryGetValue("area", out var area) && area == "yes")
            return RoadDirection.None;

        foreach (var key in AccessKeys)
        {
            if (!tags.TryGetValue(key, out var access)) continue;

            if (ClosedAccessValues.Contains(access)) return RoadDirection.None;

            break;
        }

        if (tags.TryGetValue("oneway", out var oneway))
        {
            switch (oneway)
            {
                case "yes" or "true" or "1": return RoadDirection.Forward;
                case "-1" or "reverse": return RoadDirection.Backward;
                case "no" or "false" or "0": return RoadDirection.Both;
            }
        }

        var impliedOneway = highway is "motorway" or "motorway_link" ||
                            tags.TryGetValue("junction", out var junction) && junction is "roundabout" or "circular";

        return impliedOneway ? RoadDirection.Forward : RoadDirection.Both;
    }

    // Обмеження, що не стосується автомобілів («except=motorcar» тощо), пропускаємо.
    public static bool AppliesToCars(TagsCollectionBase tags) =>
        !tags.TryGetValue("except", out var except) ||
        !except.Split(';', StringSplitOptions.TrimEntries).Intersect(CarVehicleClasses).Any();
}
