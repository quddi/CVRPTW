using System.Globalization;

namespace CVRPTW;

public abstract class LinedStreamDataParser<T> : StreamDataParser<T> where T : class, new()
{
    public string LastReadLine { get; private set; } = string.Empty;
    
    public override T Parse(StreamReader streamReader)
    {
        var t = new T();
        LastReadLine = streamReader.ReadLine()!;

        while (!string.IsNullOrEmpty(LastReadLine) && !LastReadLine.IsDividerLine())
        {
            ManageLine(LastReadLine, t);
            LastReadLine = streamReader.ReadLine()!;
        }

        return t;
    }

    protected abstract void ManageLine(string lastReadLine, T t);
}