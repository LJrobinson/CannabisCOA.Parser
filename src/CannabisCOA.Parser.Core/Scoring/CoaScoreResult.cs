namespace CannabisCOA.Parser.Core.Scoring;

public class CoaScoreResult
{
    public int Score { get; set; }
    public string Tier { get; set; } = "";

    public Dictionary<string, int> Breakdown { get; set; } = new();
}