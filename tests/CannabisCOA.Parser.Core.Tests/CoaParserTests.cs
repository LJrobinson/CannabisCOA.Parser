using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaParserTests
{
    [Fact]
    public void Parse_Returns_Cannabinoids_With_Calculated_Totals()
    {
        var text = @"
            THC: 0.42%
            THCA: 24.88%
            CBD: 0.05%
            CBDA: 0.12%
        ";

        var result = CoaParser.Parse(text);

        Assert.Equal(0.42m, result.Cannabinoids.THC.Value);
        Assert.Equal(24.88m, result.Cannabinoids.THCA.Value);
        Assert.Equal(22.24m, Math.Round(result.Cannabinoids.TotalTHC, 2));

        Assert.Equal(0.05m, result.Cannabinoids.CBD.Value);
        Assert.Equal(0.12m, result.Cannabinoids.CBDA.Value);
        Assert.Equal(0.16m, Math.Round(result.Cannabinoids.TotalCBD, 2));
    }
}