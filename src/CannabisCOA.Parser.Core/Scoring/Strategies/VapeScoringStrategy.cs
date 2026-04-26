using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Scoring.Strategies;

public class VapeScoringStrategy : ICoaScoringStrategy
{
    public CoaScoreResult Score(CoaResult coa)
    {
        var breakdown = new Dictionary<string, int>();

        // Potency is king for vapes
        var thc = coa.Cannabinoids.TotalTHC;
        var potencyScore = thc switch
        {
            >= 85 => 50,
            >= 75 => 45,
            >= 65 => 40,
            >= 55 => 30,
            _ => 20
        };
        breakdown["Potency"] = potencyScore;

        // Terpenes matter less
        var terps = coa.Terpenes.TotalTerpenes;
        var terpScore = terps switch
        {
            >= 6 => 20,
            >= 4 => 15,
            >= 2 => 10,
            _ => 5
        };
        breakdown["Terpenes"] = terpScore;

        // Freshness
        var freshnessScore = coa.Freshness.Score switch
        {
            >= 90 => 20,
            >= 70 => 15,
            >= 50 => 10,
            _ => 5
        };
        breakdown["Freshness"] = freshnessScore;

        // Compliance
        var complianceScore = coa.Compliance.Passed ? 10 : 0;
        breakdown["Compliance"] = complianceScore;

        var total = breakdown.Values.Sum();

        var tier = total switch
        {
            >= 90 => "Elite",
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