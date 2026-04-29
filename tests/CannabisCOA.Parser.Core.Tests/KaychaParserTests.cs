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
}
