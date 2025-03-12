using System.Globalization;

namespace CVRPTW;

public class DistancesDataParser : LinedStreamDataParser<Distances>
{
    protected override void ManageLine(string lastReadLine, Distances distances)
    {
        var split = lastReadLine.Split(Constants.DefaultSplitDividers);
        var matrixIndex = int.Parse(split[0]);
        var firstPointIndex = int.Parse(split[1]);
        var secondPointIndex = int.Parse(split[2]);
        var distance = double.Parse(split[3], CultureInfo.InvariantCulture);
            
        distances.AddDistance(matrixIndex, firstPointIndex, secondPointIndex, distance);
    }
}