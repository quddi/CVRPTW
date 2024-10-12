using System.Globalization;

namespace CVRPTW;

public abstract class MatrixDataParser<T> : StreamDataParser<T> where T : class, new()
{
    public override (T result, string? stopLine) Parse(StreamReader streamReader)
    {
        var t = new T();
        var lastReadLine = streamReader.ReadLine();

        while (!string.IsNullOrEmpty(lastReadLine) && !lastReadLine.Contains(Constants.TitleDividerSymbol))
        {
            ManageLine(lastReadLine, t);
        }

        return (t, lastReadLine);
    }

    protected abstract void ManageLine(string lastReadLine, T t);
}