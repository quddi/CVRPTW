using System.Globalization;

namespace CVRPTW;

public abstract class LinedStreamDataParser<T> : StreamDataParser<T> where T : class, new()
{
    public override T Parse(StreamReader streamReader)
    {
        var t = new T();
        var lastReadLine = streamReader.ReadLine();

        while (!string.IsNullOrEmpty(lastReadLine) && !lastReadLine.IsDividerLine())
        {
            ManageLine(lastReadLine, t);
            lastReadLine = streamReader.ReadLine();
        }

        return t;
    }

    protected abstract void ManageLine(string lastReadLine, T t);
}