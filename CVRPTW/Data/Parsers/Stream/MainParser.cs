namespace CVRPTW;

public class MainParser : StreamDataParser<MainData>
{
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
        var parser = Singleton.OfType<CarDataParser>();
        
        while (true)
        {
            var line = streamReader.ReadLine();

            if (string.IsNullOrEmpty(line) || line.IsDividerLine()) 
                return;

            var car = parser.Parse(line, parserParameters);
            
            result.Cars.Add(car);
        }
    }
    
    private void ParsePoints(MainData result, StreamReader streamReader, DataParserParameters parserParameters)
    {
        var parser = Singleton.OfType<PointDataParser>();

        var index = 0;
        
        while (true)
        {
            var line = streamReader.ReadLine();

            if (string.IsNullOrEmpty(line) || line.IsDividerLine()) 
                return;

            var point = parser.Parse(line, parserParameters);

            point.Index = index;
            index++;
            
            if (point.Id == Constants.DepoPointId) result.DepoPoint = point;
            else result.Points.Add(point.Index, point);
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
        mainData.Distances = Singleton.OfType<DistancesDataParser>().Parse(streamReader);
    }
    
    private void ParseTimes(MainData mainData, StreamReader streamReader)
    {
        mainData.Times = Singleton.OfType<TimesDataParser>().Parse(streamReader);
    }
    
    private void ParseTariffs(MainData mainData, StreamReader streamReader)
    {
        mainData.Tariffs = Singleton.OfType<TariffsDataParser>().Parse(streamReader);
    }

    private void ParseAlternativePoints(MainData mainData, StreamReader streamReader)
    {
        mainData.AlternativePoints = Singleton.OfType<AlternativePointsDataParser>().Parse(streamReader);
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