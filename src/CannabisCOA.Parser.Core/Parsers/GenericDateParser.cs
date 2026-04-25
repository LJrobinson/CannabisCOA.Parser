using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class GenericDateParser
{
    public static DateTime? ExtractTestDate(string text)
    {
        var patterns = new[]
        {
            @"Test\s*Date\s*[:\-]?\s*([0-9]{1,2}/[0-9]{1,2}/[0-9]{2,4})",
            @"Date\s*Tested\s*[:\-]?\s*([0-9]{1,2}/[0-9]{1,2}/[0-9]{2,4})"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out var date))
                    return date;
            }
        }

        return null;
    }
}