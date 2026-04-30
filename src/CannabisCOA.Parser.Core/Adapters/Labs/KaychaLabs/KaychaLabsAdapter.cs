using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.KaychaLabs;

public class KaychaLabsAdapter : BaseLabAdapter
{
    public override string LabName => "Kaycha Labs";

    private static readonly Regex TotalTerpenesRowRegex = new(
        @"^\s*TOTAL\s+TERPENES\s+(?<lod>\d{1,6}(?:\.\d+)?)\s+(?<loq>\d{1,6}(?:\.\d+)?)\s+\S+\s+(?<percent>\d{1,6}(?:\.\d+)?)\s+(?<mg>\d{1,6}(?:\.\d+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TerpeneRowRegex = new(
        @"^\s*(?<name>[A-Z0-9]+(?:-[A-Z0-9]+)*(?:\s+[A-Z0-9]+(?:-[A-Z0-9]+)*)*)\s+(?<lod>\d{1,6}(?:\.\d+)?)\s+(?<loq>\d{1,6}(?:\.\d+)?)\s+\S+\s+(?<percent><\s*LOQ|\d{1,6}(?:\.\d+)?)\s+(?<mg><\s*\d{1,6}(?:\.\d+)?|\d{1,6}(?:\.\d+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EdibleCannabinoidRowRegex = new(
        @"^\s*(?<name>CBDa|CBDA|CBD|THCa|THCA|Δ9-THC|∆9-THC|D9-THC|Δ8-THC|∆8-THC|D8-THC)\s+(?<loq><\s*LOQ|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?)\s+(?<mass><\s*LOQ|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?)\s+(?<mgg><\s*LOQ|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    protected override string[] DetectionTerms =>
    [
        "KAYCHA",
        "KAYCHA LABS",
        "KAYCHA LABORATORIES"
    ];

    public override CoaResult Parse(string text)
    {
        var result = base.Parse(text);

        if (TryParseKaychaCannabinoids(text, out var cannabinoids))
        {
            CannabinoidCalculator.CalculateTotals(cannabinoids);
            result.Cannabinoids = cannabinoids;
        }
        else if (TryParseKaychaEdibleCannabinoids(text, out cannabinoids))
        {
            result.Cannabinoids = cannabinoids;
        }

        if (TryParseKaychaTotalTerpenes(text, out var totalTerpenes))
            result.Terpenes.TotalTerpenes = totalTerpenes;

        if (TryParseKaychaTerpenes(text, out var terpenes))
        {
            result.Terpenes.Terpenes.Clear();

            foreach (var terpene in terpenes)
                result.Terpenes.Terpenes[terpene.Key] = terpene.Value;
        }

        return result;
    }

    private static bool TryParseKaychaCannabinoids(string text, out CannabinoidProfile profile)
    {
        profile = new CannabinoidProfile
        {
            THC = Empty("THC"),
            THCA = Empty("THCA"),
            CBD = Empty("CBD"),
            CBDA = Empty("CBDA")
        };

        var rows = NormalizeRows(text);

        for (var i = 0; i < rows.Count; i++)
        {
            var headerTokens = Tokenize(rows[i]);

            if (!LooksLikeCannabinoidHeader(headerTokens))
                continue;

            var percentRowIndex = FindPercentRow(rows, i + 1);

            if (percentRowIndex is null)
                continue;

            var percentTokens = Tokenize(rows[percentRowIndex.Value]);

            if (percentTokens.Count < 2 || percentTokens[0] != "%")
                continue;

            var values = percentTokens.Skip(1).ToList();
            var columns = headerTokens;

            if (values.Count == headerTokens.Count + 1)
                columns = ["TOTAL CANNABINOIDS", .. headerTokens];

            if (values.Count != columns.Count)
                continue;

            var parsedAny = false;

            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var fieldName = NormalizeCannabinoidName(columns[columnIndex]);

                if (fieldName is null ||
                    !TryParseResultValue(values[columnIndex], out var value))
                {
                    continue;
                }

                var field = new ParsedField<decimal>
                {
                    FieldName = fieldName,
                    Value = value,
                    SourceText = rows[percentRowIndex.Value],
                    Confidence = 0.95m
                };

                switch (fieldName)
                {
                    case "THC":
                        profile.THC = field;
                        parsedAny = true;
                        break;
                    case "THCA":
                        profile.THCA = field;
                        parsedAny = true;
                        break;
                    case "CBD":
                        profile.CBD = field;
                        parsedAny = true;
                        break;
                    case "CBDA":
                        profile.CBDA = field;
                        parsedAny = true;
                        break;
                }
            }

            if (parsedAny)
                return true;
        }

        return false;
    }

    private static bool TryParseKaychaEdibleCannabinoids(string text, out CannabinoidProfile profile)
    {
        profile = new CannabinoidProfile
        {
            THC = Empty("THC"),
            THCA = Empty("THCA"),
            CBD = Empty("CBD"),
            CBDA = Empty("CBDA")
        };

        if (!HasKaychaEdibleContext(text))
            return false;

        var rows = NormalizeRows(text);
        var tableStartIndex = rows.FindIndex(row =>
            row.Contains("Cannabinoid Relative Concentration", StringComparison.OrdinalIgnoreCase));

        if (tableStartIndex < 0)
            return false;

        var parsedAny = false;
        var delta8 = 0m;

        for (var i = tableStartIndex + 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (IsBlockedKaychaEdibleCannabinoidRow(row))
                continue;

            var match = EdibleCannabinoidRowRegex.Match(row);

            if (!match.Success)
                continue;

            var fieldName = NormalizeEdibleCannabinoidName(match.Groups["name"].Value);

            if (fieldName is null)
                continue;

            var field = CreateEdibleCannabinoidField(fieldName, match.Groups["mass"].Value, row);
            parsedAny = true;

            switch (fieldName)
            {
                case "THC":
                    profile.THC = field;
                    break;
                case "THCA":
                    profile.THCA = field;
                    break;
                case "CBD":
                    profile.CBD = field;
                    break;
                case "CBDA":
                    profile.CBDA = field;
                    break;
                case "D8-THC":
                    delta8 = field.Value;
                    break;
            }
        }

        if (!parsedAny)
            return false;

        profile.TotalTHC = profile.THC.Value + (profile.THCA.Value * 0.877m) + delta8;
        profile.TotalCBD = profile.CBD.Value + (profile.CBDA.Value * 0.877m);

        return true;
    }

