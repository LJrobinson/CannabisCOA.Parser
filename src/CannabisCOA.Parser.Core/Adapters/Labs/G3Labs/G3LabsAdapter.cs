using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.G3Labs;

public class G3LabsAdapter : BaseLabAdapter
{
    public override string LabName => "G3 Labs";

    private static readonly Regex CannabinoidRowRegex = new(
        @"^\s*(?<name>THCa|THCA|Δ9-THC|∆9-THC|CBDa|CBDA|CBD)\s+(?<loq><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MgPerGramRegex = new(
        @"(?<value>\d{1,6}(?:\.\d+)?|\.\d+)\s*mg\s*/\s*g",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TerpeneResultTripleRegex = new(
        @"^\s+(?<loq><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly (string CanonicalName, Regex NameRegex)[] G3TerpeneAnchors =
    [
        ("β-Myrcene", new Regex(@"(?<![\p{L}\p{N}])β\s*-\s*Myrcene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Pinene", new Regex(@"(?<![\p{L}\p{N}])α\s*-\s*Pinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Pinene", new Regex(@"(?<![\p{L}\p{N}])β\s*-\s*Pinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Caryophyllene", new Regex(@"(?<![\p{L}\p{N}])β\s*-\s*Caryophyllene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Humulene", new Regex(@"(?<![\p{L}\p{N}])α\s*-\s*Humulene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("δ-Limonene", new Regex(@"(?<![\p{L}\p{N}])δ\s*-\s*Limonene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Linalool", new Regex(@"(?<![\p{L}\p{N}])Linalool(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Terpinolene", new Regex(@"(?<![\p{L}\p{N}])Terpinolene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Ocimene", new Regex(@"(?<![\p{L}\p{N}])β\s*-\s*Ocimene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Bisabolol", new Regex(@"(?<![\p{L}\p{N}])α\s*-\s*Bisabolol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Terpinene", new Regex(@"(?<![\p{L}\p{N}])α\s*-\s*Terpinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Camphene", new Regex(@"(?<![\p{L}\p{N}])Camphene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Caryophyllene Oxide", new Regex(@"(?<![\p{L}\p{N}])Caryophyllene\s+Oxide(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Eucalyptol", new Regex(@"(?<![\p{L}\p{N}])Eucalyptol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("γ-Terpinene", new Regex(@"(?<![\p{L}\p{N}])γ\s*-\s*Terpinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Geraniol", new Regex(@"(?<![\p{L}\p{N}])Geraniol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Guaiol", new Regex(@"(?<![\p{L}\p{N}])Guaiol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Isopulegol", new Regex(@"(?<![\p{L}\p{N}])Isopulegol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Ocimene", new Regex(@"(?<![\p{L}\p{N}-])Ocimene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("p-Cymene", new Regex(@"(?<![\p{L}\p{N}])p\s*-\s*Cymene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("trans-Nerolidol", new Regex(@"(?<![\p{L}\p{N}])trans\s*-\s*Nerolidol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Nerolidol", new Regex(@"(?<![\p{L}\p{N}-])Nerolidol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("3-Carene", new Regex(@"(?<![\p{L}\p{N}])3\s*-\s*Carene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled))
    ];

    protected override string[] DetectionTerms =>
    [
        "G3LABS",
        "G3 LABS",
        "G3 LABORATORIES"
    ];

    public override CoaResult Parse(string text)
    {
        var result = base.Parse(text);
        var rows = NormalizeRows(text);
        var documentClassification = ClassifyG3Report(rows, result.ProductType);
        var isFullComplianceCoa = documentClassification.Equals("FullComplianceCoa", StringComparison.OrdinalIgnoreCase);

        result.DocumentClassification = documentClassification;
        result.IsFullComplianceCoa = isFullComplianceCoa;

        if (result.ProductType == ProductType.Flower)
        {
            result.ProductName = ExtractProductName(text);
            result.BatchId = ExtractBatchId(text);
        }

        if (isFullComplianceCoa && TryParseG3Cannabinoids(text, out var cannabinoids))
        {
            CannabinoidCalculator.CalculateTotals(cannabinoids);
            result.Cannabinoids = cannabinoids;
        }
        else if (!isFullComplianceCoa)
        {
            result.Cannabinoids = new CannabinoidProfile();
        }

        if (TryParseG3TotalTerpenes(text, out var totalTerpenes))
            result.Terpenes.TotalTerpenes = totalTerpenes;

        if (TryParseG3Terpenes(text, out var terpenes))
        {
            result.Terpenes.Terpenes.Clear();

            foreach (var terpene in terpenes)
                result.Terpenes.Terpenes[terpene.Key] = terpene.Value;
        }

        return result;
    }

    public override ProductType DetectProductType(string text)
    {
        return NormalizeRows(text).Any(IsG3FlowerDescriptor)
            ? ProductType.Flower
            : base.DetectProductType(text);
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
            if (!IsG3FlowerDescriptor(rows[i]))
                continue;

            for (var j = i - 1; j >= 0 && i - j <= 3; j--)
            {
                var candidate = rows[j].Trim();

                if (IsG3ProductNameCandidate(candidate))
                    return candidate;
            }
        }

        return string.Empty;
    }

    private static string ExtractStrainName(IEnumerable<string> rows)
    {
        foreach (var row in rows)
        {
            var match = Regex.Match(row, @"\bStrain\s*:\s*(?<strain>.+?)(?:\s+Batch\s*#:|$)", RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var strain = match.Groups["strain"].Value.Trim();

            if (IsG3ProductNameCandidate(strain))
                return strain;
        }

        return string.Empty;
    }

    private static string ExtractBatchId(string text)
    {
        foreach (var row in NormalizeRows(text))
        {
            var match = Regex.Match(
                row,
                @"\bBatch\s*#\s*:\s*(?<batch>.*?)(?:\s*;\s*Lot\s*#|\s+Lot\s*#:|\s+Sample\s+Received:|\s+Report\s+Created:|$)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var batch = match.Groups["batch"].Value.Trim();

            if (!string.IsNullOrWhiteSpace(batch))
                return batch;
        }

        return string.Empty;
    }

    private static bool IsG3ProductNameCandidate(string row)
    {
        return !string.IsNullOrWhiteSpace(row) &&
               !Regex.IsMatch(row, @"^[\s\-–—_]+$") &&
               !row.Equals("Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Plant", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Popcorn Buds", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Trim", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Plant,", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains(':') &&
               !row.Contains(';') &&
               !row.Contains("G3 Labs", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("G3 Laboratories", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Certificate", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Lic.", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Sparks", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Laughlin", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Amargosa Valley", StringComparison.OrdinalIgnoreCase) &&
               !Regex.IsMatch(row, @"^\d{5,}[\s._-]+\S+", RegexOptions.IgnoreCase) &&
               !Regex.IsMatch(row, @"^1A[0-9A-Z]{20,}$", RegexOptions.IgnoreCase) &&
               !Regex.IsMatch(row, @"^\(?\d{3}\)?[\s-]\d{3}[\s-]\d{4}") &&
               !Regex.IsMatch(row, @"^\d+\s+of\s+\d+$", RegexOptions.IgnoreCase);
    }

    private static bool IsG3FlowerDescriptor(string row)
    {
        return Regex.IsMatch(
            row,
            @"\bPlant\s*,\s*(?:Flower(?:\s*-\s*Cured)?|Popcorn\s+Buds|Trim(?:\s*,\s*(?:Indoor|Light\s+Deprivation))?(?!\s*,))\b",
            RegexOptions.IgnoreCase);
    }

    private static string ClassifyG3Report(IReadOnlyList<string> rows, ProductType productType)
    {
        if (productType != ProductType.Flower || !rows.Any(IsG3FlowerDescriptor))
            return "FullComplianceCoa";

        if (HasG3CannabinoidPanel(rows) || HasG3TerpenePanel(rows))
            return "FullComplianceCoa";

        if (HasG3PanelResult(rows, "Heavy Metals") &&
            !HasG3PanelResult(rows, "Pesticides") &&
            !HasG3PanelResult(rows, "Microbials") &&
            !HasG3PanelResult(rows, "Mycotoxins"))
        {
            return "SinglePanelTest";
        }

        return "FullComplianceCoa";
    }

    private static bool HasG3CannabinoidPanel(IEnumerable<string> rows)
    {
        return rows.Any(row =>
            row.Contains("Cannabinoids", StringComparison.OrdinalIgnoreCase) ||
            row.Contains("Total Potential THC", StringComparison.OrdinalIgnoreCase) ||
            Regex.IsMatch(row, @"^\s*THCa\b", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(row, @"^\s*(?:Δ|∆)9\s*-\s*THC\b", RegexOptions.IgnoreCase));
    }

    private static bool HasG3TerpenePanel(IEnumerable<string> rows)
    {
        return rows.Any(row =>
            row.Contains("Terpenes", StringComparison.OrdinalIgnoreCase) ||
            row.Contains("Total Terpenes", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasG3PanelResult(IEnumerable<string> rows, string panelName)
    {
        var panelPattern = Regex.Escape(panelName).Replace(@"\ ", @"\s+");

        return rows.Any(row =>
            Regex.IsMatch(
                row,
                $@"\b{panelPattern}\s*(?:\([^)]*\)\s*)?(?:Pass|Fail)\b",
                RegexOptions.IgnoreCase));
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

    private static bool TryParseG3Terpenes(string text, out Dictionary<string, decimal> terpenes)
    {
        terpenes = new Dictionary<string, decimal>();

        foreach (var row in NormalizeRows(text))
        {
            foreach (var anchor in G3TerpeneAnchors)
            {
                var nameMatch = anchor.NameRegex.Match(row);

                if (!nameMatch.Success)
                    continue;

                var afterName = row[(nameMatch.Index + nameMatch.Length)..];
                var valueMatch = TerpeneResultTripleRegex.Match(afterName);

                if (!valueMatch.Success ||
                    !TryParseDecimalToken(valueMatch.Groups["percent"].Value, out var percent) ||
                    percent <= 0m ||
                    percent > 25m)
                {
                    continue;
                }

                if (TryParseDecimalToken(valueMatch.Groups["mg"].Value, out var mgPerGram) &&
                    mgPerGram > 0m &&
                    Math.Abs((mgPerGram / 10m) - percent) > 0.001m)
                {
                    continue;
                }

                terpenes[anchor.CanonicalName] = percent;
            }
        }

        return terpenes.Count > 0;
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
        if (IsNonDetectResultValue(raw))
        {
            return 0m;
        }

        var normalized = Regex.Replace(raw.Trim(), @"\s+", string.Empty);

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;
    }

    private static bool TryParseDecimalToken(string raw, out decimal value)
    {
        value = 0m;

        if (IsNonDetectResultValue(raw))
            return false;

        var normalized = Regex.Replace(raw.Trim(), @"\s+", string.Empty);

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
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
