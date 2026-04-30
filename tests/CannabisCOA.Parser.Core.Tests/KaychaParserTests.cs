using CannabisCOA.Parser.Core.Adapters.Labs.KaychaLabs;
using CannabisCOA.Parser.Core.Enums;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class KaychaParserTests
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
    public void KaychaAdapter_Parse_RealFlowerFixtureDetectsHeaderFields()
    {
        var text = File.ReadAllText(FixturePath("kaycha-flower-real-001.txt"));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.Equal("Kaycha Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.NotNull(result.TestDate);
        Assert.Equal(28.0530m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.6700m, result.Cannabinoids.THC.Value);
        //Assert.True(result.Cannabinoids.CBDA.Value is null || result.Cannabinoids.CBDA.Confidence == 0m);
    }

    [Fact]
    public void KaychaAdapter_Parse_RealFlowerFixture_TotalThcMatchesFormulaWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("kaycha-flower-real-001.txt"));

        var result = new KaychaLabsAdapter().Parse(text);
        var thca = result.Cannabinoids.THCA.Value;
        var thc = result.Cannabinoids.THC.Value;
        var delta8 = 0m;
        var expectedTotalThc = (thca * 0.877m) + thc + delta8;

        Assert.InRange(result.Cannabinoids.TotalTHC, expectedTotalThc - 0.02m, expectedTotalThc + 0.02m);
        Assert.Equal(25.2725m, Math.Round(result.Cannabinoids.TotalTHC, 4));
    }

    [Fact]
    public void KaychaAdapter_Parse_RealFlowerFixture_MapsTotalTerpenes()
    {
        var text = File.ReadAllText(FixturePath("kaycha-flower-real-001.txt"));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.NotNull(result.Terpenes);
        Assert.Equal(2.3252m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void KaychaAdapter_Parse_RealFlowerFixture_MapsExpectedIndividualTerpenes()
    {
        var text = File.ReadAllText(FixturePath("kaycha-flower-real-001.txt"));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.Equal(1.0790m, result.Terpenes.Terpenes["BETA-MYRCENE"]);
        Assert.Equal(0.3955m, result.Terpenes.Terpenes["BETA-CARYOPHYLLENE"]);
        Assert.Equal(0.2500m, result.Terpenes.Terpenes["D-LIMONENE"]);
        Assert.Equal(0.1413m, result.Terpenes.Terpenes["ALPHA-HUMULENE"]);
        Assert.Equal(0.0692m, result.Terpenes.Terpenes["BETA-PINENE"]);
    }

    [Fact]
    public void KaychaAdapter_Parse_RealFlowerFixture_TerpeneTotalMatchesSumWithinTolerance()
    {
        var text = File.ReadAllText(FixturePath("kaycha-flower-real-001.txt"));

        var result = new KaychaLabsAdapter().Parse(text);
        var terpeneSum = result.Terpenes.Terpenes.Values
            .Where(percent => percent > 0m)
            .Sum();

        Assert.InRange(terpeneSum, result.Terpenes.TotalTerpenes - 0.1m, result.Terpenes.TotalTerpenes + 0.1m);
        Assert.Equal(2.3252m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void KaychaAdapter_Parse_EdibleFixture_ParsesCannabinoidsFromEdiblePotencyTable()
    {
        var text = File.ReadAllText(FixturePath("kaycha-edible-real-001.txt"));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.Equal(ProductType.Edible, result.ProductType);
        Assert.Equal(103.6212m, result.Cannabinoids.THC.Value);
        Assert.True(result.Cannabinoids.THC.Confidence > 0m);
        Assert.Equal(0m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0m, result.Cannabinoids.THCA.Confidence);
        Assert.Equal(0m, result.Cannabinoids.CBD.Value);
        Assert.Equal(0m, result.Cannabinoids.CBD.Confidence);
        Assert.Equal(0m, result.Cannabinoids.CBDA.Value);
        Assert.Equal(0m, result.Cannabinoids.CBDA.Confidence);
        Assert.Equal(103.6212m, result.Cannabinoids.TotalTHC);
        Assert.Equal(0m, result.Cannabinoids.TotalCBD);
    }

    [Fact]
    public void KaychaAdapter_Parse_EdibleFixture_DoesNotUseProductDescriptionOrWaterActivityAsCannabinoidSource()
    {
        var text = File.ReadAllText(FixturePath("kaycha-edible-real-001.txt"));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.True(
            result.Cannabinoids.THC.SourceText.Contains("Δ9-THC") ||
            result.Cannabinoids.THC.SourceText.Contains("∆9-THC"));
        Assert.DoesNotContain("Strain:", result.Cannabinoids.THC.SourceText);
        Assert.DoesNotContain("Gummy", result.Cannabinoids.THC.SourceText);
        Assert.DoesNotContain("Aw:", result.Cannabinoids.CBD.SourceText);
        Assert.DoesNotContain("Water Activity", result.Cannabinoids.CBD.SourceText);
        Assert.DoesNotContain("∆9-THC + ∆8-THC", result.Cannabinoids.CBD.SourceText);
    }
}
