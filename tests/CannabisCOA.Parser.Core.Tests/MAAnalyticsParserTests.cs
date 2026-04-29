using CannabisCOA.Parser.Core.Adapters.Labs.MAAnalytics;
using CannabisCOA.Parser.Core.Enums;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class MAAnalyticsParserTests
{
    private static string FixturePath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..",
            "Fixtures",
            "Labs",
            fileName));
    }

    [Fact]
    public void MAAnalyticsAdapter_Parse_RealFlowerFixtureDetectsHeaderFields()
    {
        var text = File.ReadAllText(FixturePath("ma-flower-real-001.txt"));

        var result = new MAAnalyticsAdapter().Parse(text);

        Assert.Equal("MA Analytics", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.NotNull(result.TestDate);
        Assert.Equal(26.278m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.544m, result.Cannabinoids.THC.Value);
    }

    [Theory]
    [InlineData("CBD", "CBD 0.640 <LOQ <LOQ")]
    [InlineData("CBDA", "CBDa 0.160 <LOQ <LOQ")]
    [InlineData("CBD", "CBD 0.640 ND ND")]
    public void MAAnalyticsAdapter_Parse_CbdNonDetectRows_MapToZeroConfidence(string cannabinoidName, string cannabinoidRow)
    {
        var text = $"""
        MA Analytics
        Product Type: Plant, Flower - Cured
        THCa 0.160 26.278 262.78
        Δ9-THC 0.640 0.544 5.44
        {cannabinoidRow}
        """;

        var result = new MAAnalyticsAdapter().Parse(text);
        var field = cannabinoidName == "CBDA"
            ? result.Cannabinoids.CBDA
            : result.Cannabinoids.CBD;

        Assert.Equal(0m, field.Value);
        Assert.Equal(0m, field.Confidence);
    }

    [Fact]
    public void MAAnalyticsAdapter_Parse_RealFlowerFixture_TotalThcMatchesFormulaWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("ma-flower-real-001.txt"));

        var result = new MAAnalyticsAdapter().Parse(text);

        var thca = result.Cannabinoids.THCA.Value;
        var thc = result.Cannabinoids.THC.Value;
        var delta8 = 0m;
        var expectedTotalThc = (thca * 0.877m) + thc + delta8;

        Assert.True(Math.Abs(result.Cannabinoids.TotalTHC - expectedTotalThc) <= 0.02m);
        var roundedExpected = Math.Round(expectedTotalThc, 3);

        Assert.True(
            roundedExpected is 23.590m or 23.591m,
            $"Expected formula rounded to 23.590 or 23.591 but was {roundedExpected}");

        var roundedTotalThc = Math.Round(result.Cannabinoids.TotalTHC, 3);
        Assert.True(roundedTotalThc is 23.590m or 23.591m);
    }

    [Fact]
    public void MAAnalyticsAdapter_Parse_RealFlowerFixture_MapsTotalTerpenesWithFullPrecision()
    {
        var text = File.ReadAllText(FixturePath("ma-flower-real-001.txt"));

        var result = new MAAnalyticsAdapter().Parse(text);

        Assert.NotNull(result.Terpenes);
        Assert.Equal(2.17251m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void MAAnalyticsAdapter_Parse_RealFlowerFixture_MapsExpectedIndividualTerpenes()
    {
        var text = File.ReadAllText(FixturePath("ma-flower-real-001.txt"));

        var result = new MAAnalyticsAdapter().Parse(text);

        Assert.Equal(0.66359m, result.Terpenes.Terpenes["β-Myrcene"]);
        Assert.Equal(0.53103m, result.Terpenes.Terpenes["β-Caryophyllene"]);
        Assert.Equal(0.41148m, result.Terpenes.Terpenes["δ-Limonene"]);
        Assert.Equal(0.23188m, result.Terpenes.Terpenes["Linalool"]);
        Assert.Equal(0.17647m, result.Terpenes.Terpenes["α-Humulene"]);
    }

    [Fact]
    public void MAAnalyticsAdapter_Parse_RealFlowerFixture_TerpeneTotalMatchesSumWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("ma-flower-real-001.txt"));

        var result = new MAAnalyticsAdapter().Parse(text);
        var terpeneSum = result.Terpenes.Terpenes.Values
            .Where(percent => percent > 0m)
            .Sum();

        // The fixture total is more precise than the subset of individual rows parsed here.
        Assert.True(Math.Abs(terpeneSum - result.Terpenes.TotalTerpenes) <= 0.1m);
    }
}
