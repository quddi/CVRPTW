using CVRPTW;
using CVRPTW.ParserParameters;

PointDataParser carDataParser = new();

var point = carDataParser.Parse(Console.ReadLine()!, new DataParserParameters { Demand = 1 });

Console.WriteLine(point);