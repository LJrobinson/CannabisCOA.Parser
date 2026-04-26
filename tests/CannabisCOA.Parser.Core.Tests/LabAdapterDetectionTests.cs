using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class LabAdapterDetectionTests
{
    [Theory]
    [InlineData("374Labs Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "374Labs")]
    [InlineData("G3 Labs Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "G3Labs")]
    [InlineData("NV CannLabs Certificate of Analysis Product Type: Flower THC: 20% Result: PASS", "NV CannLabs")]
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
}