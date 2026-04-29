using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.NVCannLabs;

public class NVCannLabsAdapter : BaseLabAdapter
{
    public override string LabName => "NV Cann Labs";

    private static readonly Regex MgPerGramRegex = new(
        @"(?<value>\d{1,6}(?:\.\d+)?|\.\d+)\s*mg\s*/\s*g",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

        if (TryParseNvTotalTerpenes(text, out var totalTerpenes))
            result.Terpenes.TotalTerpenes = totalTerpenes;

        return result;
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
