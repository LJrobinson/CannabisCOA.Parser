using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Digipath.ProductParsers;

public static class DigipathFlowerParser
{
    public static CoaResult Parse(string text, string labName)
    {
        var productType = ProductTypeDetector.Detect(text);

        var cannabinoids = GenericCannabinoidTextParser.Parse(text);
        CannabinoidCalculator.CalculateTotals(cannabinoids);

        var testDate = GenericDateParser.ExtractTestDate(text);
        var harvestDate = GenericDateParser.ExtractHarvestDate(text);
        var packageDate = GenericDateParser.ExtractPackageDate(text);

        var freshness = FreshnessCalculator.Calculate(testDate);
        var compliance = ComplianceParser.Parse(text);
        var terpenes = GenericTerpeneTextParser.Parse(text);

        return new CoaResult
        {
            ProductType = productType,
            IsAmended = CoaMetadataParser.IsAmended(text),
            LabName = labName,
            ProductName = string.Empty,
            BatchId = string.Empty,
            HarvestDate = harvestDate,
            TestDate = testDate,
            PackageDate = packageDate,
            Cannabinoids = cannabinoids,
            Terpenes = terpenes,
            Compliance = compliance,
            Freshness = freshness
        };
    }
}