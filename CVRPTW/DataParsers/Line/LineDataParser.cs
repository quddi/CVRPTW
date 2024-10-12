using CVRPTW.ParserParameters;

namespace CVRPTW;

public abstract class LineDataParser<T> where T : class, new()
{
    protected string? _line;
    protected DataParserParameters? _parameters;
    protected int? _splitIndex;
    protected string[]? _split;
    
    protected T? _result;
    
    public abstract T Parse(string line, DataParserParameters dataParserParameters);
    
    protected void SetFields(string line, DataParserParameters dataParserParameters)
    {
        _parameters = dataParserParameters;
        
        _split = line.Split(Constants.DefaultSplitDividers);
        _splitIndex = 0;
        _result = new();
    }

    protected void ClearState()
    {
        _parameters = null;
        _split = null;
        _splitIndex = null;
        _result = null;
    }

    protected void SkipSplitElements(int count)
    {
        _splitIndex += count;
    }
}