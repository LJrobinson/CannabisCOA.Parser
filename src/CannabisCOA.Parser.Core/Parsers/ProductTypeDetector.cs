using CannabisCOA.Parser.Core.Enums;

namespace CannabisCOA.Parser.Core.Parsers;

public static class ProductTypeDetector
{
    public static ProductType Detect(string text)
    {
        var upper = text.ToUpper();

        if (upper.Contains("FLOWER") || upper.Contains("BUD"))
            return ProductType.Flower;

        if (upper.Contains("PREROLL") || upper.Contains("PRE-ROLL"))
            return ProductType.PreRoll;

        if (upper.Contains("VAPE") || upper.Contains("CARTRIDGE"))
            return ProductType.Vape;

        if (upper.Contains("CONCENTRATE") || upper.Contains("SHATTER") || upper.Contains("WAX"))
            return ProductType.Concentrate;

        if (upper.Contains("EDIBLE") || upper.Contains("GUMMY") || upper.Contains("CHOCOLATE"))
            return ProductType.Edible;

        if (upper.Contains("TOPICAL"))
            return ProductType.Topical;

        if (upper.Contains("TINCTURE"))
            return ProductType.Tincture;

        return ProductType.Unknown;
    }
}