namespace CVRPTW;

public static class Singleton
{
    private static Dictionary<Type, object> _objects = new();
    
    public static T OfType<T>() where T : new()
    {
        var key = typeof(T);

        if (!_objects.ContainsKey(key))
            _objects[key] = new T();

        return (T)_objects[key];
    }
}