using CannabisCOA.Parser.Core.Adapters.Labs.G3Labs;
using CannabisCOA.Parser.Core.Enums;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class G3LabsParserTests
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
    public void G3LabsAdapter_Parse_RealFlowerFixtureDetectsHeaderFields()
    {
        var text = File.ReadAllText(FixturePath("g3-flower-real-001.txt"));

        var result = new G3LabsAdapter().Parse(text);

        Assert.Equal("G3 Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Gorilla Glue #4", result.ProductName);
        Assert.Equal("9580 3311 4391 4182", result.BatchId);
        Assert.NotNull(result.TestDate);
        Assert.Equal(28.66m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.51m, result.Cannabinoids.THC.Value);
    }

    [Fact]
    public void G3LabsAdapter_Parse_DisplayProductFixture_MapsFlowerMetadata()
    {
        var text = File.ReadAllText(FixturePath("g3-flower-display-bud-garlic-cookies.txt"));

        var result = new G3LabsAdapter().Parse(text);

        Assert.Equal("G3 Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Bud Garlic Cookies", result.ProductName);
        Assert.Equal("359", result.BatchId);
    }

    [Theory]
    [InlineData("CBD", "CBD 0.00016 <LOQ <LOQ")]
    [InlineData("CBDA", "CBDa 0.00016 <LOQ <LOQ")]
    [InlineData("CBD", "CBD 0.00016 ND ND")]
    public void G3LabsAdapter_Parse_CbdNonDetectRows_MapToZeroConfidence(string cannabinoidName, string cannabinoidRow)
    {
        var text = $"""
        G3 Labs
        Product Type: Plant, Flower - Cured
        THCa 0.00016 28.66 286.6
        Δ9-THC 0.00016 0.51 5.1
        {cannabinoidRow}
        """;

        var result = new G3LabsAdapter().Parse(text);
        var field = cannabinoidName == "CBDA"
            ? result.Cannabinoids.CBDA
            : result.Cannabinoids.CBD;

        Assert.Equal(0m, field.Value);
        Assert.Equal(0m, field.Confidence);
    }

    [Fact]
    public void G3LabsAdapter_Parse_RealFlowerFixtureNormalizesTotalTerpenesToPercent()
    {
        var text = File.ReadAllText(FixturePath("g3-flower-real-001.txt"));

        var result = new G3LabsAdapter().Parse(text);

        Assert.NotNull(result.Terpenes);
        Assert.Equal(1.006m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void G3LabsAdapter_Parse_RealFlowerFixture_MapsExpectedIndividualTerpenes()
    {
        var text = File.ReadAllText(FixturePath("g3-flower-real-001.txt"));

        var result = new G3LabsAdapter().Parse(text);

        Assert.Equal(0.740m, result.Terpenes.Terpenes["β-Caryophyllene"]);
        Assert.Equal(0.227m, result.Terpenes.Terpenes["α-Humulene"]);
        Assert.Equal(0.040m, result.Terpenes.Terpenes["δ-Limonene"]);
    }

    [Fact]
    public void G3LabsAdapter_Parse_RealFlowerFixture_TerpeneTotalMatchesSumWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("g3-flower-real-001.txt"));

        var result = new G3LabsAdapter().Parse(text);

        var terpeneSum = result.Terpenes.Terpenes.Values
            .Where(percent => percent > 0m)
            .Sum();

        Assert.InRange(terpeneSum, result.Terpenes.TotalTerpenes - 0.1m, result.Terpenes.TotalTerpenes + 0.1m);
        Assert.Equal(1.006m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void G3LabsAdapter_Parse_RealFlowerFixture_TotalThcMatchesFormula()
    {
        var text = File.ReadAllText(FixturePath("g3-flower-real-001.txt"));

        var result = new G3LabsAdapter().Parse(text);

        var thca = result.Cannabinoids.THCA.Value;
        var thc = result.Cannabinoids.THC.Value;
        var delta8 = 0m;
        var expectedTotalThc = (thca * 0.877m) + thc + delta8;

        Assert.Equal(
            Math.Round(expectedTotalThc, 2),
            Math.Round(result.Cannabinoids.TotalTHC, 2));
        Assert.Equal(25.64m, Math.Round(expectedTotalThc, 2));
    }
}
