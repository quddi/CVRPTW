namespace CVRPTW;

public class TimesDataParser : LinedStreamDataParser<Times>
{
    protected override void ManageLine(string lastReadLine, Times times)
    {
        var split = lastReadLine.Split(Constants.DefaultSplitDividers);
        var matrixIndex = int.Parse(split[0]);
        var firstPointIndex = int.Parse(split[1]);
        var secondPointIndex = int.Parse(split[2]);
        var time = int.Parse(split[3]);
            
        times.AddTime(matrixIndex, firstPointIndex, secondPointIndex, time);
    }
}