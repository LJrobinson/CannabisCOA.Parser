using CannabisCOA.Parser.Core.Adapters.Labs.Digipath.ProductParsers;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Digipath;

public class DigipathAdapter : BaseLabAdapter
{
    public override string LabName => "Digipath";

    protected override string[] DetectionTerms =>
    [
        "DIGIPATH LABS",
        "DIGIPATH LABORATORIES",
        "DIGIPATH"
    ];

    public override ProductType DetectProductType(string text)
    {
        return ProductTypeDetector.Detect(text);
    }

    public override int MatchScore(string text)
    {
        var score = base.MatchScore(text);

        return score == 0 ? 0 : score + 1;
    }

    public override CoaResult Parse(string text)
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