    private static bool TryParseKaychaTotalTerpenes(string text, out decimal totalTerpenes)
    {
        totalTerpenes = 0m;

        foreach (var row in NormalizeRows(text))
        {
            var match = TotalTerpenesRowRegex.Match(row);

            if (!match.Success ||
                !decimal.TryParse(match.Groups["percent"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var percent))
            {
                continue;
            }

            if (decimal.TryParse(match.Groups["mg"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var mgPerGram) &&
                Math.Abs((percent * 10m) - mgPerGram) > 0.01m)
            {
                continue;
            }

            totalTerpenes = percent;
            return true;
        }

        return false;
    }

    private static bool TryParseKaychaTerpenes(string text, out Dictionary<string, decimal> terpenes)
    {
        terpenes = new Dictionary<string, decimal>();

        foreach (var row in NormalizeRows(text))
        {
            var match = TerpeneRowRegex.Match(row);

            if (!match.Success)
                continue;

            var name = match.Groups["name"].Value.ToUpperInvariant();

            if (name == "TOTAL TERPENES" ||
                !TryParseResultValue(match.Groups["percent"].Value, out var percent))
            {
                continue;
            }

            if (TryParseResultValue(match.Groups["mg"].Value, out var mgPerGram) &&
                Math.Abs((percent * 10m) - mgPerGram) > 0.01m)
            {
                continue;
            }

            terpenes[name] = percent;
        }

        return terpenes.Count > 0;
    }

    private static bool LooksLikeCannabinoidHeader(IReadOnlyCollection<string> tokens)
    {
        return tokens.Contains("THCA", StringComparer.OrdinalIgnoreCase) &&
               tokens.Contains("D9-THC", StringComparer.OrdinalIgnoreCase) &&
               tokens.Contains("D8-THC", StringComparer.OrdinalIgnoreCase) &&
               tokens.Contains("CBDA", StringComparer.OrdinalIgnoreCase) &&
               tokens.Contains("CBD", StringComparer.OrdinalIgnoreCase) &&
               tokens.Contains("CBGA", StringComparer.OrdinalIgnoreCase);
    }

    private static int? FindPercentRow(IReadOnlyList<string> rows, int startIndex)
    {
        var endIndex = Math.Min(rows.Count - 1, startIndex + 4);

        for (var i = startIndex; i <= endIndex; i++)
        {
            if (rows[i].StartsWith("% ", StringComparison.Ordinal))
                return i;
        }

        return null;
    }

    private static string? NormalizeCannabinoidName(string name)
    {
        return name.ToUpperInvariant() switch
        {
            "THCA" => "THCA",
            "CBD" => "CBD",
            "CBDA" => "CBDA",
            "D9-THC" or "Δ9-THC" or "∆9-THC" => "THC",
            _ => null
        };
    }

    private static bool HasKaychaEdibleContext(string text)
    {
        return text.Contains("Kaycha Labs", StringComparison.OrdinalIgnoreCase) &&
               (text.Contains("Ingestible", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Soft Chew", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Gummy", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBlockedKaychaEdibleCannabinoidRow(string row)
    {
        return row.Contains("Strain:", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Gummy", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Aw:", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Water Activity", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("∆9-THC + ∆8-THC", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Δ9-THC + Δ8-THC", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeEdibleCannabinoidName(string name)
    {
        return name.ToUpperInvariant() switch
        {
            "CBD" => "CBD",
            "CBDA" => "CBDA",
            "THCA" => "THCA",
            "D9-THC" or "Δ9-THC" or "∆9-THC" => "THC",
            "D8-THC" or "Δ8-THC" or "∆8-THC" => "D8-THC",
            _ => null
        };
    }

    private static ParsedField<decimal> CreateEdibleCannabinoidField(string fieldName, string rawMass, string sourceText)
    {
        if (!TryParseEdibleResultValue(rawMass, out var value))
        {
            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = 0m,
                SourceText = sourceText,
                Confidence = 0m
            };
        }

        return new ParsedField<decimal>
        {
            FieldName = fieldName,
            Value = value,
            SourceText = sourceText,
            Confidence = 0.95m
        };
    }

    private static bool TryParseEdibleResultValue(string raw, out decimal value)
    {
        value = 0m;
        var normalized = Regex.Replace(raw.Trim(), @"\s+", " ");

        if (normalized.StartsWith("<", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("NT", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("Not Detected", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseResultValue(string raw, out decimal value)
    {
        value = 0m;
        var normalized = Regex.Replace(raw.Trim(), @"\s+", string.Empty);

        if (normalized.StartsWith("<", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("NT", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
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

    private static List<string> Tokenize(string row)
    {
        return row.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
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
