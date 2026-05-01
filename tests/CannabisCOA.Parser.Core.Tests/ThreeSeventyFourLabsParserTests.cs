using CannabisCOA.Parser.Core.Adapters.Labs.Labs374;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Validation;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class ThreeSeventyFourLabsParserTests
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
    public void ThreeSeventyFourLabsAdapter_Parse_RealFlowerFixtureDetectsHeaderFields()
    {
        var text = File.ReadAllText(FixturePath("374labs-flower-real-001.txt"));

        var result = new Labs374Adapter().Parse(text);

        Assert.Equal("374 Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("702 Headband", result.ProductName);
        Assert.Equal("S2.P32.R1.0223", result.BatchId);
        Assert.NotNull(result.TestDate);
        Assert.Equal(30.01m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.72m, result.Cannabinoids.THC.Value);
        Assert.Equal(0.07m, result.Cannabinoids.CBDA.Value);
    }

    [Fact]
    public void ThreeSeventyFourLabsAdapter_Parse_BatchFlowerFixtureExtractsDisplayedProductAndBatch()
    {
        var text = File.ReadAllText(FixturePath("374labs-flower-batch-garlic-cocktail.txt"));

        var result = new Labs374Adapter().Parse(text);

        Assert.Equal("374 Labs", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Garlic Cocktail", result.ProductName);
        Assert.Equal("LV.103025.R7.GC", result.BatchId);
    }

    [Fact]
    public void ThreeSeventyFourLabsAdapter_Parse_FlowerPlaceholderMetadataStaysEmpty()
    {
        var text = """
        374 Labs
        Strain: Flower
        -
        Plant, Flower - Cured
        Batch #: -; Lot #: 1
        Harvest Process Lot: ; METRC Batch: -; METRC Sample: 1A40403000000C9000013418
        THCa 0.083 22.724 227.24
        """;

        var result = new Labs374Adapter().Parse(text);

        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal(string.Empty, result.ProductName);
        Assert.Equal(string.Empty, result.BatchId);
    }

    [Theory]
    [InlineData("CBD", "CBD 0.05 <LOQ <LOQ")]
    [InlineData("CBDA", "CBDa 0.05 <LOQ <LOQ")]
    [InlineData("CBD", "CBD 0.05 ND ND")]
    public void ThreeSeventyFourLabsAdapter_Parse_CbdNonDetectRows_MapToZeroConfidence(string cannabinoidName, string cannabinoidRow)
    {
        var text = $"""
        374 Labs
        Product Type: Plant, Flower - Cured
        THCa 0.05 30.01 300.1
        Δ9-THC 0.05 0.72 7.2
        {cannabinoidRow}
        """;

        var result = new Labs374Adapter().Parse(text);
        var field = cannabinoidName == "CBDA"
            ? result.Cannabinoids.CBDA
            : result.Cannabinoids.CBD;

        Assert.Equal(0m, field.Value);
        Assert.Equal(0m, field.Confidence);
    }

    [Fact]
    public void ThreeSeventyFourLabsAdapter_Parse_VapeFixture_MapsTerpeneBreakdownFromFlattenedTable()
    {
        var text = File.ReadAllText(FixturePath("374labs-vape-real-001.txt"));

        var result = new Labs374Adapter().Parse(text);
        var validation = CoaValidator.Validate(result);
        var limonene = result.Terpenes.Terpenes.Single(terpene =>
            terpene.Key.Contains("Limonene", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(result.ProductType, new[] { ProductType.Vape, ProductType.Concentrate });
        Assert.True(result.Terpenes.TotalTerpenes > 0m);
        Assert.NotEmpty(result.Terpenes.Terpenes);
        Assert.Equal(0.13m, limonene.Value);
        Assert.DoesNotContain(result.Terpenes.Terpenes, terpene =>
            terpene.Key.Contains("Bisabolol", StringComparison.OrdinalIgnoreCase) &&
            terpene.Value == 0.05m);
        Assert.DoesNotContain(validation.Warnings, warning => warning.Code == "TERPENE_BREAKDOWN_MISSING");
    }
}
