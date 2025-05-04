namespace CVRPTW;

public class AlternativePointsDataParser : LinedStreamDataParser<AlternativePoints>
{
    protected override void ManageLine(string lastReadLine, AlternativePoints alternativePoints)
    {
        var split = lastReadLine.Split(Constants.DefaultSplitDividers);
        
        alternativePoints.AddAlternativePoints(split.Select(int.Parse).ToArray());
    }
}