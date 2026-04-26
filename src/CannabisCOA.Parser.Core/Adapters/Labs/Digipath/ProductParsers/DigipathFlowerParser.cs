using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Digipath.ProductParsers;

public static class DigipathFlowerParser
{
    private static readonly Dictionary<string, string[]> CannabinoidAliases = new()
    {
        ["THC"] = ["THC", "Δ9-THC", "DELTA-9 THC", "DELTA 9 THC", "D9-THC"],
        ["THCA"] = ["THCA", "THCa", "THC-A", "Δ9-THCA"],
        ["CBD"] = ["CBD"],
        ["CBDA"] = ["CBDA", "CBDa", "CBD-A"]
    };

    private static readonly string[] BlockedCannabinoidRowTerms =
    [
        "MME ID",
        "METRC",
        "LICENSE",
        "CERTIFICATE",
        "BATCH",
        "LOT",
        "FORMULA",
        "CALCULATION",
        "TOTAL THC =",
        "TOTAL CBD =",
        "TOTAL POTENTIAL THC",
        "TOTAL POTENTIAL CBD",
        "THCA *",
        "CBDA *",
        "THC /",
        "CBD /",
        "* 0.877",
        "/ 1"
    ];

    private static readonly Regex ResultTokenRegex = new(
        @"<\s*LOQ|ND|NR|(?<prefix><)?\s*(?<value>\d{1,4}(?:\.\d+)?|\.\d+)\s*(?<unit>%|mg\s*/\s*g|mg/g|mg\/g)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static CoaResult Parse(string text, string labName)
    {
        var productType = ProductTypeDetector.Detect(text);

        var cannabinoids = ParseDigipathCannabinoidsOrFallback(text);
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

    private static CannabinoidProfile ParseDigipathCannabinoidsOrFallback(string text)
    {
        var sectionRows = ExtractCannabinoidSectionRows(text);

        if (sectionRows.Count == 0)
            return GenericCannabinoidTextParser.Parse(text);

        var profile = ParseCannabinoidSection(sectionRows);

        if (!HasAnyParsedCannabinoid(profile))
            return GenericCannabinoidTextParser.Parse(text);

        return profile;
    }

    private static CannabinoidProfile ParseCannabinoidSection(IReadOnlyList<string> rows)
    {
        var context = DetectTableContext(rows);

        var profile = new CannabinoidProfile
        {
            THC = Extract(rows, "THC", context),
            THCA = Extract(rows, "THCA", context),
            CBD = Extract(rows, "CBD", context),
            CBDA = Extract(rows, "CBDA", context)
        };

        return profile;
    }

    private static ParsedField<decimal> Extract(
        IReadOnlyList<string> rows,
        string fieldName,
        DigipathTableContext context)
    {
        foreach (var row in rows)
        {
            if (!LooksLikeSafeCannabinoidRow(row))
                continue;

            var alias = FindLeadingAlias(row, fieldName);

            if (alias == null)
                continue;

            var parsedValue = ExtractResultValue(row, alias, context);

            if (parsedValue == null)
                continue;

            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = parsedValue.Value,
                SourceText = row,
                Confidence = parsedValue.Confidence
            };
        }

        return Empty(fieldName);
    }

    private static ParsedDigipathValue? ExtractResultValue(
        string row,
        string alias,
        DigipathTableContext context)
    {
        var aliasIndex = row.IndexOf(alias, StringComparison.OrdinalIgnoreCase);

        if (aliasIndex < 0)
            return null;

        var afterAlias = row[(aliasIndex + alias.Length)..];

        if (Regex.IsMatch(afterAlias, @"^\s*[\*/=]"))
            return null;

        var tokens = ResultTokenRegex.Matches(afterAlias)
            .Cast<Match>()
            .Select(ToToken)
            .Where(token => token != null)
            .Select(token => token!)
            .ToList();

        if (tokens.Count == 0)
            return null;

        var token = SelectResultToken(tokens, context);

        if (token == null)
            return null;

        if (token.IsNonDetect || token.IsLessThan)
            return new ParsedDigipathValue(0m, 0.85m);

        var unit = string.IsNullOrWhiteSpace(token.Unit)
            ? context.PrimaryResultUnit
            : token.Unit;

        var value = token.Value;

        if (unit == "MG/G")
            value *= 0.1m;

        if (value < 0m || value > 100m)
            return null;

        var confidence = unit == "MG/G" ? 0.9m : 0.95m;

        return new ParsedDigipathValue(value, confidence);
    }

    private static DigipathResultToken? SelectResultToken(
        IReadOnlyList<DigipathResultToken> tokens,
        DigipathTableContext context)
    {
        if (context.HasLoqColumn && tokens.Count >= 2)
            return tokens[1];

        var explicitPercent = tokens.LastOrDefault(token => token.Unit == "%");
        if (explicitPercent != null)
            return explicitPercent;

        var explicitMgPerGram = tokens.LastOrDefault(token => token.Unit == "MG/G");
        if (explicitMgPerGram != null)
            return explicitMgPerGram;

        return tokens.LastOrDefault();
    }

    private static DigipathResultToken? ToToken(Match match)
    {
        var raw = Regex.Replace(match.Value.Trim(), @"\s+", " ");

        if (raw.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
            raw.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
            Regex.IsMatch(raw, @"^<\s*LOQ$", RegexOptions.IgnoreCase))
        {
            return new DigipathResultToken(0m, string.Empty, IsLessThan: false, IsNonDetect: true);
        }

        if (!decimal.TryParse(
            match.Groups["value"].Value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var value))
        {
            return null;
        }

        var unit = NormalizeUnit(match.Groups["unit"].Value);
        var isLessThan = match.Groups["prefix"].Success;

        return new DigipathResultToken(value, unit, isLessThan, IsNonDetect: false);
    }

    private static DigipathTableContext DetectTableContext(IReadOnlyList<string> rows)
    {
        var hasLoqColumn = rows.Any(row =>
            row.Contains("ANALYTE", StringComparison.OrdinalIgnoreCase) &&
            row.Contains("LOQ", StringComparison.OrdinalIgnoreCase));

        foreach (var row in rows)
        {
            var upper = row.ToUpperInvariant();

            if (!upper.Contains("%") && !ContainsMgPerGram(upper))
                continue;

            var units = Regex.Matches(upper, @"%|MG\s*/\s*G|MG/G")
                .Cast<Match>()
                .Select(match => NormalizeUnit(match.Value))
                .ToList();

            if (hasLoqColumn && units.Count >= 2)
                return new DigipathTableContext(hasLoqColumn, units[1]);

            if (units.Count >= 1)
                return new DigipathTableContext(hasLoqColumn, units[0]);
        }

        return new DigipathTableContext(hasLoqColumn, string.Empty);
    }

    private static List<string> ExtractCannabinoidSectionRows(string text)
    {
        var rows = NormalizeRows(text);
        var startIndex = rows.FindIndex(IsCannabinoidSectionStart);

        if (startIndex < 0)
            return [];

        var sectionRows = new List<string>();

        for (var i = startIndex + 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (IsCannabinoidSectionEnd(row))
                break;

            sectionRows.Add(row);
        }

        return sectionRows;
    }

    private static bool IsCannabinoidSectionStart(string row)
    {
        return row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCannabinoidSectionEnd(string row)
    {
        return row.Contains("Total Potential THC", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Total Potential CBD", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Safety & Quality Tests", StringComparison.OrdinalIgnoreCase) ||
               row.Equals("Safety", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeSafeCannabinoidRow(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
            return false;

        if (row.Length > 180)
            return false;

        var upper = row.ToUpperInvariant();

        if (BlockedCannabinoidRowTerms.Any(term => upper.Contains(term.ToUpperInvariant())))
            return false;

        if (Regex.IsMatch(upper, @"\b(MME|ID|LICENSE|CERT|BATCH|LOT)\b.*\d{6,}"))
            return false;

        if (Regex.IsMatch(upper, @"\d{7,}"))
            return false;

        return true;
    }

    private static string? FindLeadingAlias(string row, string fieldName)
    {
        foreach (var alias in CannabinoidAliases[fieldName])
        {
            var escaped = Regex.Escape(alias);

            if (Regex.IsMatch(row, $@"^\s*{escaped}(?=\s|:|$)", RegexOptions.IgnoreCase))
                return alias;
        }

        return null;
    }

    private static bool HasAnyParsedCannabinoid(CannabinoidProfile profile)
    {
        return profile.THC.Confidence > 0m ||
               profile.THCA.Confidence > 0m ||
               profile.CBD.Confidence > 0m ||
               profile.CBDA.Confidence > 0m;
    }

    private static string NormalizeUnit(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            return string.Empty;

        var normalized = unit.ToUpperInvariant().Replace(" ", "");

        return normalized switch
        {
            "MG/G" => "MG/G",
            "%" => "%",
            _ => normalized
        };
    }

    private static bool ContainsMgPerGram(string text)
    {
        return Regex.IsMatch(text, @"MG\s*/\s*G", RegexOptions.IgnoreCase)
            || text.Contains("MG/G", StringComparison.OrdinalIgnoreCase)
            || text.Contains("MG PER G", StringComparison.OrdinalIgnoreCase);
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

    private sealed record DigipathTableContext(bool HasLoqColumn, string PrimaryResultUnit);

    private sealed record DigipathResultToken(
        decimal Value,
        string Unit,
        bool IsLessThan,
        bool IsNonDetect);

    private sealed record ParsedDigipathValue(decimal Value, decimal Confidence);
}
