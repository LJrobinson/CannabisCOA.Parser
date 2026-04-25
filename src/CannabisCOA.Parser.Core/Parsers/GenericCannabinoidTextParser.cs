using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class GenericCannabinoidTextParser
{
    public static CannabinoidProfile Parse(string text)
    {
        return new CannabinoidProfile
        {
            THC = ExtractField(text, @"THC\s*[:\-]?\s*(\d+\.?\d*)", "THC"),
            THCA = ExtractField(text, @"THCA\s*[:\-]?\s*(\d+\.?\d*)", "THCA"),
            CBD = ExtractField(text, @"CBD\s*[:\-]?\s*(\d+\.?\d*)", "CBD"),
            CBDA = ExtractField(text, @"CBDA\s*[:\-]?\s*(\d+\.?\d*)", "CBDA")
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
                Confidence = 0m,
                SourceText = ""
            };
        }

        if (!decimal.TryParse(match.Groups[1].Value, out var value))
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
            Confidence = 0.9m,
            SourceText = match.Value
        };
    }
}