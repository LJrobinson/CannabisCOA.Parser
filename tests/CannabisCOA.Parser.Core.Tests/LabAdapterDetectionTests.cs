using CannabisCOA.Parser.Core.Enums;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class LabAdapterDetectionTests
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

    [Theory]
    [InlineData("374Labs Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "374 Labs")]
    [InlineData("G3 Labs Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "G3 Labs")]
    [InlineData("NV CannLabs Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "NV Cann Labs")]
    [InlineData("Ace Analytical Laboratory Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "Ace Analytical Laboratory")]
    [InlineData("Kaycha Labs Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "Kaycha Labs")]
    [InlineData("Digipath Labs Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "Digipath")]
    [InlineData("MA Analytics Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "MA Analytics")]
    [InlineData("RSR Analytical Laboratories Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "RSR Analytical Laboratories")]
    public void Detects_Known_Labs(string text, string expectedLab)
    {
        var result = CoaParser.Parse(text);

        Assert.Equal(expectedLab, result.LabName);
    }

    [Fact]
    public void Resolves_Digipath_When_DigipathBodyContains_NvCannLabsFooterText()
    {
        var text = """
        Digipath Labs
        Certificate of Analysis
        Concentrates & Extracts, Formulated Vape Oil
        Cannabinoid Test Results Terpene Test Results
        Analyte LOQ Mass Mass Analyte CAS No. LOQ Mass Mass
        mg/g % % mg/g
        Δ9-THC 0.0030 749.470 74.9470 Terpinolene 586-62-9 0.006 0.023 0.23
        All analyses were performed at NV Cann Labs unless otherwise stated.
        """;

        var result = CoaParser.Parse(text);

        Assert.Equal("Digipath", result.LabName);
    }

    [Fact]
    public void Resolves_MaAnalytics_When_MicrobialMethodsReferenceG3Sops()
    {
        var text = File.ReadAllText(FixturePath("ma-flower-g3-method-footer-ice-cream-mintz.txt"));

        var result = CoaParser.Parse(text);

        Assert.Equal("MA Analytics", result.LabName);
        Assert.NotEqual("G3 Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Ice Cream Mintz", result.ProductName);
        Assert.Equal("08020725ICM", result.BatchId);
    }

    [Fact]
    public void Resolves_G3Fixture_AsG3Labs()
    {
        var text = File.ReadAllText(FixturePath("g3-flower-real-001.txt"));

        var result = CoaParser.Parse(text);

        Assert.Equal("G3 Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
    }

    [Fact]
    public void Resolves_Rsr_When_HeaderNoteReferencesDigipathTesting()
    {
        var text = File.ReadAllText(FixturePath("rsr-flower-digipath-note-mai-tai.txt"));

        var result = CoaParser.Parse(text);

        Assert.Equal("RSR Analytical Laboratories", result.LabName);
        Assert.NotEqual("Digipath", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Mai Tai", result.ProductName);
        Assert.Equal("08121925MT", result.BatchId);
    }

    [Fact]
    public void Resolves_DigipathFixture_AsDigipath()
    {
        var text = File.ReadAllText(FixturePath("Digipath_Flower.txt"));

        var result = CoaParser.Parse(text);

        Assert.Equal("Digipath", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
    }
}
