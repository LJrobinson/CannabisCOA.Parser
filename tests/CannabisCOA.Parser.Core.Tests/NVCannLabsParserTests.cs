using CannabisCOA.Parser.Core.Adapters.Labs.NVCannLabs;
using CannabisCOA.Parser.Core.Enums;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class NVCannLabsParserTests
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
    public void NVCannLabsAdapter_Parse_RealFlowerFixtureDetectsHeaderFields()
    {
        var text = File.ReadAllText(FixturePath("nvcannlabs-flower-real-001.txt"));

        var result = new NVCannLabsAdapter().Parse(text);

        Assert.Equal("NV Cann Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.NotNull(result.TestDate);
        Assert.Equal(30.782m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.894m, result.Cannabinoids.THC.Value);
    }

    [Theory]
    [InlineData("CBD", "CBD 0.083 <LOQ <LOQ")]
    [InlineData("CBDA", "CBDa 0.083 <LOQ <LOQ")]
    [InlineData("CBD", "CBD 0.083 ND ND")]
    public void NVCannLabsAdapter_Parse_CbdNonDetectRows_MapToZeroConfidence(string cannabinoidName, string cannabinoidRow)
    {
        var text = $"""
        NV Cann Labs
        Product Type: Plant, Flower - Cured
        THCa 0.083 30.782 307.82
        Δ9-THC 0.083 0.894 8.94
        {cannabinoidRow}
        """;

        var result = new NVCannLabsAdapter().Parse(text);
        var field = cannabinoidName == "CBDA"
            ? result.Cannabinoids.CBDA
            : result.Cannabinoids.CBD;

        Assert.Equal(0m, field.Value);
        Assert.Equal(0m, field.Confidence);
    }

    [Fact]
    public void NVCannLabsAdapter_Parse_ConcentrateSideBySideRows_DoesNotBleedTerpenesIntoCannabinoids()
    {
        var text = """
        NV Cann Labs
        Product Type: Concentrate
        Cannabinoids Terpenes
        Analyte LOQ Mass Mass
        % % mg/g
        Δ9-THC 0.106 10.125 1.420 Caryophyllene Oxide 0.015 <LOQ <LOQ
        THCa 0.106 <LOQ <LOQ Camphene 0.015 <LOQ <LOQ
        CBD 0.106 <LOQ <LOQ 3-Carene 0.015 <LOQ <LOQ
        CBDa 0.106 <LOQ <LOQ δ -Limonene 0.015 <LOQ <LOQ
        """;

        var result = new NVCannLabsAdapter().Parse(text);

        Assert.Equal(ProductType.Concentrate, result.ProductType);
        Assert.Equal(1.420m, result.Cannabinoids.THC.Value);
        Assert.Equal(0m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0m, result.Cannabinoids.CBD.Value);
        Assert.Equal(0m, result.Cannabinoids.CBDA.Value);
        Assert.Equal(1.420m, result.Cannabinoids.TotalTHC);
        Assert.DoesNotContain("Caryophyllene Oxide", result.Cannabinoids.THC.SourceText);
        Assert.DoesNotContain("Camphene", result.Cannabinoids.THCA.SourceText);
        Assert.DoesNotContain("3-Carene", result.Cannabinoids.CBD.SourceText);
        Assert.DoesNotContain("Limonene", result.Cannabinoids.CBDA.SourceText);
    }

    [Fact]
    public void NVCannLabsAdapter_Parse_RealFlowerFixture_NormalizesTotalTerpenesToPercent()
    {
        var text = File.ReadAllText(FixturePath("nvcannlabs-flower-real-001.txt"));

        var result = new NVCannLabsAdapter().Parse(text);

        Assert.Equal(2.5379m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void NVCannLabsAdapter_Parse_RealFlowerFixture_MapsExpectedIndividualTerpenes()
    {
        var text = File.ReadAllText(FixturePath("nvcannlabs-flower-real-001.txt"));

        var result = new NVCannLabsAdapter().Parse(text);

        Assert.Equal(0.5920m, result.Terpenes.Terpenes["Linalool"]);
        Assert.Equal(0.5567m, result.Terpenes.Terpenes["δ-Limonene"]);
        Assert.Equal(0.4655m, result.Terpenes.Terpenes["β-Caryophyllene"]);
    }

    [Fact]
    public void NVCannLabsAdapter_Parse_RealFlowerFixture_TerpeneTotalMatchesSumWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("nvcannlabs-flower-real-001.txt"));

        var result = new NVCannLabsAdapter().Parse(text);

        var terpeneSum = result.Terpenes.Terpenes.Values
            .Where(percent => percent > 0m)
            .Sum();

        Assert.InRange(terpeneSum, result.Terpenes.TotalTerpenes - 0.1m, result.Terpenes.TotalTerpenes + 0.1m);
    }

    [Fact]
    public void NVCannLabsAdapter_Parse_RealFlowerFixture_TotalThcMatchesFormulaWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("nvcannlabs-flower-real-001.txt"));

        var result = new NVCannLabsAdapter().Parse(text);

        var thca = result.Cannabinoids.THCA.Value;
        var thc = result.Cannabinoids.THC.Value;
        var delta8 = 0m;

        var expectedTotalThc = (thca * 0.877m) + thc + delta8;

        // Allow small lab rounding differences
        Assert.True(Math.Abs(result.Cannabinoids.TotalTHC - expectedTotalThc) <= 0.02m);

        // NV Cann reports ~27.890
        var roundedExpected = Math.Round(expectedTotalThc, 3);
        Assert.True(roundedExpected is 27.890m or 27.891m);

        var roundedActual = Math.Round(result.Cannabinoids.TotalTHC, 3);
        Assert.True(roundedActual is 27.890m or 27.891m);
    }

    [Fact]
    public void NVCannLabsAdapter_Parse_ExpandedTerpeneAliases_UsesPercentColumn()
    {
        var text = """
        NV Cann Labs
        Product Type: Plant, Flower - Cured
        44.000 mg/g
        Total Terpenes
        Analyte LOQ Mass Mass
        β -Ocimene 0.083 2.833 0.2833
        delta-3-Carene 0.083 0.309 0.0309
        γ -Terpinene 0.083 0.144 0.0144
        p-Cymene 0.083 0.222 0.0222
        Valencene 0.083 0.111 0.0111
        """;

        var result = new NVCannLabsAdapter().Parse(text);

        Assert.Equal(4.4000m, result.Terpenes.TotalTerpenes);
        Assert.Equal(0.2833m, result.Terpenes.Terpenes["β-Ocimene"]);
        Assert.Equal(0.0309m, result.Terpenes.Terpenes["δ-3-Carene"]);
        Assert.Equal(0.0144m, result.Terpenes.Terpenes["γ-Terpinene"]);
        Assert.Equal(0.0222m, result.Terpenes.Terpenes["p-Cymene"]);
        Assert.Equal(0.0111m, result.Terpenes.Terpenes["Valencene"]);
        Assert.DoesNotContain(0.083m, result.Terpenes.Terpenes.Values);
    }
}
