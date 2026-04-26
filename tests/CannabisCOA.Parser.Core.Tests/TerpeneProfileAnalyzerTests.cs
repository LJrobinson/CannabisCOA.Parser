using CannabisCOA.Parser.Core.Analysis;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class TerpeneProfileAnalyzerTests
{
    [Fact]
    public void Analyze_Returns_Dominant_And_Profile()
    {
        var text = @"
            Digipath Labs
            Product Type: Flower
            THC: 0.42%
            THCA: 24.88%
            Beta-Myrcene: 0.82%
            Limonene: 0.41%
            Beta-Caryophyllene: 0.38%
            Test Date: 01/01/2026
            Result: PASS
        ";

        var result = CoaAnalyzer.Analyze(text);

        Assert.Equal("Beta-Myrcene", result.Profile.DominantTerpene);
        Assert.Contains("Beta-Myrcene", result.Profile.TopTerpenes);
        Assert.Equal("Earthy / Citrus", result.Profile.ProfileType);
        Assert.Equal("Indica-Leaning", result.Profile.Lean);
    }
}