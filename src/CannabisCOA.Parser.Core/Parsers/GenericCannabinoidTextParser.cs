using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class GenericCannabinoidTextParser
{
    private static readonly Regex NumberRegex = new(
        @"(?<![A-Za-z0-9])(?<value>\d{1,4}(?:\.\d+)?)(?:\s*(?<unit>%|mg/g|mg\/g|mg))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] BlockedRowTerms =
    [
        "MME ID",
        "LICENSE",
        "CERTIFICATE",
        "METHOD",
        "ANALYTE",
        "TOTAL THC =",
        "TOTAL CBD =",
        "THCA *",
        "CBDA *",
        "THC /",
        "CBD /",
        "FORMULA",
        "CALCULATION"
    ];

    public static CannabinoidProfile Parse(string text)
    {
        var profile = new CannabinoidProfile
        {
            THC = Empty("THC"),
            THCA = Empty("THCA"),
            CBD = Empty("CBD"),
            CBDA = Empty("CBDA")
        };

        if (string.IsNullOrWhiteSpace(text))
            return profile;

        var rows = NormalizeRows(text);

        profile.THC = Extract(rows, "THC", ["THC", "Δ9-THC", "DELTA 9 THC", "DELTA-9 THC"]);
        profile.THCA = Extract(rows, "THCA", ["THCA", "THCa", "THC-A", "Δ9-THCA"]);
        profile.CBD = Extract(rows, "CBD", ["CBD"]);
        profile.CBDA = Extract(rows, "CBDA", ["CBDA", "CBDa", "CBD-A"]);

        return profile;
    }

    private static ParsedField<decimal> Extract(IReadOnlyList<string> rows, string fieldName, string[] aliases)
    {
        foreach (var row in rows)
        {
            if (!LooksLikeResultRow(row))
                continue;

            if (!ContainsAlias(row, aliases))
                continue;

            var value = ExtractBestResultValue(row);

            if (value is null)
                continue;

            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = value.Value,
                SourceText = row,
                Confidence = 0.9m
            };
        }

        return Empty(fieldName);
    }

    private static decimal? ExtractBestResultValue(string row)
    {
        var matches = NumberRegex.Matches(row)
            .Cast<Match>()
            .Where(m => m.Success)
            .Select(m => new
            {
                ValueText = m.Groups["value"].Value,
                Unit = m.Groups["unit"].Success ? m.Groups["unit"].Value : string.Empty
            })
            .Where(x => decimal.TryParse(x.ValueText, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            .Select(x =>
            {
                var rawValue = decimal.Parse(x.ValueText, CultureInfo.InvariantCulture);
                var normalizedValue = NormalizeValue(rawValue, x.Unit);

                return new
                {
                    RawValue = rawValue,
                    NormalizedValue = normalizedValue,
                    x.Unit
                };
            })
            .Where(x => x.NormalizedValue >= 0m && x.NormalizedValue <= 100m)
            .ToList();

        if (matches.Count == 0)
            return null;

        var percentValue = matches.LastOrDefault(x => x.Unit == "%");
        if (percentValue is not null)
            return percentValue.NormalizedValue;

        var mgPerGramValue = matches.LastOrDefault(x =>
            x.Unit.Equals("mg/g", StringComparison.OrdinalIgnoreCase) ||
            x.Unit.Equals("mg/g", StringComparison.OrdinalIgnoreCase));

        if (mgPerGramValue is not null)
            return mgPerGramValue.NormalizedValue;

        return matches.Last().NormalizedValue;
    }

    private static decimal NormalizeValue(decimal value, string unit)
    {
        if (unit.Equals("mg/g", StringComparison.OrdinalIgnoreCase) ||
            unit.Equals("mg/g", StringComparison.OrdinalIgnoreCase))
        {
            return value * 0.1m;
        }

        return value;
    }

    private static bool LooksLikeResultRow(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
            return false;

        if (row.Length > 220)
            return false;

        var upper = row.ToUpperInvariant();

        if (BlockedRowTerms.Any(term => upper.Contains(term, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (Regex.IsMatch(upper, @"\b(THCA|CBDA|THC|CBD)\b\s*[\*/=]"))
            return false;

        if (Regex.IsMatch(upper, @"[\*/=]\s*(0\.877|1)\b"))
            return false;

        return true;
    }

    private static bool ContainsAlias(string row, string[] aliases)
    {
        foreach (var alias in aliases)
        {
            var escaped = Regex.Escape(alias);

            if (Regex.IsMatch(row, $@"(?<![A-Za-z0-9]){escaped}(?![A-Za-z0-9])", RegexOptions.IgnoreCase))
                return true;

            if (Regex.IsMatch(row, $@"^{escaped}\s*\d", RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }

    private static List<string> NormalizeRows(string text)
    {
        return text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(row => Regex.Replace(row.Trim(), @"\s+", " "))
            .Where(row => !string.IsNullOrWhiteSpace(row))
            .ToList();
    }

    private static ParsedField<decimal> Empty(string fieldName)
    {
        return new ParsedField<decimal>
        {
            FieldName = fieldName,
            Value = 0m,
            SourceText = string.Empty,
            Confidence = 0m
        };
    }
}