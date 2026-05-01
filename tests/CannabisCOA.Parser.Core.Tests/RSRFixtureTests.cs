using System;
using System.IO;
using CannabisCOA.Parser.Core.Adapters.Labs.RSRAnalytical;
using CannabisCOA.Parser.Core.Enums;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class RSRFixtureTests
{
    private static string LoadFixture(string name)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Labs",
            name);

        return File.ReadAllText(path);
    }

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
    public void Parses_RSR_Flower_Fixture_Correctly()
    {
        var text = LoadFixture("RSR_Flower.txt");

        var result = CoaParser.Parse(text);

        Assert.Equal("RSR Analytical Laboratories", result.LabName);

        Assert.Equal(37.74m, result.Cannabinoids.THCA.Value);
        Assert.Equal(1.12m, result.Cannabinoids.THC.Value);

        Assert.NotNull(result.TestDate);
        Assert.Equal(new DateTime(2026, 4, 1), result.TestDate);

        Assert.True(result.Compliance.Passed);

        Assert.Equal(2.143m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void RsrAnalyticalAdapter_Parse_RealFlowerFixtureDetectsHeaderFields()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-real-001.txt"));

        var result = new RSRAnalyticalAdapter().Parse(text);

        Assert.Equal("RSR Analytical Laboratories", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Bud Garlic Cookies", result.ProductName);
        Assert.Equal("354 GC 01-05-26-B1", result.BatchId);
        Assert.NotNull(result.TestDate);
        Assert.Equal(37.74m, result.Cannabinoids.THCA.Value);
        Assert.Equal(1.12m, result.Cannabinoids.THC.Value);
    }

    [Theory]
    [InlineData("CBD", "CBD 0.25 <LOQ <LOQ")]
    [InlineData("CBDA", "CBDa 0.25 <LOQ <LOQ")]
    [InlineData("CBD", "CBD 0.25 ND ND")]
    public void RsrAnalyticalAdapter_Parse_CbdNonDetectRows_MapToZeroConfidence(string cannabinoidName, string cannabinoidRow)
    {
        var text = $"""
        RSR Analytical Laboratories
        Product Type: Plant, Flower - Cured
        THCa 0.25 20.00 200.00
        Δ9-THC 0.25 1.00 10.00
        {cannabinoidRow}
        """;

        var result = new RSRAnalyticalAdapter().Parse(text);
        var field = cannabinoidName == "CBDA"
            ? result.Cannabinoids.CBDA
            : result.Cannabinoids.CBD;

        Assert.Equal(0m, field.Value);
        Assert.Equal(0m, field.Confidence);
    }

    [Fact]
    public void RsrAnalyticalAdapter_Parse_RealFlowerFixture_MapsExpectedCannabinoidValues()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-real-001.txt"));

        var result = new RSRAnalyticalAdapter().Parse(text);
        var document = CannabisCOA.Parser.Core.Mappers.CoaDocumentMapper.FromCoaResult(result);

        var thca = FindCannabinoid("THCA");
        var thc = FindCannabinoid("THC");

        Assert.Equal(37.74m, thca.Percent);
        Assert.Equal(1.12m, thc.Percent);
        Assert.Equal("%", thca.Unit);
        Assert.Equal("%", thc.Unit);
        Assert.False(string.IsNullOrEmpty(thca.SourceText));
        Assert.False(string.IsNullOrEmpty(thc.SourceText));

        CannabisCOA.Parser.Core.Models.CoaAnalyteResult FindCannabinoid(string name)
        {
            return document.Cannabinoids.Single(cannabinoid =>
                cannabinoid.Name == name ||
                cannabinoid.NormalizedName == name);
        }
    }

    [Fact]
    public void RsrAnalyticalAdapter_Parse_RealFlowerFixture_TotalThcMatchesFormulaWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-real-001.txt"));

        var result = new RSRAnalyticalAdapter().Parse(text);

        var thca = result.Cannabinoids.THCA.Value;
        var thc = result.Cannabinoids.THC.Value;
        var delta8 = 0m;
        var expectedTotalThc = (thca * 0.877m) + thc + delta8;

        Assert.True(Math.Abs(result.Cannabinoids.TotalTHC - expectedTotalThc) <= 0.02m);

        var roundedTotalThc = Math.Round(result.Cannabinoids.TotalTHC, 2);
        Assert.True(roundedTotalThc is 34.21m or 34.22m);
    }

    [Fact]
    public void RsrAnalyticalAdapter_Parse_RealFlowerFixture_MapsTotalTerpenes()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-real-001.txt"));

        var result = new RSRAnalyticalAdapter().Parse(text);

        Assert.NotNull(result.Terpenes);
        Assert.Equal(2.143m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void RsrAnalyticalAdapter_Parse_RealFlowerFixture_TerpeneTotalMatchesSumWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-real-001.txt"));

        var result = new RSRAnalyticalAdapter().Parse(text);
        var terpeneSum = result.Terpenes.Terpenes.Values
            .Where(percent => percent > 0m)
            .Sum();

        Assert.True(Math.Abs(terpeneSum - result.Terpenes.TotalTerpenes) <= 0.1m);
    }
}
