using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.RSRAnalytical;

public class RSRAnalyticalAdapter : BaseLabAdapter
{
    public override string LabName => "RSR Analytical Laboratories";

    private static readonly Regex TerpeneTotalRegex = new(
        @"\bTotal\s+(?<percent>\d{1,2}\.\d{3,})\s+(?<mg>\d{1,3}\.\d+)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TerpeneResultQuadRegex = new(
        @"^\s*(?<loqPercent>\d{1,2}\.\d+)\s+(?<loqMg>\d{1,2}\.\d+)\s+(?<percent><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,2}\.\d+)\s+(?<mg><\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,3}\.\d+)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly (string CanonicalName, Regex NameRegex)[] RsrTerpeneAnchors =
    [
        ("β-Myrcene", new Regex(@"(?<![\p{L}\p{N}])β\s*-\s*Myrcene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Pinene", new Regex(@"(?<![\p{L}\p{N}])α\s*-\s*Pinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Pinene", new Regex(@"(?<![\p{L}\p{N}])β\s*-\s*Pinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("β-Caryophyllene", new Regex(@"(?<![\p{L}\p{N}])β\s*-\s*Caryophyllene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Humulene", new Regex(@"(?<![\p{L}\p{N}])α\s*-\s*Humulene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("δ-Limonene", new Regex(@"(?<![\p{L}\p{N}])δ\s*-\s*Limonene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Linalool", new Regex(@"(?<![\p{L}\p{N}])Linalool(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Terpinolene", new Regex(@"(?<![\p{L}\p{N}])Terpinolene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Bisabolol", new Regex(@"(?<![\p{L}\p{N}])α\s*-\s*Bisabolol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Caryophyllene Oxide", new Regex(@"(?<![\p{L}\p{N}])Caryophyllene\s+Oxide(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("α-Terpinene", new Regex(@"(?<![\p{L}\p{N}])α\s*-\s*Terpinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Camphene", new Regex(@"(?<![\p{L}\p{N}])Camphene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("cis-Nerolidol", new Regex(@"(?<![\p{L}\p{N}])cis\s*-\s*Nerolidol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("trans-Nerolidol", new Regex(@"(?<![\p{L}\p{N}])trans\s*-\s*Nerolidol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("δ-3-Carene", new Regex(@"(?<![\p{L}\p{N}])δ\s*-\s*3\s*-\s*Carene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Eucalyptol", new Regex(@"(?<![\p{L}\p{N}])Eucalyptol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("γ-Terpinene", new Regex(@"(?<![\p{L}\p{N}])γ\s*-\s*Terpinene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Guaiol", new Regex(@"(?<![\p{L}\p{N}])Guaiol(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Ocimene", new Regex(@"(?<![\p{L}\p{N}-])Ocimene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("p-Cymene", new Regex(@"(?<![\p{L}\p{N}])p\s*-\s*Cymene(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.Compiled))
    ];

    protected override string[] DetectionTerms =>
    [
        "RSR ANALYTICAL",
        "RSR ANALYTICAL LABORATORIES",
        "RSR LABORATORIES"
    ];

    public override int MatchScore(string text)
    {
        var score = base.MatchScore(text);

        if (score == 0)
            return 0;

        if (text.Contains("http://www.rsrlabs.com", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("www.rsrlabs.com", StringComparison.OrdinalIgnoreCase))
        {
            score += 3;
        }

        if (text.Contains("241 W Charleston Blvd", StringComparison.OrdinalIgnoreCase))
            score += 2;

        if (Regex.IsMatch(text, @"tested\s+by\s+RSR\s+Analytical\s+Laboratories", RegexOptions.IgnoreCase))
            score += 2;

        if (text.Contains("Haifei Yin", StringComparison.OrdinalIgnoreCase))
            score += 1;

        if (Regex.IsMatch(text, @"\bRSR-SOP-\d{3}\b", RegexOptions.IgnoreCase))
            score += 1;

        return score;
    }

    public override ProductType DetectProductType(string text)
    {
        return NormalizeRows(text).Any(IsRsrFlowerDescriptor)
            ? ProductType.Flower
            : base.DetectProductType(text);
    }

    public override CoaResult Parse(string text)
    {
        var result = base.Parse(text);

        if (result.ProductType == ProductType.Flower)
        {
            result.ProductName = ExtractProductName(text);
            result.BatchId = ExtractBatchId(text);
        }

        if (TryParseRsrTotalTerpenes(text, out var totalTerpenes))
            result.Terpenes.TotalTerpenes = totalTerpenes;

        if (TryParseRsrTerpenes(text, out var terpenes))
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
            if (!IsRsrFlowerDescriptor(rows[i]))
                continue;

            for (var j = i - 1; j >= 0 && i - j <= 4; j--)
            {
                var candidate = rows[j].Trim();

                if (IsRsrProductNameCandidate(candidate))
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
                @"\bStrain\s*:\s*(?<strain>.+?)(?:\s+Batch\s*#:|\s+Lot\s*#:|$)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var strain = match.Groups["strain"].Value.Trim();

            if (IsRsrProductNameCandidate(strain))
                return strain;
        }

        return string.Empty;
    }

    private static string ExtractBatchId(string text)
    {
        var rows = NormalizeRows(text);
        var batchId = ExtractBatchId(rows);

        if (!string.IsNullOrWhiteSpace(batchId))
            return batchId;

        return ExtractLotId(rows);
    }

    private static string ExtractBatchId(IEnumerable<string> rows)
    {
        foreach (var row in rows)
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

    private static string ExtractLotId(IEnumerable<string> rows)
    {
        foreach (var row in rows)
        {
            if (row.Contains("Harvest Process Lot", StringComparison.OrdinalIgnoreCase))
                continue;

            var match = Regex.Match(
                row,
                @"\bLot\s*#\s*:\s*(?<lot>.*?)(?:\s+Sample\s+Received:|\s+Report\s+Created:|$)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var lot = match.Groups["lot"].Value.Trim();

            if (!string.IsNullOrWhiteSpace(lot))
                return lot;
        }

        return string.Empty;
    }

    private static bool IsRsrProductNameCandidate(string row)
    {
        return !string.IsNullOrWhiteSpace(row) &&
               !Regex.IsMatch(row, @"^[\s\-_]+$") &&
               !row.Equals("Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Plant", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Trim", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Bulk", StringComparison.OrdinalIgnoreCase) &&
               !row.Equals("Bulk Flower", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains(':') &&
               !row.Contains(';') &&
               !row.Contains("@") &&
               !row.Contains("RSR Analytical", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Certificate", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Confident LIMS", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Laughlin", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Bruce Woodbury", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Lic.", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Plant,", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Batch", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Lot", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("METRC", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Harvest Process", StringComparison.OrdinalIgnoreCase) &&
               !Regex.IsMatch(row, @"^1A[0-9A-Z]{16,}$", RegexOptions.IgnoreCase) &&
               !Regex.IsMatch(row, @"^\(?\d{3}\)?[\s-]\d{3}[\s-]\d{4}") &&
               !Regex.IsMatch(row, @"^\d+\s+of\s+\d+$", RegexOptions.IgnoreCase);
    }

    private static bool IsRsrFlowerDescriptor(string row)
    {
        return Regex.IsMatch(
            row,
            @"\bPlant\s*,\s*(?:Flower(?:\s*-\s*Cured)?|Trim|Bulk\s+Flower\s*,\s*Indoor)\b",
            RegexOptions.IgnoreCase);
    }

    private static bool TryParseRsrTotalTerpenes(string text, out decimal totalTerpenes)
    {
        totalTerpenes = 0m;

        foreach (Match match in TerpeneTotalRegex.Matches(text))
        {
            if (!decimal.TryParse(match.Groups["percent"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var percent) ||
                !decimal.TryParse(match.Groups["mg"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var mgPerGram))
            {
                continue;
            }

            if (percent > 0m && percent <= 25m && Math.Abs((percent * 10m) - mgPerGram) <= 0.01m)
            {
                totalTerpenes = percent;
                return true;
            }
        }

        return false;
    }

    private static bool TryParseRsrTerpenes(string text, out Dictionary<string, decimal> terpenes)
    {
        terpenes = new Dictionary<string, decimal>();

        foreach (var row in NormalizeRows(text))
        {
            foreach (var anchor in RsrTerpeneAnchors)
            {
                var nameMatch = anchor.NameRegex.Match(row);

                if (!nameMatch.Success)
                    continue;

                var afterName = row[(nameMatch.Index + nameMatch.Length)..];
                var valueMatch = TerpeneResultQuadRegex.Match(afterName);

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

        return terpenes.Count > 0;
    }

    private static bool TryParseTerpeneResult(string rawPercent, string rawMgPerGram, out decimal percent)
    {
        percent = 0m;

        if (IsNonDetectResult(rawPercent))
            return false;

        if (!decimal.TryParse(rawPercent, NumberStyles.Number, CultureInfo.InvariantCulture, out percent) ||
            !decimal.TryParse(rawMgPerGram, NumberStyles.Number, CultureInfo.InvariantCulture, out var mgPerGram))
        {
            return false;
        }

        if (percent <= 0m || percent > 25m || mgPerGram <= 0m)
            return false;

        return Math.Abs((percent * 10m) - mgPerGram) <= 0.01m;
    }

    private static bool IsNonDetectResult(string raw)
    {
        var normalized = Regex.Replace(raw.Trim(), @"\s+", string.Empty);

        return normalized.StartsWith("<", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("NT", StringComparison.OrdinalIgnoreCase);
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
