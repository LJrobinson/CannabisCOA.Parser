using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaParserDateTests
{
    [Fact]
    public void Parse_Extracts_TestDate_And_Calculates_Freshness()
    {
        var text = @"
            THC: 1.0%
            THCA: 20.0%
            Test Date: 01/01/2026
        ";

        var result = CoaParser.Parse(text);

        Assert.NotNull(result.TestDate);
        Assert.True(result.Freshness.DaysSinceTest >= 0);
        Assert.NotEqual("Unknown", result.Freshness.Band);
    }
}