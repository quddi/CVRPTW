using System.Globalization;

namespace CVRPTW;

public class TariffsDataParser : LinedStreamDataParser<Tariffs>
{
    protected override void ManageLine(string lastReadLine, Tariffs tariffs)
    {
        var split = lastReadLine.Split(Constants.DefaultSplitDividers);
        var matrixIndex = int.Parse(split[0]);
        var firstPointIndex = int.Parse(split[1]);
        var secondPointIndex = int.Parse(split[2]);
        var distance = int.Parse(split[3]);
            
        tariffs.AddTariff(matrixIndex, firstPointIndex, secondPointIndex, distance);
    }
}