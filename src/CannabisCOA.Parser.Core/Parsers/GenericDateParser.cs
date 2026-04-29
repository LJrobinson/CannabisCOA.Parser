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
        "REPORT DATE",
        "REPORT CREATED",
        "REPORT GENERATED",
        "DATE REPORTED",
        "REPORTED",
        "COMPLETED",
        "DATE COMPLETED"
    ];

    private static readonly string[] HarvestDateLabels =
    [
        "HARVEST/PRODUCTION DATE",
        "HARVEST DATE",
        "DATE HARVESTED",
        "HARVESTED"
    ];

    private static readonly string[] PackageDateLabels =
    [
        "PACKAGE DATE",
        "PACKAGED DATE",
        "DATE PACKAGED",
        "PACKAGED"
    ];

    private static readonly Regex DateRegex = new(
        @"(?<date>
            \b\d{1,2}/\d{1,2}/\d{2,4}\b |
            \b\d{4}-\d{2}-\d{2}\b |
            \b[A-Za-z]+\s+\d{1,2},\s+\d{4}\b
        )",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    public static DateTime? ExtractTestDate(string text)
    {
        return ExtractLabeledDate(text, TestDateLabels, allowFallbackTestLanguage: true);
    }

    public static DateTime? ExtractHarvestDate(string text)
    {
        return ExtractLabeledDate(text, HarvestDateLabels, allowFallbackTestLanguage: false);
    }

    public static DateTime? ExtractPackageDate(string text)
    {
        return ExtractLabeledDate(text, PackageDateLabels, allowFallbackTestLanguage: false);
    }

    private static DateTime? ExtractLabeledDate(
        string text,
        string[] labels,
        bool allowFallbackTestLanguage)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var rows = NormalizeRows(text);

        foreach (var row in rows)
        {
            var date = ExtractDateAfterFirstMatchingLabel(row, labels);
            if (date != null)
                return date;
        }

        if (!allowFallbackTestLanguage)
            return null;

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

    private static DateTime? ExtractDateAfterFirstMatchingLabel(string row, string[] labels)
    {
        foreach (var label in labels.OrderByDescending(label => label.Length))
        {
            var index = row.IndexOf(label, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                continue;

            var afterLabel = row[(index + label.Length)..];

            var date = ExtractDateFromRow(afterLabel);
            if (date != null)
                return date;
        }

        return null;
    }

    private static DateTime? ExtractDateFromRow(string row)
    {
        var match = DateRegex.Match(row);
        if (!match.Success)
            return null;

        var raw = match.Groups["date"].Value;

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed.Date;

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
