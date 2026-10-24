namespace CVRPTW;

public class MainResult
{
    public double Estimation { get; set; }

    public int UnreachableEdgesCount { get; set; }

    public Dictionary<Car, CarResult> Results { get; set; } = new();

    public MainResult Clone()
    {
        return new MainResult
        {
            UnreachableEdgesCount = UnreachableEdgesCount,
            Results = Results.ToDictionary(pair => pair.Key, pair => pair.Value.Clone())  
        };
    }
    
    public override string ToString()
    {
        Constants.SharedStringBuilder.AppendLine($"{nameof(MainResult)} (total estimation: {Estimation.ToFormattedString()}, {nameof(UnreachableEdgesCount)}: {UnreachableEdgesCount}):");

        foreach (var (car, carResult) in Results)
        {
            Constants.SharedStringBuilder.Append("\t");
            Constants.SharedStringBuilder.AppendLine(carResult.ToString());
        }
        
        var result = Constants.SharedStringBuilder.ToString();
        
        Constants.SharedStringBuilder.Clear();
        
        return result;
    }
}