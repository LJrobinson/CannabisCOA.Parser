using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.G3Labs;

public class G3LabsAdapter : BaseLabAdapter
{
    public override string LabName => "G3 Labs";

    private static readonly Regex CannabinoidRowRegex = new(
        @"^\s*(?<name>THCa|THCA|Δ9-THC|∆9-THC|CBDa|CBDA|CBD)\s+(?<loq><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MgPerGramRegex = new(
        @"(?<value>\d{1,6}(?:\.\d+)?|\.\d+)\s*mg\s*/\s*g",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    protected override string[] DetectionTerms =>
    [
        "G3LABS",
        "G3 LABS",
        "G3 LABORATORIES"
    ];

    public override CoaResult Parse(string text)
    {
        var result = base.Parse(text);

        if (TryParseG3Cannabinoids(text, out var cannabinoids))
        {
            CannabinoidCalculator.CalculateTotals(cannabinoids);
            result.Cannabinoids = cannabinoids;
        }

        if (TryParseG3TotalTerpenes(text, out var totalTerpenes))
            result.Terpenes.TotalTerpenes = totalTerpenes;

        return result;
    }

    private static bool TryParseG3Cannabinoids(string text, out CannabinoidProfile profile)
    {
        profile = new CannabinoidProfile
        {
            THC = Empty("THC"),
            THCA = Empty("THCA"),
            CBD = Empty("CBD"),
            CBDA = Empty("CBDA")
        };

        var parsedAny = false;

        foreach (var row in NormalizeRows(text))
        {
            var match = CannabinoidRowRegex.Match(row);

            if (!match.Success)
                continue;

            var fieldName = NormalizeCannabinoidName(match.Groups["name"].Value);
            var field = new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = ParseResultValue(match.Groups["percent"].Value),
                SourceText = row,
                Confidence = 0.95m
            };

            switch (fieldName)
            {
                case "THC" when profile.THC.Confidence == 0m:
                    profile.THC = field;
                    parsedAny = true;
                    break;
                case "THCA" when profile.THCA.Confidence == 0m:
                    profile.THCA = field;
                    parsedAny = true;
                    break;
                case "CBD" when profile.CBD.Confidence == 0m:
                    profile.CBD = field;
                    parsedAny = true;
                    break;
                case "CBDA" when profile.CBDA.Confidence == 0m:
                    profile.CBDA = field;
                    parsedAny = true;
                    break;
            }
        }

        return parsedAny;
    }

    private static bool TryParseG3TotalTerpenes(string text, out decimal totalTerpenes)
    {
        totalTerpenes = 0m;
        var rows = NormalizeRows(text);

        for (var i = 0; i < rows.Count; i++)
        {
            if (!rows[i].Contains("Total Terpenes", StringComparison.OrdinalIgnoreCase))
                continue;

            var start = Math.Max(0, i - 2);
            var end = Math.Min(rows.Count - 1, i + 2);

            for (var j = i; j >= start; j--)
            {
                if (TryExtractMgPerGramTotal(rows[j], out totalTerpenes))
                    return true;
            }

            for (var j = i + 1; j <= end; j++)
            {
                if (TryExtractMgPerGramTotal(rows[j], out totalTerpenes))
                    return true;
            }
        }

        return false;
    }

    private static bool TryExtractMgPerGramTotal(string row, out decimal totalTerpenes)
    {
        totalTerpenes = 0m;
        var match = MgPerGramRegex.Match(row);

        if (!match.Success ||
            !decimal.TryParse(match.Groups["value"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var mgPerGram))
        {
            return false;
        }

        totalTerpenes = mgPerGram / 10m;
        return true;
    }

    private static decimal ParseResultValue(string raw)
    {
        var normalized = Regex.Replace(raw.Trim(), @"\s+", string.Empty);

        if (normalized.StartsWith("<", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("NT", StringComparison.OrdinalIgnoreCase))
        {
            return 0m;
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;
    }

    private static string NormalizeCannabinoidName(string name)
    {
        return name.ToUpperInvariant() switch
        {
            "THCA" => "THCA",
            "CBD" => "CBD",
            "CBDA" => "CBDA",
            _ => "THC"
        };
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
