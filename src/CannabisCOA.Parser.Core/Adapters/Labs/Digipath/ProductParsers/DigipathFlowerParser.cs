using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Digipath.ProductParsers;

public static class DigipathFlowerParser
{
    public static CoaResult Parse(string text, string labName)
    {
        var productType = ProductTypeDetector.Detect(text);

        var cannabinoids = new CannabinoidProfile
        {
            THC = ExtractField(text, @"\b(THC|Δ9-THC|DELTA-9 THC)\b", "THC"),
            THCA = ExtractField(text, @"\b(THCA|THC-A|THCa)\b", "THCA"),
            CBD = ExtractField(text, @"\b(CBD)\b", "CBD"),
            CBDA = ExtractField(text, @"\b(CBDA|CBD-A|CBDa)\b", "CBDA")
        };

        CannabinoidCalculator.CalculateTotals(cannabinoids);

        var testDate = GenericDateParser.ExtractTestDate(text);
        var freshness = FreshnessCalculator.Calculate(testDate);
        var compliance = ComplianceParser.Parse(text);
        var terpenes = GenericTerpeneTextParser.Parse(text);

        return new CoaResult
        {
            LabName = labName,
            ProductType = productType,
            Cannabinoids = cannabinoids,
            Terpenes = terpenes,
            TestDate = testDate,
            Freshness = freshness,
            Compliance = compliance
        };
    }

    private static ParsedField<decimal> ExtractField(string text, string labelPattern, string fieldName)
    {
        var pattern = $@"{labelPattern}[^\dA-Za-z]{{0,30}}(\d+\.\d+|\d+)";

        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = 0m,
                Confidence = 0m,
                SourceText = ""
            };
        }

        if (!decimal.TryParse(match.Groups[match.Groups.Count - 1].Value, out var value))
        {
            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = 0m,
                Confidence = 0.2m,
                SourceText = match.Value
            };
        }

        if (value > 100m)
        {
            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = 0m,
                Confidence = 0m,
                SourceText = match.Value
            };
        }

        return new ParsedField<decimal>
        {
            FieldName = fieldName,
            Value = value,
            Confidence = 0.95m,
            SourceText = match.Value
        };
    }
}