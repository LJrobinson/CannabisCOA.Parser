using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Scoring.Strategies;

public class FlowerScoringStrategy : ICoaScoringStrategy
{
    public CoaScoreResult Score(CoaResult coa)
    {
        var breakdown = new Dictionary<string, int>();

        // Potency
        var thc = coa.Cannabinoids.TotalTHC;
        var potencyScore = thc switch
        {
            >= 30 => 40,
            >= 25 => 35,
            >= 20 => 30,
            >= 15 => 20,
            >= 10 => 10,
            _ => 5
        };
        breakdown["Potency"] = potencyScore;

        // Terpenes
        var terps = coa.Terpenes.TotalTerpenes;
        var terpScore = terps switch
        {
            >= 3.5m => 25,
            >= 2.5m => 20,
            >= 1.5m => 15,
            >= 1.0m => 10,
            _ => 5
        };
        breakdown["Terpenes"] = terpScore;

        // Freshness
        var freshnessScore = coa.Freshness.Score switch
        {
            >= 90 => 25,
            >= 70 => 20,
            >= 50 => 15,
            >= 30 => 10,
            _ => 5
        };
        breakdown["Freshness"] = freshnessScore;

        // Compliance
        var complianceScore = coa.Compliance.Passed ? 10 : 0;
        breakdown["Compliance"] = complianceScore;

        var total = breakdown.Values.Sum();

        var tier = total switch
        {
            >= 90 => "God Tier",
            >= 80 => "Fire",
            >= 70 => "Solid",
            >= 60 => "Mid",
            _ => "Skip"
        };

        return new CoaScoreResult
        {
            Score = total,
            Tier = tier,
            Breakdown = breakdown
        };
    }
}