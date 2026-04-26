namespace CannabisCOA.Parser.Core.Parsers;

public static class CoaMetadataParser
{
    public static bool IsAmended(string text)
    {
        var upper = text.ToUpperInvariant();

        return upper.Contains("AMENDED")
            || upper.Contains("REVISED")
            || upper.Contains("CORRECTED")
            || upper.Contains("REVISION");
    }
}