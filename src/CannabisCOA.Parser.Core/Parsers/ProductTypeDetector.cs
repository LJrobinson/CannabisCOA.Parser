using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Enums;

namespace CannabisCOA.Parser.Core.Parsers;

public static class ProductTypeDetector
{
    public static ProductType Detect(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ProductType.Unknown;

        var rows = NormalizeRows(text);

        // Priority order matters (most specific → least)
        if (MatchAny(rows, PreRollPatterns))
            return ProductType.PreRoll;

        if (MatchAny(rows, VapePatterns))
            return ProductType.Vape;

        if (MatchAny(rows, ConcentratePatterns))
            return ProductType.Concentrate;

        if (MatchAny(rows, EdiblePatterns))
            return ProductType.Edible;

        if (MatchAny(rows, TopicalPatterns))
            return ProductType.Topical;

        if (MatchAny(rows, TincturePatterns))
            return ProductType.Tincture;

        if (MatchAny(rows, FlowerPatterns))
            return ProductType.Flower;

        return ProductType.Unknown;
    }

    // --- Pattern Sets ---

    private static readonly string[] PreRollPatterns =
    [
        @"\bPRE[\s\-]?ROLL\b",
        @"\bPREROLL\b"
    ];

    private static readonly string[] VapePatterns =
    [
        @"\bVAPE\b",
        @"\bCARTRIDGE\b",
        @"\bCART\b"
    ];

    private static readonly string[] ConcentratePatterns =
    [
        @"\bCONCENTRATE\b",
        @"\bSHATTER\b",
        @"\bWAX\b",
        @"\bLIVE RESIN\b",
        @"\bROSIN\b",
        @"\bBHO\b"
    ];

    private static readonly string[] EdiblePatterns =
    [
        @"\bEDIBLE\b",
        @"\bGUMMY\b",
        @"\bCHOCOLATE\b",
        @"\bINFUSED\b"
    ];

    private static readonly string[] TopicalPatterns =
    [
        @"\bTOPICAL\b",
        @"\bBALM\b",
        @"\bCREAM\b",
        @"\bLOTION\b"
    ];

    private static readonly string[] TincturePatterns =
    [
        @"\bTINCTURE\b",
        @"\bSUBLINGUAL\b"
    ];

    private static readonly string[] FlowerPatterns =
    [
        @"\bFLOWER\b",
        @"\bBUD\b",
        @"\bWHOLE FLOWER\b"
    ];

    // --- Helpers ---

    private static bool MatchAny(IEnumerable<string> rows, string[] patterns)
    {
        foreach (var row in rows)
        {
            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(row, pattern, RegexOptions.IgnoreCase))
                    return true;
            }
        }

        return false;
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