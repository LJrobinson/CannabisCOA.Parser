using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class ComplianceParser
{
    public static ComplianceResult Parse(string text)
    {
        var upper = text.ToUpperInvariant();

        var overallPatterns = new[]
        {
            @"OVERALL\s+RESULT[^\w]*(PASS|PASSED|FAIL|FAILED)",
            @"FINAL\s+RESULT[^\w]*(PASS|PASSED|FAIL|FAILED)",
            @"RESULT[^\w]*(PASS|PASSED|FAIL|FAILED)",
            @"STATUS[^\w]*(PASS|PASSED|FAIL|FAILED)"
        };

        foreach (var pattern in overallPatterns)
        {
            var match = Regex.Match(upper, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var value = match.Groups[1].Value;

            if (value.StartsWith("PASS"))
                return Passed();

            if (value.StartsWith("FAIL"))
                return Failed();
        }

        if (Regex.IsMatch(upper, @"\bPASS(ED)?\b"))
            return Passed();

        if (Regex.IsMatch(upper, @"\bFAIL(ED)?\b"))
            return Failed();

        return new ComplianceResult
        {
            Passed = false,
            Status = "unknown",
            ContaminantsPassed = null
        };
    }

    private static ComplianceResult Passed() => new()
    {
        Passed = true,
        Status = "pass",
        ContaminantsPassed = true
    };

    private static ComplianceResult Failed() => new()
    {
        Passed = false,
        Status = "fail",
        ContaminantsPassed = false
    };
}