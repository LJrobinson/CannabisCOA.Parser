using System.Globalization;
using System.Text.RegularExpressions;

namespace CannabisCOA.Parser.Core.Parsers;

public static class GenericDateParser
{
    private static readonly string[] TestDateLabels =
    [
        "TEST DATE",
        "DATE TESTED",
        "ANALYSIS DATE",
        "DATE OF ANALYSIS",
        "REPORTED",
        "DATE REPORTED",
        "COMPLETED",
        "DATE COMPLETED"
    ];

    private static readonly Regex DateRegex = new(
        @"(?<date>
            \b\d{1,2}/\d{1,2}/\d{2,4}\b |          # 04/20/2024
            \b\d{4}-\d{2}-\d{2}\b |               # 2024-04-20
            \b[A-Za-z]+\s+\d{1,2},\s+\d{4}\b     # April 20, 2024
        )",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    public static DateTime? ExtractTestDate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var rows = NormalizeRows(text);

        // 1. Look for label + nearby date
        foreach (var row in rows)
        {
            var upper = row.ToUpperInvariant();

            if (!ContainsTestLabel(upper))
                continue;

            var date = ExtractDateFromRow(row);
            if (date != null)
                return date;
        }

        // 2. Fallback: scan for date near "tested" language
        foreach (var row in rows)
        {
            var upper = row.ToUpperInvariant();

            if (!upper.Contains("TEST"))
                continue;

            var date = ExtractDateFromRow(row);
            if (date != null)
                return date;
        }

        return null;
    }

    private static bool ContainsTestLabel(string row)
    {
        return TestDateLabels.Any(label => row.Contains(label));
    }

    private static DateTime? ExtractDateFromRow(string row)
    {
        var match = DateRegex.Match(row);
        if (!match.Success)
            return null;

        var raw = match.Groups["date"].Value;

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;

        return null;
    }

    private static List<string> NormalizeRows(string text)
    {
        return text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(r => Regex.Replace(r.Trim(), @"\s+", " "))
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToList();
    }
}