using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Parsers;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class ProductTypeDetectorTests
{
    [Fact]
    public void Detects_Flower()
    {
        var text = "Product Type: Flower";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Flower, result);
    }

    [Fact]
    public void Detects_PreRoll()
    {
        var text = "Infused Pre-Roll 1g";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.PreRoll, result);
    }

    [Fact]
    public void Detects_Vape()
    {
        var text = "Vape Cartridge 0.5g";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Vape, result);
    }

    [Fact]
    public void Detects_Concentrate()
    {
        var text = "Live Resin Concentrate";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Concentrate, result);
    }

    [Fact]
    public void Detects_Edible()
    {
        var text = "THC Gummies 100mg";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Edible, result);
    }

    [Fact]
    public void Detects_Topical()
    {
        var text = "Cannabis Topical Cream";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Topical, result);
    }

    [Fact]
    public void Detects_Tincture()
    {
        var text = "THC Tincture Sublingual Drops";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Tincture, result);
    }

    [Fact]
    public void Returns_Unknown_When_No_Product_Type_Found()
    {
        var text = "Laboratory Certificate of Analysis";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Unknown, result);
    }

    [Fact]
    public void Does_Not_FalsePositive_From_Disclaimer_Text()
    {
        var text = """
        This product was tested using validated laboratory methods.
        The word flower appears in documentation but is not the product type.
        """;

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Unknown, result);
    }

    [Fact]
    public void Prioritizes_PreRoll_Over_Flower()
    {
        var text = "Pre-Roll Flower Product";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.PreRoll, result);
    }

    [Fact]
    public void Prioritizes_Vape_Over_Generic_Words()
    {
        var text = "Premium Vape Cart Product";

        var result = ProductTypeDetector.Detect(text);

        Assert.Equal(ProductType.Vape, result);
    }
}