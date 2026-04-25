using CannabisCOA.Parser.Core.Analysis;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaScorerTests
{
    [Fact]
    public void Analyze_Returns_Score()
    {
        var text = @"
            THC: 0.42%
            THCA: 24.88%
            Beta-Myrcene: 0.82%
            Limonene: 0.41%
            Test Date: 01/01/2026
            Result: PASS
        ";

        var result = CoaAnalyzer.Analyze(text);

        Assert.True(result.Score.Score > 0);
        Assert.False(string.IsNullOrEmpty(result.Score.Tier));
    }
}