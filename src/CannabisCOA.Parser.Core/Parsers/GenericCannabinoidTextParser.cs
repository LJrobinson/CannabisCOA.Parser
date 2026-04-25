using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class GenericCannabinoidTextParser
{
    public static CannabinoidProfile Parse(string text)
    {
        var profile = new CannabinoidProfile();

        profile.THC = ExtractValue(text, @"THC\s*[:\-]?\s*(\d+\.?\d*)");
        profile.THCA = ExtractValue(text, @"THCA\s*[:\-]?\s*(\d+\.?\d*)");
        profile.CBD = ExtractValue(text, @"CBD\s*[:\-]?\s*(\d+\.?\d*)");
        profile.CBDA = ExtractValue(text, @"CBDA\s*[:\-]?\s*(\d+\.?\d*)");

        return profile;
    }

    private static decimal ExtractValue(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (!match.Success)
            return 0m;

        if (decimal.TryParse(match.Groups[1].Value, out var value))
            return value;

        return 0m;
    }
}