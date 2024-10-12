namespace CVRPTW;

public abstract class StreamDataParser<T> where T : class, new()
{
    public abstract T Parse(StreamReader streamReader);
}