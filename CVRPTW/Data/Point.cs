namespace CVRPTW.Data;

/*
 * comps - unic position index\id\latitude\longitude\demand\[open tw\
 * close tw у секундах від початку доби (можуть бути + tw)]\service time\
 * -\late penalty - штраф за одну хвилину запізнення, додається до цільової
 * функції\wait штраф\-\-\-\-
 */
public class Point : IData
{
    public int Comp { get; set; }
    
    public int Id { get; set; }
    
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }
    
    public int Demand { get; set; }
    
    public List<TimeWindow> TimeWindows { get; set; }
    
    public int ServiceTime { get; set; }
    
    public int LatePenalty { get; set; }

    public int WaitPenalty { get; set; }
}