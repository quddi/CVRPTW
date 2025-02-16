using System.Text;

namespace CVRPTW;

public static class Constants
{
    public const string AnyPointElementValue = "*";
    public const char TitleDividerSymbol = '=';
    public const int DefaultMatrixId = 0;
    public const int DepoPointId = -1;
    public const int NotSelectedIndex = -1;
        
    public static readonly char[] DefaultSplitDividers = [','];
    public static readonly char[] PointsSplitDividers = ['(', ';', ')'];
    
    public static StringBuilder SharedStringBuilder = new StringBuilder();
}
