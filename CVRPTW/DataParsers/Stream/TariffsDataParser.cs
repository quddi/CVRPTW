using System.Globalization;

namespace CVRPTW;

public class TariffsDataParser : MatrixDataParser<Tariffs>
{
    protected override void ManageLine(string lastReadLine, Tariffs tariffs)
    {
        var split = lastReadLine.Split();
        var matrixIndex = int.Parse(split[0]);
        var firstPointId = int.Parse(split[1]);
        var secondPointId = int.Parse(split[2]);
        var distance = int.Parse(split[3]);
            
        tariffs.AddTariff(matrixIndex, firstPointId, secondPointId, distance);
    }
}