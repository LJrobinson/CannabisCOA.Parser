using System.Globalization;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Adapters;
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

    public override CoaResult Parse(string text)
    {
        var result = base.Parse(text);

        if (TryParseRsrTotalTerpenes(text, out var totalTerpenes))
            result.Terpenes.TotalTerpenes = totalTerpenes;

        return result;
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
}
