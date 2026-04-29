using CannabisCOA.Parser.Core.Adapters.Labs.Labs374;
using CannabisCOA.Parser.Core.Enums;
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
        Assert.NotNull(result.TestDate);
        Assert.Equal(30.01m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.72m, result.Cannabinoids.THC.Value);
        Assert.Equal(0.07m, result.Cannabinoids.CBDA.Value);
    }
}
