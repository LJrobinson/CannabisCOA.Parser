using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.MAAnalytics;

public class MAAnalyticsAdapter : BaseLabAdapter
{
    public override string LabName => "MA Analytics";

    private static readonly Regex CannabinoidRowRegex = new(
        @"^\s*(?<name>THCa|THCA|Δ9-THC|∆9-THC|THC|Δ8-THC|∆8-THC|CBDa|CBDA|CBD|CBC|CBG|CBN|THCV|CBGa|CBGA)\s+(?<loq><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TerpeneTotalRegex = new(
        @"\bTotal\s+(?<mg>\d{1,6}(?:\.\d+)?)\s+(?<percent>\d{1,2}(?:\.\d+)?)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TerpeneRowRegex = new(
        @"(?<![\p{L}\p{N}])(?<name>(?:β|beta)\s*-\s*Myrcene|(?:β|beta)\s*-\s*Caryophyllene|(?:δ|delta)\s*-\s*Limonene|Linalool|(?:α|alpha)\s*-\s*Humulene|(?:β|beta)\s*-\s*Pinene|(?:α|alpha)\s*-\s*Pinene|(?:α|alpha)\s*-\s*Bisabolol|Terpinolene|(?:β|beta)\s*-\s*Ocimene|Caryophyllene\s+Oxide|cis\s*-\s*Nerolidol|trans\s*-\s*Nerolidol|Nerolidol|(?:δ|delta)\s*-\s*3\s*-\s*Carene|3\s*-\s*Carene|Camphene|Guaiol|Eucalyptol|(?:γ|gamma)\s*-\s*Terpinene|p\s*-\s*Cymene|Geraniol|Isopulegol|Farnesene|Fenchone|Valencene|Citronellol|Menthol)\s+(?<loq><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    protected override string[] DetectionTerms =>
    [
        "MA ANALYTICS",
        "M.A. ANALYTICS"
    ];

    public override CoaResult Parse(string text)
    {
        var result = base.Parse(text);

        if (TryParseMaCannabinoids(text, out var cannabinoids))
        {
            CannabinoidCalculator.CalculateTotals(cannabinoids);
            result.Cannabinoids = cannabinoids;
        }

        if (TryParseMaTotalTerpenes(text, out var totalTerpenes))
            result.Terpenes.TotalTerpenes = totalTerpenes;

        if (TryParseMaTerpenes(text, out var terpenes))
        {
            result.Terpenes.Terpenes.Clear();

            foreach (var terpene in terpenes)
                result.Terpenes.Terpenes[terpene.Key] = terpene.Value;
        }

        return result;
    }

    private static bool TryParseMaCannabinoids(string text, out CannabinoidProfile profile)
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

            if (fieldName is null)
                continue;

            var percentRaw = match.Groups["percent"].Value;
            var field = new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = ParseResultValue(percentRaw),
                SourceText = row,
                Confidence = IsNonDetectResultValue(percentRaw) ? 0m : 0.95m
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

    private static bool TryParseMaTotalTerpenes(string text, out decimal totalTerpenes)
    {
        totalTerpenes = 0m;

        foreach (Match match in TerpeneTotalRegex.Matches(text))
        {
            if (!decimal.TryParse(match.Groups["mg"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var mgPerGram) ||
                !decimal.TryParse(match.Groups["percent"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var percent))
            {
                continue;
            }

            if (percent > 0m && percent <= 25m && Math.Abs((mgPerGram / 10m) - percent) <= 0.0001m)
            {
                totalTerpenes = percent;
                return true;
            }
        }

        return false;
    }

    private static bool TryParseMaTerpenes(string text, out Dictionary<string, decimal> terpenes)
    {
        terpenes = new Dictionary<string, decimal>();

        foreach (var row in NormalizeRows(text))
        {
            var match = TerpeneRowRegex.Match(row);

            if (!match.Success)
                continue;

            var percent = ParseResultValue(match.Groups["percent"].Value);

            if (percent <= 0m)
                continue;

            var mgPerGram = ParseResultValue(match.Groups["mg"].Value);

            if (mgPerGram > 0m && Math.Abs((mgPerGram / 10m) - percent) > 0.0001m)
                continue;

            terpenes[match.Groups["name"].Value] = percent;
        }

        return terpenes.Count > 0;
    }

    private static decimal ParseResultValue(string raw)
    {
        if (IsNonDetectResultValue(raw))
        {
            return 0m;
        }

        var normalized = Regex.Replace(raw.Trim(), @"\s+", string.Empty);

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;
    }

    private static bool IsNonDetectResultValue(string raw)
    {
        var normalized = Regex.Replace(raw.Trim(), @"\s+", string.Empty);

        return normalized.StartsWith("<", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("NT", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("NOTDETECTED", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeCannabinoidName(string name)
    {
        return name.ToUpperInvariant() switch
        {
            "THCA" => "THCA",
            "CBD" => "CBD",
            "CBDA" => "CBDA",
            "THC" or "Δ9-THC" or "∆9-THC" => "THC",
            _ => null
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
