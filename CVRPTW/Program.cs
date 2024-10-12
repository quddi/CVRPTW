using CVRPTW;
using CVRPTW.ParserParameters;

CarDataParser carDataParser = new();

var car = carDataParser.Parse(Console.ReadLine()!, new DataParserParameters { Demand = 1 });

Console.WriteLine(car);