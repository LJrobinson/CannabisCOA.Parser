using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public static class GenericTerpeneTextParser
{
    private static readonly Dictionary<string, string[]> TerpeneAliases = new()
    {
        ["Beta-Myrcene"] = ["β-Myrcene", "Beta Myrcene", "Beta-Myrcene", "B-Myrcene", "Myrcene"],
        ["Limonene"] = ["D-Limonene", "Limonene"],
        ["Beta-Caryophyllene"] = ["β-Caryophyllene", "Beta Caryophyllene", "Beta-Caryophyllene", "B-Caryophyllene", "Caryophyllene"],
        ["Alpha-Pinene"] = ["α-Pinene", "Alpha Pinene", "Alpha-Pinene", "A-Pinene"],
        ["Beta-Pinene"] = ["β-Pinene", "Beta Pinene", "Beta-Pinene", "B-Pinene"],
        ["Linalool"] = ["Linalool"],
        ["Humulene"] = ["Alpha-Humulene", "α-Humulene", "Humulene"],
        ["Terpinolene"] = ["Terpinolene"],
        ["Ocimene"] = ["Ocimene"],
        ["Bisabolol"] = ["Alpha-Bisabolol", "α-Bisabolol", "Bisabolol"]
    };

    public static TerpeneProfile Parse(string text)
    {
        var profile = new TerpeneProfile();

        ParseTableLines(text, profile);
        RemoveSuspiciousRepeatedValues(profile);

        profile.TotalTerpenes = ExtractTotalTerpenes(text);

        if (profile.TotalTerpenes == 0m && profile.Terpenes.Count > 0)
        {
            profile.TotalTerpenes = profile.Terpenes.Values.Sum();
        }

        if (profile.TotalTerpenes > 25m)
        {
            profile.TotalTerpenes = 0m;
            profile.Terpenes.Clear();
        }

        return profile;
    }

    private static void ParseTableLines(string text, TerpeneProfile profile)
    {
        var lines = text.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.Length > 140)
                continue;

            foreach (var terpene in TerpeneAliases)
            {
                foreach (var alias in terpene.Value)
                {
                    if (!line.Contains(alias, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var value = ExtractReasonableValueNearAlias(line, alias);

                    if (value > 0m)
                    {
                        profile.Terpenes[terpene.Key] = value;
                    }
                }
            }
        }
    }

    private static decimal ExtractReasonableValueNearAlias(string line, string alias)
    {
        var aliasIndex = line.IndexOf(alias, StringComparison.OrdinalIgnoreCase);

        if (aliasIndex < 0)
            return 0m;

        var afterAlias = line.Substring(aliasIndex + alias.Length);

        if (Regex.IsMatch(afterAlias, @"^[A-Za-z]{2,}"))
            return 0m;

        var match = Regex.Match(
            afterAlias,
            @"^[^\d]{0,30}(\d+\.\d+)\s*(%|mg/g)?",
            RegexOptions.IgnoreCase
        );

        if (!match.Success)
            return 0m;

        if (!decimal.TryParse(match.Groups[1].Value, out var value))
            return 0m;

        var unit = match.Groups[2].Value.ToUpperInvariant();

        if (unit == "MG/G")
            value *= 0.1m;

        if (value <= 0m || value > 25m)
            return 0m;

        return value;
    }

    private static void RemoveSuspiciousRepeatedValues(TerpeneProfile profile)
    {
        var repeatedValues = profile.Terpenes
            .GroupBy(t => t.Value)
            .Where(g => g.Count() >= 3)
            .Select(g => g.Key)
            .ToHashSet();

        if (repeatedValues.Count == 0)
            return;

        foreach (var key in profile.Terpenes
            .Where(t => repeatedValues.Contains(t.Value))
            .Select(t => t.Key)
            .ToList())
        {
            profile.Terpenes.Remove(key);
        }
    }

    private static decimal ExtractTotalTerpenes(string text)
    {
        var patterns = new[]
        {
            @"Total\s+Terpenes[^\d]{0,40}(\d+\.\d+)\s*(%|mg/g)?",
            @"Total\s+Terpene\s+Content[^\d]{0,40}(\d+\.\d+)\s*(%|mg/g)?",
            @"Terpenes\s+Total[^\d]{0,40}(\d+\.\d+)\s*(%|mg/g)?"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            if (!decimal.TryParse(match.Groups[1].Value, out var value))
                continue;

            var unit = match.Groups[2].Value.ToUpperInvariant();

            if (unit == "MG/G")
                value *= 0.1m;

            if (value > 0m && value <= 25m)
                return value;
        }

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();

            if (!Regex.IsMatch(line, @"^Total\s+\d+\.\d+\s+\d+\.\d+$", RegexOptions.IgnoreCase))
                continue;

            var match = Regex.Match(line, @"^Total\s+(\d+\.\d+)\s+\d+\.\d+$", RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            if (!decimal.TryParse(match.Groups[1].Value, out var value))
                continue;

            if (value > 0m && value <= 25m)
                return value;
        }

        return 0m;
    }
}