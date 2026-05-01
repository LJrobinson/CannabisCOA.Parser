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

    protected override string[] DetectionTerms =>
    [
        "RSR ANALYTICAL",
        "RSR ANALYTICAL LABORATORIES",
        "RSR LABORATORIES"
    ];

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
               !row.Contains(':') &&
               !row.Contains(';') &&
               !row.Contains("@") &&
               !row.Contains("RSR Analytical", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Certificate", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Laughlin", StringComparison.OrdinalIgnoreCase) &&
               !row.Contains("Bruce Woodbury", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Lic.", StringComparison.OrdinalIgnoreCase) &&
               !row.StartsWith("Plant,", StringComparison.OrdinalIgnoreCase) &&
               !Regex.IsMatch(row, @"^\(?\d{3}\)?[\s-]\d{3}[\s-]\d{4}") &&
               !Regex.IsMatch(row, @"^\d+\s+of\s+\d+$", RegexOptions.IgnoreCase);
    }

    private static bool IsRsrFlowerDescriptor(string row)
    {
        return Regex.IsMatch(
            row,
            @"\bPlant\s*,\s*(?:Flower(?:\s*-\s*Cured)?|Trim)\b",
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
