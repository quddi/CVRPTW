namespace CVRPTW;

public abstract class StreamDataParser<T> where T : class
{
    public abstract T Parse(StreamReader streamReader);
}