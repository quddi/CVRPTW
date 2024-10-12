using CVRPTW.ParserParameters;

namespace CVRPTW;

/*
 * id\capacity (* demand)\tw open\tw close\overload penalty\
 * (start point id; *; end point id)\-\driver overtime penalty\
 * can start late (-?)\max capacity (Ukraine only)\max capacity
 * penalty\тариф\max trip count (always 1)\wait between trips
 * (always 0)\-\-\-\-\-\-\-\-\-
 */
public class CarDataParser : DataParser<Car>
{
    public override Car Parse(string line, DataParserParameters dataParserParameters)
    {
        SetFields(line, dataParserParameters);

        ParseId();
        ParseCapacities();
        ParseTimeWindow();
        ParseOverloadPenalty();
        ParsePointsInfos();
        SkipSplitElements(1);
        ParseDriverOvertimePenalty();
        SkipSplitElements(1);
        ParseMaxCapacity();
        ParseMaxCapacityPenalty();
        ParseTariff();
        ParseMaxTripsCount();
        ParseWaitBetweenTrips();
        
        var car = _result;
        
        ClearState();
        
        return car!;
    }

    private void ParseId()
    {
        _result!.Id = int.Parse(_split![_splitIndex!.Value]);
        _splitIndex++;
    }

    private void ParseCapacities()
    {
        _result!.Capacities = new();
        
        for (int i = _splitIndex!.Value; i < _parameters!.Value.Demand + _splitIndex!.Value; i++)
        {
            var capacity = int.Parse(_split![i]);
            _result.Capacities.Add(capacity);
        }

        _splitIndex += _parameters!.Value.Demand;
    }

    private void ParseTimeWindow()
    {
        _result!.TimeWindow = new TimeWindow
        {
            Start = int.Parse(_split![_splitIndex!.Value]),
            End = int.Parse(_split![_splitIndex!.Value + 1])
        };

        _splitIndex += 2;
    }

    private void ParseOverloadPenalty()
    {
        _result!.OverloadPenalty = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }

    private void ParsePointsInfos()
    {
        _result!.PointsInfos = new();

        var subSplit = _split![_splitIndex!.Value].Split(Constants.PointsSplitDividers, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var element in subSplit)
        {
            _result!.PointsInfos.Add(new PointVisitInfo
            (
                element == Constants.AnyPointElementValue ? null : int.Parse(element)
            ));
        }

        _splitIndex++;
    }

    private void ParseDriverOvertimePenalty()
    {
        _result!.DriverOvertimePenalty = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
    
    private void ParseMaxCapacity()
    {
        _result!.MaxCapacity = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
    
    private void ParseMaxCapacityPenalty()
    {
        _result!.MaxCapacityPenalty = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
    
    private void ParseTariff()
    {
        _result!.Tariff = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
    
    private void ParseMaxTripsCount()
    {
        _result!.MaxTripsCount = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
    
    private void ParseWaitBetweenTrips()
    {
        _result!.WaitBetweenTrips = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
}