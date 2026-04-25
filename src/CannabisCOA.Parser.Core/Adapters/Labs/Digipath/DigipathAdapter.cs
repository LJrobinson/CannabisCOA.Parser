using CannabisCOA.Parser.Core.Adapters.Interfaces;
using CannabisCOA.Parser.Core.Adapters.Labs.Digipath.ProductParsers;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Digipath;

public class DigipathAdapter : ICoaAdapter
{
    public string LabName => "Digipath";

    public bool CanParse(string text)
    {
        var upper = text.ToUpperInvariant();

        return upper.Contains("DIGIPATH")
            || upper.Contains("DIGIPATH LABS")
            || upper.Contains("DIGIPATH LABORATORIES");
    }

    public ProductType DetectProductType(string text)
    {
        return ProductTypeDetector.Detect(text);
    }

    public CoaResult Parse(string text)
    {
        var productType = DetectProductType(text);

        return productType switch
        {
            ProductType.Flower => DigipathFlowerParser.Parse(text, LabName),
            ProductType.PreRoll => DigipathFlowerParser.Parse(text, LabName), // prerolls usually flower logic first
            _ => DigipathFlowerParser.Parse(text, LabName) // temporary fallback
        };
    }
}