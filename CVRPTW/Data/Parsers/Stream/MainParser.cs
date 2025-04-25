using CVRPTW.Parsers;

namespace CVRPTW;

public class MainParser : StreamDataParser<MainData>
{
    private readonly CarDataParser _carDataParser = new();
    private readonly PointDataParser _pointDataParser = new();
    private readonly DistancesDataParser _distancesDataParser = new();
    private readonly TimesDataParser _timesDataParser = new();
    private readonly TariffsDataParser _tariffsDataParser = new();
    private readonly AlternativePointsDataParser _alternativePointsDataParser = new();
    
    public override MainData Parse(StreamReader streamReader)
    {
        var mainData = new MainData();
        
        var lastLine = ParseStartParameters(mainData, streamReader);

        var parseParameters = new DataParserParameters
        {
            Demand = mainData.DemandSize,
            Title = GetSectionName(lastLine)
        };

        while (true)
        {
            var parseResult = ParseSection(mainData, streamReader, parseParameters);

            if (parseResult.EndReached) break;
            
            parseParameters.Title = GetSectionName(parseResult.LastLine);
        }
        
        return mainData;
    }

    private SectionParseResult ParseSection(MainData mainData, StreamReader streamReader, DataParserParameters parserParameters)
    {
        //var title = streamReader.ReadLine()!;

        var result = new SectionParseResult { EndReached = false };
        
        switch (parserParameters.Title)
        {
            case "POINTS":
                result.LastLine = ParsePoints(mainData, streamReader, parserParameters);
                break;
            case "CARS":
                result.LastLine = ParseCars(mainData, streamReader, parserParameters);
                break;
            case "COMMON_PARAMETERS":
                result.LastLine = ParseCommonParameters(mainData, streamReader);
                break;
            case "DISTANCE":
                result.LastLine = ParseDistances(mainData, streamReader);
                break;
            case "TIME":
                result.LastLine = ParseTimes(mainData, streamReader);
                break;
            case "CAR_TARIFF":
                result.LastLine = ParseTariffs(mainData, streamReader);
                break;
            case "ALTERNATIVE_POINTS":
                result.LastLine = ParseAlternativePoints(mainData, streamReader);
                break;
            case "END":
                result.EndReached = true;
                result.LastLine = parserParameters.Title;
                break;
            case "AR_LINK_DIST_TIME":
            case "CAR_LINK_DIST_TIME":
            case "COMPS_CARS":
                result.LastLine = SkipSection(streamReader);
                break;
            default:
                throw new NotImplementedException("Unknown section: " + parserParameters.Title);
        }

        return result;
    }

    private string ParseStartParameters(MainData mainData, StreamReader streamReader)
    {
        SkipLines(streamReader, 1);
        
        mainData.DemandSize = int.Parse(streamReader.ReadLine()!);

        SkipLines(streamReader, 1);

        mainData.TimeWindows = int.Parse(streamReader.ReadLine()!);
        
        SkipLines(streamReader, 2);

        return streamReader.ReadLine()!;
    }

    private string ParsePoints(MainData result, StreamReader streamReader, DataParserParameters parserParameters)
    {
        var index = 0;
        string line = string.Empty;
        
        while (true)
        {
            line = streamReader.ReadLine()!;

            if (string.IsNullOrEmpty(line) || line.IsDividerLine()) 
                break;

            var point = _pointDataParser.Parse(line, parserParameters);

            point.Index = index;
            index++;
            
            result.Points.Add(point);
            
            if (point.Id == Constants.DepoPointId) result.DepoPoint = point;
            else result.PointsByIds.Add(point.Index, point);
        }

        return line;
    }

    private string ParseCars(MainData result, StreamReader streamReader, DataParserParameters parserParameters)
    {
        var line = string.Empty;
        
        while (true)
        {
            line = streamReader.ReadLine();

            if (string.IsNullOrEmpty(line) || line.IsDividerLine()) 
                return line!;

            var car = _carDataParser.Parse(line, parserParameters);
            
            result.Cars.Add(car);
        }
    }

    private string ParseCommonParameters(MainData mainData, StreamReader streamReader)
    {
        var line = string.Empty;
        
        while (true)
        {
            line = streamReader.ReadLine()!;
            
            if (string.IsNullOrEmpty(line) || line.IsDividerLine()) 
                break;
            
            var split = line.Split(Constants.DefaultSplitDividers);

            if (split[0] == "Penalty_Max_overload")
            {
                mainData.MaxOverload = long.Parse(split[1]);
                continue;
            }

            if (split[0] == "Penalty_Max_comps_overload")
            {
                mainData.MaxCompsOverload = long.Parse(split[1]);
            }
        }
        
        return line;
    }

    private string ParseDistances(MainData mainData, StreamReader streamReader)
    {
        mainData.Distances = _distancesDataParser.Parse(streamReader);

        return _distancesDataParser.LastReadLine;
    }
    
    private string ParseTimes(MainData mainData, StreamReader streamReader)
    {
        mainData.Times = _timesDataParser.Parse(streamReader);
        
        return _timesDataParser.LastReadLine;
    }
    
    private string ParseTariffs(MainData mainData, StreamReader streamReader)
    {
        mainData.Tariffs = _tariffsDataParser.Parse(streamReader);
        
        return _tariffsDataParser.LastReadLine;
    }

    private string ParseAlternativePoints(MainData mainData, StreamReader streamReader)
    {
        mainData.AlternativePoints = _alternativePointsDataParser.Parse(streamReader);
        
        return _alternativePointsDataParser.LastReadLine;
    }

    private string SkipSection(StreamReader streamReader)
    {
        while (true)
        {
            var line = streamReader.ReadLine();
            
            if (string.IsNullOrEmpty(line) || line.IsDividerLine())
                return line!;
        }
    }

    private void SkipLines(StreamReader streamReader, int count)
    {
        for (int i = 0; i < count; i++)
        {
            streamReader.ReadLine();
        }
    }

    private static string GetSectionName(string allTitle) => allTitle.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[0];
}