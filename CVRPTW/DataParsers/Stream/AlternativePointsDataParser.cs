namespace CVRPTW;

public class AlternativePointsDataParser : LinedStreamDataParser<AlternativePoints>
{
    protected override void ManageLine(string lastReadLine, AlternativePoints alternativePoints)
    {
        var split = lastReadLine.Split(Constants.DefaultSplitDividers);
        
        alternativePoints.AddAlternativePoint(int.Parse(split[0]), int.Parse(split[1]));
    }
}