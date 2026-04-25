using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Digipath.ProductParsers;

public static class DigipathFlowerParser
{
        public static CoaResult Parse(string text, string labName)
        {
            var cannabinoids = new CannabinoidProfile
            {
                THC = ExtractField(text, @"(THC|Δ9-THC|DELTA-9 THC)[^\d]*(\d+\.?\d*)", "THC"),
                THCA = ExtractField(text, @"(THCA|THC-A|Δ9-THCA)[^\d]*(\d+\.?\d*)", "THCA"),
                CBD = ExtractField(text, @"(CBD)[^\d]*(\d+\.?\d*)", "CBD"),
                CBDA = ExtractField(text, @"(CBDA|CBD-A)[^\d]*(\d+\.?\d*)", "CBDA")
            };

        CannabinoidCalculator.CalculateTotals(cannabinoids);;

        var testDate = GenericDateParser.ExtractTestDate(text);
        var freshness = FreshnessCalculator.Calculate(testDate);

        var compliance = ComplianceParser.Parse(text);
        var terpenes = GenericTerpeneTextParser.Parse(text);

        return new CoaResult
        {
            LabName = labName,
            Cannabinoids = cannabinoids,
            Terpenes = terpenes,
            TestDate = testDate,
            Freshness = freshness,
            Compliance = compliance
        };
    }

    private static ParsedField<decimal> ExtractField(string text, string pattern, string fieldName)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = 0m,
                Confidence = 0,
                SourceText = ""
            };
        }

        if (!decimal.TryParse(match.Groups[2].Value, out var value))
        {
            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = 0m,
                Confidence = 0.2m,
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