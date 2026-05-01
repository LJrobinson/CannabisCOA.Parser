using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.NVCannLabs;

public class NVCannLabsAdapter : BaseLabAdapter
{
    public override string LabName => "NV Cann Labs";

    private static readonly Regex MgPerGramRegex = new(
        @"(?<value>\d{1,6}(?:\.\d+)?|\.\d+)\s*mg\s*/\s*g",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TerpeneResultTripleRegex = new(
        @"^\s+(?<loq><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ResultTokenRegex = new(
        @"<\s*LOQ|<\s*LOD|ND|NR|NT|Not\s+Detected|\d{1,6}(?:\.\d+)?|\.\d+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly (string FieldName, Regex NameRegex)[] NvCannabinoidAnchors =
    [
        ("THCA", new Regex(@"^\s*THC\s*-?\s*A\b|^\s*THCa\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("CBDA", new Regex(@"^\s*CBD\s*-?\s*A\b|^\s*CBDa\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("D8-THC", new Regex(@"^\s*(?:Δ|∆)8\s*-\s*THC\b|^\s*D8\s*-\s*THC\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("THC", new Regex(@"^\s*(?:Δ|∆)9\s*-\s*THC\b|^\s*D9\s*-\s*THC\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("CBD", new Regex(@"^\s*CBD(?!\s*-?\s*A\b)(?!a\b)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled))
    ];

    private static readonly (string CanonicalName, Regex NameRegex)[] NvTerpeneAnchors =
    [
        ("Linalool", new Regex(@"(?<![\p{L}\p{N}])Linalool(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("δ-Limonene", new Regex(@"(?<![\p{L}\p{N}])(?:δ|delta)\s*-\s*Limonene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Caryophyllene", new Regex(@"(?<![\p{L}\p{N}])(?:β|beta)\s*-\s*Caryophyllene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Farnesene", new Regex(@"(?<![\p{L}\p{N}])Farnesene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Myrcene", new Regex(@"(?<![\p{L}\p{N}])(?:β|beta)\s*-\s*Myrcene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Humulene", new Regex(@"(?<![\p{L}\p{N}])(?:α|alpha)\s*-\s*Humulene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Bisabolol", new Regex(@"(?<![\p{L}\p{N}])(?:α|alpha)\s*-\s*Bisabolol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Pinene", new Regex(@"(?<![\p{L}\p{N}])(?:β|beta)\s*-\s*Pinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Terpineol", new Regex(@"(?<![\p{L}\p{N}])(?:α|alpha)\s*-\s*Terpineol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Pinene", new Regex(@"(?<![\p{L}\p{N}])(?:α|alpha)\s*-\s*Pinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("cis-Nerolidol", new Regex(@"(?<![\p{L}\p{N}])cis\s*-\s*Nerolidol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("trans-Nerolidol", new Regex(@"(?<![\p{L}\p{N}])trans\s*-\s*Nerolidol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Nerolidol", new Regex(@"(?<![\p{L}\p{N}-])Nerolidol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Terpinolene", new Regex(@"(?<![\p{L}\p{N}])Terpinolene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Caryophyllene Oxide", new Regex(@"(?<![\p{L}\p{N}])Caryophyllene\s+Oxide(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Camphene", new Regex(@"(?<![\p{L}\p{N}])Camphene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Guaiol", new Regex(@"(?<![\p{L}\p{N}])Guaiol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("δ-3-Carene", new Regex(@"(?<![\p{L}\p{N}])(?:(?:δ|delta)\s*-\s*)?3\s*-\s*Carene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Ocimene", new Regex(@"(?<![\p{L}\p{N}])(?:β|beta)\s*-\s*Ocimene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Eucalyptol", new Regex(@"(?<![\p{L}\p{N}])Eucalyptol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Fenchone", new Regex(@"(?<![\p{L}\p{N}])Fenchone(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("γ-Terpinene", new Regex(@"(?<![\p{L}\p{N}])(?:γ|gamma)\s*-\s*Terpinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Geraniol", new Regex(@"(?<![\p{L}\p{N}])Geraniol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Menthol", new Regex(@"(?<![\p{L}\p{N}])Menthol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Isopulegol", new Regex(@"(?<![\p{L}\p{N}])Isopulegol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("p-Cymene", new Regex(@"(?<![\p{L}\p{N}])p\s*-\s*Cymene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Valencene", new Regex(@"(?<![\p{L}\p{N}])Valencene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Citronellol", new Regex(@"(?<![\p{L}\p{N}])Citronellol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled))
    ];

    protected override string[] DetectionTerms =>
    [
        "NV CANNLABS",
        "NV CANN LABS",
        "NEVADA CANNLABS",
        "NEVADA CANN LABS"
    ];

    public override CoaResult Parse(string text)
    {
        var result = base.Parse(text);

        if (result.ProductType == ProductType.Flower)
        {
            result.ProductName = ExtractProductName(text);
            result.BatchId = ExtractBatchId(text);
        }

        if (TryParseNvSideBySideCannabinoids(text, result.ProductType, out var cannabinoids))
            result.Cannabinoids = cannabinoids;

        if (TryParseNvTotalTerpenes(text, out var totalTerpenes))
            result.Terpenes.TotalTerpenes = totalTerpenes;

        if (TryParseNvTerpenes(text, out var terpenes))
        {
            result.Terpenes.Terpenes.Clear();

            foreach (var terpene in terpenes)
                result.Terpenes.Terpenes[terpene.Key] = terpene.Value;
        }

        return result;
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
            if (!IsNvFlowerDescriptor(rows[i]))
                continue;

            for (var j = i - 1; j >= 0 && i - j <= 3; j--)
            {
                var candidate = rows[j].Trim();

                if (IsNvProductNameCandidate(candidate))
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

            if (IsNvProductNameCandidate(strain))
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
                @"\bBatch\s*#\s*:\s*(?<batch>.*?)(?:\s*;\s*Lot\s*#|\s+Lot\s*#:|\s+Sample\s+Received:|\s+Report\s+Created:|\s+Harvest/Production\s+Date:|$)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var batch = match.Groups["batch"].Value.Trim();

            if (!string.IsNullOrWhiteSpace(batch))
                return batch;
        }

        return string.Empty;
    }

    private static bool IsNvProductNameCandidate(string row)
    {
        return !string.IsNullOrWhiteSpace(row) &&
               !Regex.IsMatch(row, @"^[\s\-–—_]+$") &&
               !row.Equals("Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains(':') &&
               !row.Contains(';') &&
               !Regex.IsMatch(row, @"^\(?\d{3}\)?[\s-]\d{3}[\s-]\d{4}") &&
               !row.Contains("Las Vegas", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNvFlowerDescriptor(string row)
    {
        return Regex.IsMatch(
            row,
            @"\bPlant\s*,\s*Flower(?:\s*-\s*Cured)?\b",
            RegexOptions.IgnoreCase);
    }

    private static bool TryParseNvSideBySideCannabinoids(
        string text,
        ProductType productType,
        out CannabinoidProfile cannabinoids)
    {
        cannabinoids = CreateEmptyCannabinoidProfile();
        var acceptedRows = 0;
        var delta8 = 0m;

        foreach (var row in NormalizeRows(text))
        {
            if (!TryFindLeadingCannabinoid(row, out var fieldName, out var aliasEndIndex))
                continue;

            var slicedRow = SliceBeforeFirstTerpene(row, aliasEndIndex);

            if (!TryParseCannabinoidRow(slicedRow, fieldName, productType, out var parsedField))
                continue;

            acceptedRows++;

            switch (fieldName)
            {
                case "THC":
                    cannabinoids.THC = parsedField;
                    break;
                case "THCA":
                    cannabinoids.THCA = parsedField;
                    break;
                case "CBD":
                    cannabinoids.CBD = parsedField;
                    break;
                case "CBDA":
                    cannabinoids.CBDA = parsedField;
                    break;
                case "D8-THC":
                    delta8 = parsedField.Value;
                    break;
            }
        }

        if (acceptedRows == 0)
            return false;

        cannabinoids.TotalTHC = cannabinoids.THC.Value +
                                (cannabinoids.THCA.Value * 0.877m) +
                                delta8;
        cannabinoids.TotalCBD = cannabinoids.CBD.Value +
                                (cannabinoids.CBDA.Value * 0.877m);

        return true;
    }

    private static bool TryFindLeadingCannabinoid(
        string row,
        out string fieldName,
        out int aliasEndIndex)
    {
        foreach (var anchor in NvCannabinoidAnchors)
        {
            var match = anchor.NameRegex.Match(row);

            if (!match.Success)
                continue;

            fieldName = anchor.FieldName;
            aliasEndIndex = match.Index + match.Length;
            return true;
        }

        fieldName = string.Empty;
        aliasEndIndex = -1;
        return false;
    }

    private static string SliceBeforeFirstTerpene(string row, int searchStartIndex)
    {
        var earliestTerpeneIndex = row.Length;

        foreach (var anchor in NvTerpeneAnchors)
        {
            var match = anchor.NameRegex.Match(row, searchStartIndex);

            if (match.Success && match.Index < earliestTerpeneIndex)
                earliestTerpeneIndex = match.Index;
        }

        return row[..earliestTerpeneIndex].Trim();
    }

    private static bool TryParseCannabinoidRow(
        string row,
        string fieldName,
        ProductType productType,
        out ParsedField<decimal> parsedField)
    {
        parsedField = Empty(fieldName);

        if (!TryFindLeadingCannabinoid(row, out _, out var aliasEndIndex))
            return false;

        var tokens = ResultTokenRegex.Matches(row[aliasEndIndex..])
            .Cast<Match>()
            .Select(match => Regex.Replace(match.Value.Trim(), @"\s+", " "))
            .ToList();

        if (tokens.Count < 3)
            return false;

        var valueToken = ShouldStoreCannabinoidsAsMgPerGram(productType)
            ? tokens[2]
            : tokens[1];

        if (IsNonDetectResultValue(valueToken))
        {
            parsedField = new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = 0m,
                SourceText = row,
                Confidence = 0m
            };

            return true;
        }

        if (!decimal.TryParse(valueToken, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return false;

        var maxValue = ShouldStoreCannabinoidsAsMgPerGram(productType) ? 1000m : 100m;

        if (value < 0m || value > maxValue)
            return false;

        parsedField = new ParsedField<decimal>
        {
            FieldName = fieldName,
            Value = value,
            SourceText = row,
            Confidence = 0.95m
        };

        return true;
    }

    private static bool TryParseNvTotalTerpenes(string text, out decimal totalTerpenes)
    {
        totalTerpenes = 0m;
        var rows = NormalizeRows(text);

        for (var i = 0; i < rows.Count; i++)
        {
            if (!rows[i].Contains("Total Terpenes", StringComparison.OrdinalIgnoreCase))
                continue;

            var start = Math.Max(0, i - 3);

            for (var j = i; j >= start; j--)
            {
                if (TryExtractMgPerGramTotal(rows[j], out totalTerpenes))
                    return true;
            }
        }

        return false;
    }

    private static bool TryParseNvTerpenes(string text, out Dictionary<string, decimal> terpenes)
    {
        terpenes = new Dictionary<string, decimal>();

        foreach (var row in NormalizeRows(text))
        {
            foreach (var anchor in NvTerpeneAnchors)
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

    private static bool TryParseDecimalToken(string raw, out decimal value)
    {
        value = 0m;

        if (IsNonDetectResultValue(raw))
            return false;

        var normalized = Regex.Replace(raw.Trim(), @"\s+", string.Empty);

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static bool ShouldStoreCannabinoidsAsMgPerGram(ProductType productType)
    {
        return productType is not ProductType.Flower and not ProductType.PreRoll;
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

    private static CannabinoidProfile CreateEmptyCannabinoidProfile()
    {
        return new CannabinoidProfile
        {
            THC = Empty("THC"),
            THCA = Empty("THCA"),
            CBD = Empty("CBD"),
            CBDA = Empty("CBDA")
        };
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
}
