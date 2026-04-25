using CannabisCOA.Parser.Core.Adapters.Interfaces;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Generic;

public class GenericCoaAdapter : ICoaAdapter
{
    public string LabName => "Generic";

    public bool CanParse(string text) => true; // always fallback

    public CoaResult Parse(string text)
    {
        var cannabinoids = GenericCannabinoidTextParser.Parse(text);
        CannabinoidCalculator.CalculateTotals(cannabinoids);

        var testDate = GenericDateParser.ExtractTestDate(text);
        var freshness = FreshnessCalculator.Calculate(testDate);

        var compliance = ComplianceParser.Parse(text);

        return new CoaResult
        {
            LabName = LabName,
            Cannabinoids = cannabinoids,
            TestDate = testDate,
            Freshness = freshness,
            Compliance = compliance
        };
    }
}