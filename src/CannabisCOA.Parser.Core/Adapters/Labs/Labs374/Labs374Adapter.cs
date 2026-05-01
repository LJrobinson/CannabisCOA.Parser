using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Labs374;

public class Labs374Adapter : BaseLabAdapter
{
    public override string LabName => "374 Labs";

    private static readonly Regex CannabinoidRowRegex = new(
        @"^\s*(?<name>THCa|THCA|Δ9-THC|∆9-THC|Δ8-THC|∆8-THC|CBDa|CBDA|CBD|CBC|CBG|CBN|THCV|CBGa|CBGA)\s+(?<loq><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TerpeneValueTripleRegex = new(
        @"^\s+(?<loq><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|<\s*MDL|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly (string CanonicalName, Regex NameRegex)[] TerpeneAnchors =
    [
        ("β-Myrcene", new Regex(@"(?<![\p{L}\p{N}])(?:β|Beta|B)\s*-?\s*Myrcene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("δ-Limonene", new Regex(@"(?<![\p{L}\p{N}])(?:(?:δ|Delta|D)\s*-\s*)?Limonene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Caryophyllene", new Regex(@"(?<![\p{L}\p{N}])(?:β|Beta|B)\s*-?\s*Caryophyllene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Humulene", new Regex(@"(?<![\p{L}\p{N}])(?:α|Alpha|A)\s*-?\s*Humulene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Linalool", new Regex(@"(?<![\p{L}\p{N}])Linalool(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Pinene", new Regex(@"(?<![\p{L}\p{N}])(?:β|Beta|B)\s*-?\s*Pinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Bisabolol", new Regex(@"(?<![\p{L}\p{N}])(?:α|Alpha|A)\s*-?\s*Bisabolol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Pinene", new Regex(@"(?<![\p{L}\p{N}])(?:α|Alpha|A)\s*-?\s*Pinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Caryophyllene Oxide", new Regex(@"(?<![\p{L}\p{N}])Caryophyllene\s+Oxide(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Ocimene", new Regex(@"(?<![\p{L}\p{N}])Ocimene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Terpinolene", new Regex(@"(?<![\p{L}\p{N}])Terpinolene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("trans-Nerolidol", new Regex(@"(?<![\p{L}\p{N}])(?:trans\s*-?\s*)?Nerolidol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Camphene", new Regex(@"(?<![\p{L}\p{N}])Camphene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("δ-3-Carene", new Regex(@"(?<![\p{L}\p{N}])(?:(?:δ|Delta|D)\s*-\s*)?3\s*-?\s*Carene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Isopulegol", new Regex(@"(?<![\p{L}\p{N}])Isopulegol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Guaiol", new Regex(@"(?<![\p{L}\p{N}])Guaiol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Eucalyptol", new Regex(@"(?<![\p{L}\p{N}])Eucalyptol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Fenchol", new Regex(@"(?<![\p{L}\p{N}])Fenchol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Citronellol", new Regex(@"(?<![\p{L}\p{N}])Citronellol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled))
    ];

    protected override string[] DetectionTerms =>
    [
        "374LABS",
        "374 LABS",
        "374 LABORATORIES"
    ];

    public override CoaResult Parse(string text)
    {
        var result = base.Parse(text);

        if (result.ProductType == ProductType.Flower)
        {
            result.ProductName = ExtractProductName(text);
            result.BatchId = ExtractBatchId(text);
        }

        if (TryParse374Cannabinoids(text, out var cannabinoids))
        {
            CannabinoidCalculator.CalculateTotals(cannabinoids);
            result.Cannabinoids = cannabinoids;
        }

        if (TryParse374Terpenes(text, out var terpenes))
        {
            result.Terpenes.Terpenes.Clear();

            foreach (var terpene in terpenes)
                result.Terpenes.Terpenes[terpene.Key] = terpene.Value;

            if (result.Terpenes.TotalTerpenes == 0m)
                result.Terpenes.TotalTerpenes = terpenes.Values.Sum();
        }

        return result;
    }

    public override ProductType DetectProductType(string text)
    {
        var productType = base.DetectProductType(text);

        if (productType != ProductType.Unknown)
            return productType;

        return NormalizeRows(text).Any(Is374FlowerDescriptor)
            ? ProductType.Flower
            : ProductType.Unknown;
    }

    private static string ExtractProductName(string text)
    {
        var rows = NormalizeRows(text);
        var displayedProductName = ExtractDisplayedProductName(rows);

        if (!string.IsNullOrWhiteSpace(displayedProductName))
            return displayedProductName;

        return ExtractStrainName(rows);
    }

    private static string ExtractDisplayedProductName(IReadOnlyList<string> rows)
    {
        for (var i = 1; i < rows.Count; i++)
        {
            if (!Is374FlowerDescriptor(rows[i]))
                continue;

            for (var j = i - 1; j >= 0 && i - j <= 4; j--)
            {
                var candidate = CleanMetadataValue(rows[j]);

                if (Is374ProductNameCandidate(candidate))
                    return candidate;
            }
        }

        return string.Empty;
    }

    private static string ExtractStrainName(IEnumerable<string> rows)
    {
        foreach (var row in rows)
        {
            var match = Regex.Match(
                row,
                @"\bStrain\s*:\s*(?<strain>.+?)(?:,\s*Classi\S*cation\s*:|;\s*|$)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var strain = CleanMetadataValue(match.Groups["strain"].Value);

            if (Is374ProductNameCandidate(strain))
                return strain;
        }

        return string.Empty;
    }

    private static string ExtractBatchId(string text)
    {
        var rows = NormalizeRows(text);
        var batchId = ExtractBatchId(rows, @"\bBatch\s*#\s*:\s*(?<batch>.*?)(?:\s*;\s*Lot\s*#|\s+Lot\s*#:|\s+Sample\s+Received:|\s+Report\s+Created:|\s+Harvest/Production\s+Date:|$)");

        if (!string.IsNullOrWhiteSpace(batchId))
            return batchId;

        return ExtractBatchId(rows, @"\bMETRC\s+Batch\s*:\s*(?<batch>.*?)(?:\s*;\s*METRC\s+Sample\s*:|\s*;|$)");
    }

    private static string ExtractBatchId(IEnumerable<string> rows, string pattern)
    {
        foreach (var row in rows)
        {
            var match = Regex.Match(row, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var batch = CleanMetadataValue(match.Groups["batch"].Value);

            if (Is374BatchIdCandidate(batch))
                return batch;
        }

        return string.Empty;
    }

    private static string CleanMetadataValue(string value)
    {
        return Regex.Replace(value.Replace("\0", string.Empty).Trim(), @"\s+", " ");
    }

    private static bool Is374ProductNameCandidate(string row)
    {
        return !string.IsNullOrWhiteSpace(row) &&
               !IsPlaceholder(row) &&
               !row.Equals("Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Plant", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Trim", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Bulk", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Bulk Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Ground Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Plant,", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Batch", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Lot", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("METRC", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Harvest Process", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains(':') &&
               !row.Contains(';') &&
               !row.Contains("@") &&
               !row.Contains("374 Labs", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("www.", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Certificate", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Certi", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Confident LIMS", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Lic.", StringComparison.OrdinalIgnoreCase) &&
               !Regex.IsMatch(row, @"^1A[0-9A-Z]{16,}$", RegexOptions.IgnoreCase) &&
               !Regex.IsMatch(row, @"^\(?\d{3}\)?[\s-]\d{3}[\s-]\d{4}") &&
               !Regex.IsMatch(row, @"^\d+\s+of\s+\d+$", RegexOptions.IgnoreCase) &&
               !Regex.IsMatch(row, @"\b(?:Greg\s+St|Sparks,\s*NV|Las\s+Vegas,\s*NV|Pahrump,\s*NV|Drive|Road|Street|Avenue)\b", RegexOptions.IgnoreCase);
    }

    private static bool Is374BatchIdCandidate(string row)
    {
        return !string.IsNullOrWhiteSpace(row) &&
               !IsPlaceholder(row) &&
               !row.Equals("Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Plant, Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("374 Labs", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("@") &&
               !row.Contains("www.", StringComparison.OrdinalIgnoreCase) &&
               !Regex.IsMatch(row, @"^\(?\d{3}\)?[\s-]\d{3}[\s-]\d{4}");
    }

    private static bool IsPlaceholder(string value)
    {
        return Regex.IsMatch(value, @"^[\s\-–—_]+$");
    }

    private static bool Is374FlowerDescriptor(string row)
    {
        return Regex.IsMatch(
            row,
            @"\bPlant\s*,\s*(?:Flower(?:\s*-\s*Cured)?|Popcorn\s+Buds|Trim\s*,\s*Indoor|Ground\s+Flower\s*,\s*Indoor|Bulk\s*,\s*Flower|Bulk\s+Flower)\b",
            RegexOptions.IgnoreCase);
    }

    private static bool TryParse374Cannabinoids(string text, out CannabinoidProfile profile)
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

    private static bool TryParse374Terpenes(string text, out Dictionary<string, decimal> terpenes)
    {
        terpenes = new Dictionary<string, decimal>();

        foreach (var row in Extract374PotencyTableRows(text))
        {
            foreach (var anchor in TerpeneAnchors)
            {
                foreach (Match nameMatch in anchor.NameRegex.Matches(row))
                {
                    if (!nameMatch.Success)
                        continue;

                    var afterName = row[(nameMatch.Index + nameMatch.Length)..];
                    var valueMatch = TerpeneValueTripleRegex.Match(afterName);

                    if (!valueMatch.Success ||
                        !TryParseTerpeneResult(
                            valueMatch.Groups["percent"].Value,
                            valueMatch.Groups["mg"].Value,
                            out var percent))
                    {
                        continue;
                    }

                    terpenes[anchor.CanonicalName] = percent;
                }
            }
        }

        return terpenes.Count > 0;
    }

    private static bool TryParseTerpeneResult(string rawPercent, string rawMgPerGram, out decimal percent)
    {
        percent = 0m;

        if (IsNonDetectResultValue(rawPercent) && IsNonDetectResultValue(rawMgPerGram))
            return false;

        if (IsNonDetectResultValue(rawPercent) || IsNonDetectResultValue(rawMgPerGram))
            return false;

        if (!decimal.TryParse(rawPercent, NumberStyles.Number, CultureInfo.InvariantCulture, out percent) ||
            !decimal.TryParse(rawMgPerGram, NumberStyles.Number, CultureInfo.InvariantCulture, out var mgPerGram) ||
            percent <= 0m ||
            percent > 25m)
        {
            return false;
        }

        return Math.Abs((percent * 10m) - mgPerGram) <= Math.Max(0.01m, mgPerGram * 0.01m);
    }

    private static IEnumerable<string> Extract374PotencyTableRows(string text)
    {
        var inTable = false;

        foreach (var row in NormalizeRows(text))
        {
            if (!inTable)
            {
                if (Is374PotencyTableStart(row))
                    inTable = true;

                continue;
            }

            if (Is374PotencyTableEnd(row))
                yield break;

            yield return row;
        }
    }

    private static bool Is374PotencyTableStart(string row)
    {
        return row.Contains("Cannabinoid", StringComparison.OrdinalIgnoreCase) &&
               row.Contains("Terpene", StringComparison.OrdinalIgnoreCase);
    }

    private static bool Is374PotencyTableEnd(string row)
    {
        return row.StartsWith("10 Greg St", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("www.374labs.com", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Unless otherwise", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Microbials", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Residual Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase);
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
            "Δ9-THC" or "∆9-THC" => "THC",
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
