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
        
        ParseStartParameters(mainData, streamReader);

        var parseParameters = new DataParserParameters { Demand = mainData.DemandSize };

        ParsePoints(mainData, streamReader, parseParameters);
        ParseCars(mainData, streamReader, parseParameters);
        ParseCommonParameters(mainData, streamReader);
        ParseDistances(mainData, streamReader);
        ParseTimes(mainData, streamReader);
        ParseTariffs(mainData, streamReader);
        SkipSection(streamReader);
        ParseAlternativePoints(mainData, streamReader);
        
        return mainData;
    }

    private void ParseStartParameters(MainData result, StreamReader streamReader)
    {
        SkipLines(streamReader, 1);

        result.DemandSize = int.Parse(streamReader.ReadLine()!);

        SkipLines(streamReader, 1);

        result.TimeWindows = int.Parse(streamReader.ReadLine()!);
        
        SkipLines(streamReader, 3);
    }

    private void ParseCars(MainData result, StreamReader streamReader, DataParserParameters parserParameters)
    {
        while (true)
        {
            var line = streamReader.ReadLine();

            if (string.IsNullOrEmpty(line) || line.IsDividerLine()) 
                return;

            var car = _carDataParser.Parse(line, parserParameters);
            
            result.Cars.Add(car);
        }
    }
    
    private void ParsePoints(MainData result, StreamReader streamReader, DataParserParameters parserParameters)
    {
        var index = 0;
        
        while (true)
        {
            var line = streamReader.ReadLine();

            if (string.IsNullOrEmpty(line) || line.IsDividerLine()) 
                return;

            var point = _pointDataParser.Parse(line, parserParameters);

            point.Index = index;
            index++;
            
            if (point.Id == Constants.DepoPointId) result.DepoPoint = point;
            else result.PointsByIds.Add(point.Index, point);
        }
    }

    private void ParseCommonParameters(MainData mainData, StreamReader streamReader)
    {
        SkipLines(streamReader, 1);
        
        mainData.MaxOverload = int.Parse(streamReader.ReadLine()!.Split(Constants.DefaultSplitDividers)[1]);
        mainData.MaxCompsOverload = int.Parse(streamReader.ReadLine()!.Split(Constants.DefaultSplitDividers)[1]);
        
        SkipLines(streamReader, 12);
    }

    private void ParseDistances(MainData mainData, StreamReader streamReader)
    {
        mainData.Distances = _distancesDataParser.Parse(streamReader);
    }
    
    private void ParseTimes(MainData mainData, StreamReader streamReader)
    {
        mainData.Times = _timesDataParser.Parse(streamReader);
    }
    
    private void ParseTariffs(MainData mainData, StreamReader streamReader)
    {
        mainData.Tariffs = _tariffsDataParser.Parse(streamReader);
    }

    private void ParseAlternativePoints(MainData mainData, StreamReader streamReader)
    {
        mainData.AlternativePoints = _alternativePointsDataParser.Parse(streamReader);
    }
    
    private void SkipLines(StreamReader streamReader, int count)
    {
        for (int i = 0; i < count; i++)
        {
            streamReader.ReadLine();
        }
    }
    
    private void SkipSection(StreamReader streamReader)
    {
        while (true)
        {
            var line = streamReader.ReadLine();
            
            if (string.IsNullOrEmpty(line) || line.IsDividerLine())
                return;
        }
    }
}