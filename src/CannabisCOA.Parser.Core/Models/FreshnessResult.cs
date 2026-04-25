namespace CannabisCOA.Parser.Core.Models;

public class FreshnessResult
{
    public int? DaysSinceTest { get; set; }
    public int Score { get; set; }
    public string Band { get; set; } = "Unknown";
}