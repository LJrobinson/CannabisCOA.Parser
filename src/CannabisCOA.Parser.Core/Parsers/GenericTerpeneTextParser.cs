using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class GenericTerpeneTextParser
{
    private static readonly Dictionary<string, string[]> TerpeneAliases = new()
    {
        ["Beta-Myrcene"] = ["β-Myrcene", "Beta Myrcene", "B-Myrcene", "Myrcene"],
        ["Limonene"] = ["D-Limonene", "Limonene"],
        ["Beta-Caryophyllene"] = ["β-Caryophyllene", "Beta Caryophyllene", "B-Caryophyllene", "Caryophyllene"],
        ["Alpha-Pinene"] = ["α-Pinene", "Alpha Pinene", "A-Pinene"],
        ["Beta-Pinene"] = ["β-Pinene", "Beta Pinene", "B-Pinene"],
        ["Linalool"] = ["Linalool"],
        ["Humulene"] = ["Alpha-Humulene", "α-Humulene", "Humulene"],
        ["Terpinolene"] = ["Terpinolene"],
        ["Ocimene"] = ["Ocimene"],
        ["Bisabolol"] = ["Alpha-Bisabolol", "α-Bisabolol", "Bisabolol"]
    };

    public static TerpeneProfile Parse(string text)
    {
        var profile = new TerpeneProfile();

        foreach (var terpene in TerpeneAliases)
        {
            var value = ExtractFirstMatch(text, terpene.Value);

            if (value > 0)
            {
                profile.Terpenes[terpene.Key] = value;
            }
        }

        ParseTableLines(text, profile);

        profile.TotalTerpenes = ExtractTotalTerpenes(text);

        if (profile.TotalTerpenes == 0m && profile.Terpenes.Count > 0)
        {
            profile.TotalTerpenes = profile.Terpenes.Values.Sum();
        }

        return profile;
    }

    private static decimal ExtractFirstMatch(string text, string[] aliases)
    {
        foreach (var alias in aliases)
        {
            var escaped = Regex.Escape(alias);
            var pattern = $@"{escaped}[^\d]*(\d+\.?\d*)\s*(%|MG\/G)?";

            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                if (decimal.TryParse(match.Groups[1].Value, out var value))
                {
                    var unit = match.Groups[2].Value.ToUpper();

                    if (unit == "MG/G")
                        value *= 0.1m;

                    return value;
                }
            }
        }

        return 0m;
    }

    private static decimal ExtractTotalTerpenes(string text)
    {
        var patterns = new[]
        {
            @"Total\s+Terpenes[^\d]*(\d+\.?\d*)",
            @"Total\s+Terpene\s+Content[^\d]*(\d+\.?\d*)",
            @"Terpenes\s+Total[^\d]*(\d+\.?\d*)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

            if (match.Success && decimal.TryParse(match.Groups[1].Value, out var value))
                return value;
        }

        return 0m;
    }

    private static void ParseTableLines(string text, TerpeneProfile profile)
    {
        var lines = text.Split('\n');

        foreach (var line in lines)
        {
            var cleaned = line.Trim();

            if (string.IsNullOrWhiteSpace(cleaned))
                continue;

            foreach (var terpene in TerpeneAliases)
            {
                foreach (var alias in terpene.Value)
                {
                    if (!cleaned.ToUpper().Contains(alias.ToUpper()))
                        continue;

                    var match = Regex.Match(cleaned, @"(\d+\.?\d*)\s*(%|MG\/G)?", RegexOptions.IgnoreCase);

                    if (!match.Success)
                        continue;

                    if (!decimal.TryParse(match.Groups[1].Value, out var value))
                        continue;

                    var unit = match.Groups[2].Value.ToUpper();

                    if (unit == "MG/G")
                        value *= 0.1m;

                    profile.Terpenes[terpene.Key] = value;
                }
            }
        }
    }
}