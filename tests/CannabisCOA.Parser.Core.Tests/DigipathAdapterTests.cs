using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class DigipathAdapterTests
{
    [Fact]
    public void Parse_Detects_Digipath_And_Parses_Flower_Cannabinoids()
    {
        var text = @"
            Digipath Labs
            Product Type: Flower
            THC: 0.42%
            THCA: 24.88%
            CBD: 0.05%
            CBDA: 0.12%
            Test Date: 01/01/2026
            Overall Result: PASS
        ";

        var result = CoaParser.Parse(text);

        Assert.Equal("Digipath", result.LabName);
        Assert.Equal(0.42m, result.Cannabinoids.THC.Value);
        Assert.Equal(24.88m, result.Cannabinoids.THCA.Value);
        Assert.Equal(22.24m, Math.Round(result.Cannabinoids.TotalTHC, 2));
        Assert.Equal("pass", result.Compliance.Status);
        Assert.NotNull(result.TestDate);
    }

    [Fact]
    public void Parse_Handles_Digipath_Variant_Names()
    {
        var text = @"
            Digipath Laboratories
            Δ9-THC: 0.51 %
            THCa: 23.77 %
            CBD-A: 0.11 %
            Test Date: 01/01/2026
            Result: PASS
        ";

        var result = CoaParser.Parse(text);

        Assert.Equal("Digipath", result.LabName);
        Assert.Equal(0.51m, result.Cannabinoids.THC.Value);
        Assert.Equal(23.77m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.11m, result.Cannabinoids.CBDA.Value);
    }
}