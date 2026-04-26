using CannabisCOA.Parser.Core.Analysis;
using CannabisCOA.Parser.Core.Enums;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaScoringStrategyTests
{
    [Fact]
    public void Uses_Vape_Scoring_For_Vape_Product()
    {
        var text = @"
            Digipath Labs
            Product Type: Vape
            THC: 80%
            Test Date: 01/01/2026
            Result: PASS
        ";

        var result = CoaAnalyzer.Analyze(text);

        Assert.Equal(ProductType.Vape, result.Coa.ProductType);
        Assert.True(result.Score.Score > 0);
    }
}