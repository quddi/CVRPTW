﻿using System.Globalization;

namespace CVRPTW;

/*
 * comps - unic position index\id\latitude\longitude\demand\[open tw\
 * close tw у секундах від початку доби (можуть бути + tw)]\service time\
 * -\late penalty - штраф за одну хвилину запізнення, додається до цільової
 * функції\wait штраф\-\-\-\-
 */
public class PointDataParser : LineDataParser<Point>
{
    public override Point Parse(string line, DataParserParameters dataParserParameters)
    {
        SetFields(line, dataParserParameters);

        ParseComp();
        ParseId();
        ParseCoordinates();
        ParseDemand();
        ParseTimeWindows();
        ParseServiceTime();
        SkipSplitElements(1);
        ParseLatePenalty();
        ParseWaitPenalty();
        
        var point = _result;
        
        ClearState();

        return point!;
    }

    private void ParseComp()
    {
        _result!.Comp = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
    
    private void ParseId()
    {
        _result!.Id = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
    
    private void ParseCoordinates()
    {
        var coordinates = new Coordinates
        {
            Latitude = double.Parse(_split![_splitIndex!.Value], CultureInfo.InvariantCulture),
            Longitude = double.Parse(_split![_splitIndex!.Value + 1], CultureInfo.InvariantCulture)
        };

        _result!.Coordinates = coordinates;

        _splitIndex += 2;
    }
    
    private void ParseDemand()
    {
        var hasDemand = _parameters!.Value.Demand != 0;
        
        _result!.Demand = hasDemand
            ? double.Parse(_split![_splitIndex!.Value], CultureInfo.InvariantCulture)
            : Constants.MissingPointDemand;

        if (hasDemand) _splitIndex++;
    }

    private void ParseTimeWindows()
    {
        _result!.TimeWindows = new();

        for (int i = 0; i < _parameters!.Value.Demand; i++)
        {
            _result.TimeWindows.Add
            (
                new TimeWindow
                {
                    Start = double.Parse(_split![_splitIndex!.Value + i * 2], CultureInfo.InvariantCulture).FromMinutesToSeconds(),
                    End = double.Parse(_split![_splitIndex!.Value + i * 2 + 1], CultureInfo.InvariantCulture).FromMinutesToSeconds()
                }
            );
        }

        _splitIndex += _parameters!.Value.Demand * 2;
    }
    
    private void ParseServiceTime()
    {
        _result!.ServiceTime = int.Parse(_split![_splitIndex!.Value]);

        _splitIndex++;
    }
    
    private void ParseLatePenalty()
    {
        _result!.LatePenalty = int.Parse(_split![_splitIndex!.Value]).FromMinutesToSeconds();

        _splitIndex++;
    }
    
    private void ParseWaitPenalty()
    {
        _result!.WaitPenalty = int.Parse(_split![_splitIndex!.Value]).FromMinutesToSeconds();

        _splitIndex++;
    }
}