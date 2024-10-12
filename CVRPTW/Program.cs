using CVRPTW;

PointDataParser carDataParser = new();

var point = carDataParser.Parse(Console.ReadLine()!, new DataParserParameters { Demand = 1 });

Console.WriteLine(point);