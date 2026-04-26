using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class ComplianceParser
{
    private static readonly string[] ExplicitStatusLabels =
    [
        "OVERALL RESULT",
        "FINAL RESULT",
        "OVERALL STATUS",
        "COMPLIANCE STATUS",
        "RESULT STATUS",
        "STATUS"
    ];

    public static ComplianceResult Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Unknown();

        var rows = NormalizeRows(text);

        // 1. Look for explicit overall result rows
        foreach (var row in rows)
        {
            var upper = row.ToUpperInvariant();

            if (!ContainsExplicitLabel(upper))
                continue;

            if (TryExtractStatus(upper, out var status))
                return status;
        }

        // 2. Secondary: look for strong standalone phrases (NOT analyte rows)
        foreach (var row in rows)
        {
            var upper = row.ToUpperInvariant();

            if (LooksLikeAnalyteRow(upper))
                continue;

            if (TryExtractStandaloneStatus(upper, out var status))
                return status;
        }

        // 3. Default: unknown (never guess)
        return Unknown();
    }

    private static bool ContainsExplicitLabel(string row)
    {
        return ExplicitStatusLabels.Any(label => row.Contains(label));
    }

    private static bool TryExtractStatus(string row, out ComplianceResult result)
    {
        if (row.Contains("PASS"))
        {
            result = Passed(row);
            return true;
        }

        if (row.Contains("FAIL"))
        {
            result = Failed(row);
            return true;
        }

        result = Unknown();
        return false;
    }

    private static bool TryExtractStandaloneStatus(string row, out ComplianceResult result)
    {
        // Accept ONLY very clean standalone lines
        if (Regex.IsMatch(row, @"^\s*(PASS|PASSED)\s*$"))
        {
            result = Passed(row);
            return true;
        }

        if (Regex.IsMatch(row, @"^\s*(FAIL|FAILED)\s*$"))
        {
            result = Failed(row);
            return true;
        }

        result = Unknown();
        return false;
    }

    private static bool LooksLikeAnalyteRow(string row)
    {
        // Prevent things like:
        // "Salmonella PASS"
        // "Residual Solvents: PASS"
        // "Pesticides - PASS"

        return Regex.IsMatch(row, @":\s*(PASS|FAIL|PASSED|FAILED)") ||
               Regex.IsMatch(row, @"\b(PPM|CFU|LOD|LOQ|ACTION LIMIT)\b");
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

    private static ComplianceResult Passed(string source) => new()
    {
        Passed = true,
        Status = "pass",
        ContaminantsPassed = true
    };

    private static ComplianceResult Failed(string source) => new()
    {
        Passed = false,
        Status = "fail",
        ContaminantsPassed = false
    };

    private static ComplianceResult Unknown() => new()
    {
        Passed = false,
        Status = "unknown",
        ContaminantsPassed = null
    };
}