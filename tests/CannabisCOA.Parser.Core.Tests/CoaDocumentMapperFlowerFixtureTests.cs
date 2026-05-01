using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Mappers;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaDocumentMapperFlowerFixtureTests
{
    public static IEnumerable<object[]> TargetFlowerFixtures =>
    [
        ["374labs-flower-real-001.txt"],
        ["g3-flower-real-001.txt"],
        ["nvcannlabs-flower-real-001.txt"],
        ["ace-flower-real-001.txt"],
        ["kaycha-flower-real-001.txt"],
        ["Digipath_Flower.txt"],
        ["ma-flower-real-001.txt"],
        ["rsr-flower-real-001.txt"]
    ];

    [Theory]
    [MemberData(nameof(TargetFlowerFixtures))]
    public void FromCoaResult_MapsTargetFlowerFixture_ToFlowerCoaV1Document(string fixtureName)
    {
        var text = File.ReadAllText(FixturePath(fixtureName));

        var result = CoaParser.Parse(text);
        var document = CoaDocumentMapper.FromCoaResult(
            result,
            sourceFileName: fixtureName,
            parserName: nameof(CoaDocumentMapper));

        Assert.Equal("flower-coa-v1", document.SchemaVersion);

        Assert.False(string.IsNullOrWhiteSpace(document.LabName));
        Assert.Equal(result.LabName, document.LabName);

        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Flower", document.ProductType);

        Assert.NotNull(result.TestDate);
        Assert.Equal(result.TestDate, document.TestDate);

        Assert.NotEmpty(document.Cannabinoids);
        Assert.Contains(document.Cannabinoids, cannabinoid => cannabinoid.NormalizedName == "THCA");
        Assert.Contains(document.Cannabinoids, cannabinoid => cannabinoid.NormalizedName == "THC");

        Assert.NotNull(document.TotalThcPercent);
        Assert.Equal(result.Cannabinoids.TotalTHC, document.TotalThcPercent);
        Assert.True(document.TotalThcPercent > 0m);

        Assert.NotNull(document.ParserMetadata);
        Assert.Equal(fixtureName, document.ParserMetadata.SourceFileName);
        Assert.Equal(result.LabName, document.ParserMetadata.DetectedLab);
        Assert.Equal(nameof(CoaDocumentMapper), document.ParserMetadata.ParserName);
        Assert.True(document.ParserMetadata.ConfidenceScore > 0m);
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
}
