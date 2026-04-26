using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class GenericCannabinoidTextParser
{
    private static readonly Dictionary<string, string[]> Aliases = new()
    {
        ["THC"] = ["THC", "Δ9-THC", "DELTA-9 THC", "DELTA 9 THC", "D9-THC"],
        ["THCA"] = ["THCA", "THCa", "THC-A", "Δ9-THCA"],
        ["CBD"] = ["CBD"],
        ["CBDA"] = ["CBDA", "CBDa", "CBD-A"]
    };

    private static readonly string[] BlockedTerms =
    [
        "MME ID",
        "LICENSE",
        "CERTIFICATE",
        "METHOD",
        "FORMULA",
        "CALCULATION",
        "TOTAL THC =",
        "TOTAL CBD =",
        "THCA *",
        "CBDA *",
        "THC /",
        "CBD /",
        "* 0.877",
        "/ 1"
    ];

    private static readonly Regex NumberRegex = new(
        @"(?<prefix><)?\s*(?<value>\d{1,3}(?:\.\d+)?|\.\d+)\s*(?<unit>%|mg\s*/\s*g|mg/g|mg\/g)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

        profile.THC = Extract(rows, "THC");
        profile.THCA = Extract(rows, "THCA");
        profile.CBD = Extract(rows, "CBD");
        profile.CBDA = Extract(rows, "CBDA");

        return profile;
    }

    private static ParsedField<decimal> Extract(IReadOnlyList<string> rows, string fieldName)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];

            if (!LooksLikeSafeResultRow(row))
                continue;

            var alias = FindMatchingAlias(row, fieldName);

            if (alias == null)
                continue;

            var unitContext = DetectNearbyUnitContext(rows, i);
            var value = ExtractBestValueAfterAlias(row, alias, unitContext);

            if (value is null)
                continue;

            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = value.Value,
                SourceText = row,
                Confidence = unitContext == "MG/G" ? 0.85m : 0.9m
            };
        }

        return Empty(fieldName);
    }

    private static decimal? ExtractBestValueAfterAlias(string row, string alias, string unitContext)
    {
        var aliasIndex = row.IndexOf(alias, StringComparison.OrdinalIgnoreCase);

        if (aliasIndex < 0)
            return null;

        var afterAlias = row[(aliasIndex + alias.Length)..];

        if (Regex.IsMatch(afterAlias, @"^\s*[\*/=]"))
            return null;

        var candidates = NumberRegex.Matches(afterAlias)
            .Cast<Match>()
            .Where(m => m.Success)
            .Select(m => ToCandidate(m, unitContext))
            .Where(c => c != null)
            .Select(c => c!)
            .Where(c => c.Value >= 0m && c.Value <= 100m)
            .ToList();

        if (candidates.Count == 0)
            return null;

        var explicitPercent = candidates.LastOrDefault(c => c.Unit == "%");
        if (explicitPercent != null)
            return explicitPercent.Value;

        var explicitMgPerGram = candidates.LastOrDefault(c => c.Unit == "MG/G");
        if (explicitMgPerGram != null)
            return explicitMgPerGram.Value;

        return candidates.Last().Value;
    }

    private static Candidate? ToCandidate(Match match, string unitContext)
    {
        if (!decimal.TryParse(match.Groups["value"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return null;

        if (match.Groups["prefix"].Success)
            value = 0m;

        var unit = NormalizeUnit(match.Groups["unit"].Value);

        if (string.IsNullOrWhiteSpace(unit))
            unit = unitContext;

        if (unit == "MG/G")
            value *= 0.1m;

        return new Candidate(value, unit);
    }

    private static string DetectNearbyUnitContext(IReadOnlyList<string> rows, int currentIndex)
    {
        var start = Math.Max(0, currentIndex - 8);
        var end = Math.Min(rows.Count - 1, currentIndex + 1);

        for (var i = currentIndex; i >= start; i--)
        {
            var upper = rows[i].ToUpperInvariant();

            if (ContainsMgPerGram(upper))
                return "MG/G";

            if (upper.Contains("%"))
                return "%";
        }

        for (var i = currentIndex + 1; i <= end; i++)
        {
            var upper = rows[i].ToUpperInvariant();

            if (ContainsMgPerGram(upper))
                return "MG/G";

            if (upper.Contains("%"))
                return "%";
        }

        return string.Empty;
    }

    private static bool ContainsMgPerGram(string text)
    {
        return Regex.IsMatch(text, @"MG\s*/\s*G", RegexOptions.IgnoreCase)
            || text.Contains("MG/G", StringComparison.OrdinalIgnoreCase)
            || text.Contains("MG PER G", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeUnit(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            return string.Empty;

        var normalized = unit
            .ToUpperInvariant()
            .Replace(" ", "");

        return normalized switch
        {
            "MG/G" => "MG/G",
            "%" => "%",
            _ => normalized
        };
    }

    private static bool LooksLikeSafeResultRow(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
            return false;

        if (row.Length > 220)
            return false;

        var upper = row.ToUpperInvariant();

        if (BlockedTerms.Any(term => upper.Contains(term.ToUpperInvariant())))
            return false;

        if (Regex.IsMatch(upper, @"\b(MME|ID|LICENSE|CERT|BATCH|LOT)\b.*\d{6,}"))
            return false;

        if (Regex.IsMatch(upper, @"\d{7,}"))
            return false;

        return true;
    }

    private static string? FindMatchingAlias(string row, string fieldName)
    {
        foreach (var alias in Aliases[fieldName])
        {
            var escaped = Regex.Escape(alias);

            if (Regex.IsMatch(row, $@"(?<![A-Za-z0-9]){escaped}(?![A-Za-z0-9])", RegexOptions.IgnoreCase))
                return alias;
        }

        return null;
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

    private sealed record Candidate(decimal Value, string Unit);
}