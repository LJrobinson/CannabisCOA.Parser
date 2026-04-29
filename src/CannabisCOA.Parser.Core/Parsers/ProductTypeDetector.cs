using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Enums;

namespace CannabisCOA.Parser.Core.Parsers;

public static class ProductTypeDetector
{
    private static readonly string[] StrongContextPatterns =
    [
        @"^\s*PRODUCT\s*TYPE\b",
        @"^\s*SAMPLE\s*TYPE\b",
        @"^\s*MATRIX\b",
        @"^\s*CATEGORY\b",
        @"\bPLANT\s*,?\s*FLOWER\b",
        @"\bFLOWER\s*-\s*CURED\b"
    ];

    public static ProductType Detect(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ProductType.Unknown;

        var rows = NormalizeRows(text);

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

        if (MatchAny(rows, FlowerPatterns, requireContext: true))
            return ProductType.Flower;

        return ProductType.Unknown;
    }

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
        @"\bEDIBLES\b",
        @"\bGUMMY\b",
        @"\bGUMMIES\b",
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

    private static bool MatchAny(IEnumerable<string> rows, string[] patterns, bool requireContext = false)
    {
        foreach (var row in rows)
        {
            if (requireContext && !HasStrongContext(row))
                continue;

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(row, pattern, RegexOptions.IgnoreCase))
                    return true;
            }
        }

        return false;
    }

    private static bool HasStrongContext(string row)
    {
        return StrongContextPatterns.Any(pattern =>
            Regex.IsMatch(row, pattern, RegexOptions.IgnoreCase));
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
