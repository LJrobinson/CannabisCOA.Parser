using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class ComplianceParser
{
    public static ComplianceResult Parse(string text)
    {
        var upper = text.ToUpper();

        if (upper.Contains("FAIL"))
        {
            return new ComplianceResult
            {
                Passed = false,
                Status = "fail",
                ContaminantsPassed = false
            };
        }

        if (upper.Contains("PASS"))
        {
            return new ComplianceResult
            {
                Passed = true,
                Status = "pass",
                ContaminantsPassed = true
            };
        }

        return new ComplianceResult
        {
            Passed = false,
            Status = "unknown",
            ContaminantsPassed = null
        };
    }
}