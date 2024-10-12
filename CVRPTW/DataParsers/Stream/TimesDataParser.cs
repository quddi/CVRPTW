namespace CVRPTW;

public class TimesDataParser : MatrixDataParser<Times>
{
    protected override void ManageLine(string lastReadLine, Times times)
    {
        var split = lastReadLine.Split();
        var matrixIndex = int.Parse(split[0]);
        var firstPointId = int.Parse(split[1]);
        var secondPointId = int.Parse(split[2]);
        var time = int.Parse(split[3]);
            
        times.AddTime(matrixIndex, firstPointId, secondPointId, time);
    }
}