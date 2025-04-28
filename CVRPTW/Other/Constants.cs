using System.Text;

namespace CVRPTW;

public static class Constants
{
    public const string AnyPointElementValue = "*";
    public const char TitleDividerSymbol = '=';
    public const int DefaultMatrixId = 0;
    public const int DepoPointId = -1;
    public const int NotSelectedIndex = -1;
    public const int OnlyPointsIndex = 0;
    public const int AllResultsIndex = 1;
    public const int ServiceIndexesCount = 2;
    public const double DoubleComparisonTolerance = 0.000001;
    public const int MissingPointDemand = 1;
    public const int MissingCarCapacity = 50;
        
    public static readonly char[] DefaultSplitDividers = [','];
    public static readonly char[] PointsSplitDividers = ['(', ';', ')'];
    
    public static readonly StringBuilder SharedStringBuilder = new();
}
