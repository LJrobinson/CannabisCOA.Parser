using System;
using System.IO;
using CannabisCOA.Parser.Core.Adapters.Labs.RSRAnalytical;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Validation;
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

    [Fact]
    public void RsrAnalyticalAdapter_Parse_TrimFixtureMapsFlowerMetadata()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-trim-medellin.txt"));

        var result = new RSRAnalyticalAdapter().Parse(text);

        Assert.Equal("RSR Analytical Laboratories", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("MED 13316", result.ProductName);
        Assert.Equal("111025-F2T6-MED", result.BatchId);
    }

    [Fact]
    public void RsrAnalyticalAdapter_Parse_BulkFlowerIndoorFixtureMapsFlowerMetadata()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-bulk-flower-indoor-garlic-cookies.txt"));

        var result = new RSRAnalyticalAdapter().Parse(text);

        Assert.Equal("RSR Analytical Laboratories", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("FullComplianceCoa", result.DocumentClassification);
        Assert.True(result.IsFullComplianceCoa);
        Assert.Equal("Bulk Garlic Cookies", result.ProductName);
        Assert.Equal("GC-BULK-0312", result.BatchId);
        Assert.True(result.Cannabinoids.TotalTHC > 0m);
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
    public void RsrAnalyticalAdapter_Parse_TerpeneResultColumns_MapsBreakdownWithoutMismatchWarning()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-terpene-result-columns-spritzer.txt"));

        var result = new RSRAnalyticalAdapter().Parse(text);
        var validation = CoaValidator.Validate(result);
        var terpeneSum = result.Terpenes.Terpenes.Values.Sum();

        Assert.Equal("RSR Analytical Laboratories", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("SPRITZER", result.ProductName);
        Assert.Equal("SP 7925", result.BatchId);
        Assert.Equal(1.724m, result.Terpenes.TotalTerpenes);
        Assert.Equal(8, result.Terpenes.Terpenes.Count);
        Assert.Equal(0.582m, result.Terpenes.Terpenes["δ-Limonene"]);
        Assert.Equal(0.439m, result.Terpenes.Terpenes["β-Caryophyllene"]);
        Assert.Equal(0.310m, result.Terpenes.Terpenes["Linalool"]);
        Assert.Equal(0.140m, result.Terpenes.Terpenes["α-Humulene"]);
        Assert.Equal(0.104m, result.Terpenes.Terpenes["β-Myrcene"]);
        Assert.Equal(0.066m, result.Terpenes.Terpenes["β-Pinene"]);
        Assert.Equal(0.043m, result.Terpenes.Terpenes["α-Pinene"]);
        Assert.Equal(0.040m, result.Terpenes.Terpenes["α-Bisabolol"]);
        Assert.InRange(terpeneSum, result.Terpenes.TotalTerpenes - 0.01m, result.Terpenes.TotalTerpenes + 0.01m);
        Assert.DoesNotContain(validation.Warnings, warning => warning.Code == "TERPENE_TOTAL_MISMATCH");
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
