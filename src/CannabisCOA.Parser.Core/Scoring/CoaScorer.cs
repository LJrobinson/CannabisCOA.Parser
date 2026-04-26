using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Scoring.Strategies;

namespace CannabisCOA.Parser.Core.Scoring;

public static class CoaScorer
{
    public static CoaScoreResult Score(CoaResult coa)
    {
        var strategy = GetStrategy(coa);

        return strategy.Score(coa);
    }

    private static ICoaScoringStrategy GetStrategy(CoaResult coa)
    {
        return coa.ProductType switch
        {
            ProductType.Flower => new FlowerScoringStrategy(),
            ProductType.PreRoll => new FlowerScoringStrategy(), // same logic for now
            ProductType.Vape => new VapeScoringStrategy(),
            _ => new FlowerScoringStrategy()
        };
    }
}