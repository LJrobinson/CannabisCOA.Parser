using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class LabAdapterDetectionTests
{
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
}
