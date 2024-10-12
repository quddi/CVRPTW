namespace CVRPTW;

public abstract class StreamDataParser<T> where T : class, new()
{
    public abstract (T result, string? stopLine) Parse(StreamReader streamReader);
}