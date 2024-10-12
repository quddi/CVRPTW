namespace CVRPTW;

public static class ExtensionsMethods
{
    public static bool IsDividerLine(this string line)
    {
        return line.Contains(Constants.TitleDividerSymbol);
    }
}