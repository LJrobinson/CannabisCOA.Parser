using CannabisCOA.Parser.Core.Adapters.Interfaces;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Generic;

public class GenericCoaAdapter : ICoaAdapter
{
    public string LabName => "Unknown";

    public bool CanParse(string text)
    {
        return true;
    }

    public ProductType DetectProductType(string text)
    {
        return ProductTypeDetector.Detect(text);
    }

    public CoaResult Parse(string text)
    {
        var cannabinoids = GenericCannabinoidTextParser.Parse(text);
        CannabinoidCalculator.CalculateTotals(cannabinoids);

        var testDate = GenericDateParser.ExtractTestDate(text);
        var harvestDate = GenericDateParser.ExtractHarvestDate(text);
        var packageDate = GenericDateParser.ExtractPackageDate(text);

        return new CoaResult
        {
            ProductType = DetectProductType(text),
            IsAmended = CoaMetadataParser.IsAmended(text),
            LabName = LabName,
            ProductName = string.Empty,
            BatchId = string.Empty,
            HarvestDate = harvestDate,
            TestDate = testDate,
            PackageDate = packageDate,
            Cannabinoids = cannabinoids,
            Terpenes = GenericTerpeneTextParser.Parse(text),
            Compliance = ComplianceParser.Parse(text),
            Freshness = FreshnessCalculator.Calculate(testDate)
        };
    }
}