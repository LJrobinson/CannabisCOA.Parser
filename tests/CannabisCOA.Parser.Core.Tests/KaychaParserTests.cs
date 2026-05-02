using CannabisCOA.Parser.Core.Adapters.Labs.KaychaLabs;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Validation;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class KaychaParserTests
{
    public static IEnumerable<object[]> KaychaFlowerCuredFixtures =>
    [
        ["kaycha-flower-1A40403000004B6000020660.txt", "Kaycha Flower 20660", "KCH-20660"],
        ["kaycha-flower-1A4040300000E11000056638.txt", "Kaycha Flower 56638", "KCH-56638"],
        ["kaycha-flower-1A4040300008856000040815.txt", "Kaycha Flower 40815", "KCH-40815"],
        ["kaycha-flower-1A4040300008856000043658.txt", "Kaycha Flower 43658", "KCH-43658"]
    ];

    public static IEnumerable<object[]> KaychaRawPlantFlowerFixtures =>
    [
        ["kaycha-flower-trim-1A404030000012D000087179.txt", "TRIM - GHOST TRAIN HAZE", "GTH 3426"],
        ["kaycha-flower-popcorn-buds-1A4040300000153000052234.txt", "SHADY APPLES", "SAP.11.06.25"],
        ["kaycha-flower-cured-pave-new-layout.txt", "PAVE", "Pave082625.1.4"],
        ["kaycha-flower-shake-carbon-fiber.txt", "Carbon Fiber", "DDUV 11.23.25 FR1"]
    ];

    public static IEnumerable<object[]> KaychaRemainingFlowerMetadataFixtures =>
    [
        ["kaycha-flower-las-vegas-kush-cake.txt", "Las Vegas Kush Cake", "LVKC 12-29-25 F4 #421"],
        ["kaycha-flower-702-headband-header.txt", "702 Headband", "S2.P29.R2.1201"],
        ["kaycha-flower-production-run-ghost-train-haze.txt", "Featured Farms - Natures Chemistry Ghost Train Haze", "251002GTH-24-IPR"],
        ["kaycha-flower-batch-id-blurazz.txt", "BluRazz - Flower - (A)", "BLURAZZ - 03 MARCH 25 FL-03"],
        ["kaycha-flower-popcorn-old-layout-trop-cherry.txt", "Trop Cherry - Small Buds - (A)", "TROP CHERRY - 11 JUNE 24 FL-01"],
        ["kaycha-flower-other-not-listed-coconut-milk.txt", "Featured Farms - Natures Chemistry Coconut Milk", "250304CM-60-IPR"]
    ];

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
        Assert.Equal("BANANA SMUGGLER", result.ProductName);
        Assert.Equal("BS 21126", result.BatchId);
        Assert.NotNull(result.TestDate);
        Assert.Equal(28.0530m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.6700m, result.Cannabinoids.THC.Value);
        //Assert.True(result.Cannabinoids.CBDA.Value is null || result.Cannabinoids.CBDA.Confidence == 0m);
    }

    [Theory]
    [MemberData(nameof(KaychaFlowerCuredFixtures))]
    public void KaychaAdapter_Parse_FlowerCuredFixturesWithEdiblesInstrumentText_DetectsFlower(
        string fixtureName,
        string expectedProductName,
        string expectedBatchId)
    {
        var text = File.ReadAllText(FixturePath(fixtureName));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.Equal("Kaycha Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal(expectedProductName, result.ProductName);
        Assert.Equal(expectedBatchId, result.BatchId);
        Assert.NotNull(result.TestDate);
        Assert.True(result.Cannabinoids.TotalTHC > 0m);
    }

    [Fact]
    public void KaychaAdapter_Parse_FlowerDisplayedProductBlock_MapsProductName()
    {
        var text = File.ReadAllText(FixturePath("kaycha-flower-displayed-product-royale-grape.txt"));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.Equal("Kaycha Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Royale Grape", result.ProductName);
        Assert.Equal("RG-0420", result.BatchId);
    }

    [Theory]
    [MemberData(nameof(KaychaRawPlantFlowerFixtures))]
    public void KaychaAdapter_Parse_RawPlantFlowerTypeVariants_DetectsFlower(
        string fixtureName,
        string expectedProductName,
        string expectedBatchId)
    {
        var text = File.ReadAllText(FixturePath(fixtureName));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.Equal("Kaycha Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal(expectedProductName, result.ProductName);
        Assert.Equal(expectedBatchId, result.BatchId);
        Assert.NotNull(result.TestDate);
        Assert.True(result.Cannabinoids.TotalTHC > 0m);
    }

    [Theory]
    [MemberData(nameof(KaychaRemainingFlowerMetadataFixtures))]
    public void KaychaAdapter_Parse_RemainingFlowerMetadataLayouts_MapCoreFields(
        string fixtureName,
        string expectedProductName,
        string expectedBatchId)
    {
        var text = File.ReadAllText(FixturePath(fixtureName));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.Equal("Kaycha Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal(expectedProductName, result.ProductName);
        Assert.Equal(expectedBatchId, result.BatchId);
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
    public void KaychaAdapter_Parse_SplitTerpeneTableFixture_MapsBreakdownWithoutWarnings()
    {
        var text = File.ReadAllText(FixturePath("kaycha-flower-split-terpene-table-kush-mints.txt"));

        var result = new KaychaLabsAdapter().Parse(text);
        var validation = CoaValidator.Validate(result);
        var terpeneSum = result.Terpenes.Terpenes.Values.Sum();

        Assert.Equal("Kaycha Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal(0.6572m, result.Terpenes.TotalTerpenes);
        Assert.Equal(7, result.Terpenes.Terpenes.Count);
        Assert.Equal(0.2708m, result.Terpenes.Terpenes["BETA-CARYOPHYLLENE"]);
        Assert.Equal(0.1046m, result.Terpenes.Terpenes["ALPHA-HUMULENE"]);
        Assert.Equal(0.0858m, result.Terpenes.Terpenes["LINALOOL"]);
        Assert.Equal(0.0636m, result.Terpenes.Terpenes["D-LIMONENE"]);
        Assert.Equal(0.0532m, result.Terpenes.Terpenes["BETA-MYRCENE"]);
        Assert.Equal(0.0529m, result.Terpenes.Terpenes["FARNESENE"]);
        Assert.Equal(0.0263m, result.Terpenes.Terpenes["FENCHOL"]);
        Assert.InRange(terpeneSum, result.Terpenes.TotalTerpenes - 0.01m, result.Terpenes.TotalTerpenes + 0.01m);
        Assert.DoesNotContain(validation.Warnings, warning => warning.Code == "TERPENE_BREAKDOWN_MISSING");
        Assert.DoesNotContain(validation.Warnings, warning => warning.Code == "TERPENE_TOTAL_MISMATCH");
    }

    [Fact]
    public void KaychaAdapter_Parse_EdibleFixture_ParsesCannabinoidsFromEdiblePotencyTable()
    {
        var text = File.ReadAllText(FixturePath("kaycha-edible-real-001.txt"));

        var result = new KaychaLabsAdapter().Parse(text);

        Assert.Equal("Kaycha Labs", result.LabName);
        Assert.Equal(ProductType.Edible, result.ProductType);
        Assert.Equal(string.Empty, result.ProductName);
        Assert.Equal(string.Empty, result.BatchId);
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
