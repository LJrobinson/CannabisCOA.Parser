using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Adapters.Labs.AceAnalytical.ProductParsers;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.AceAnalytical;

public class AceAnalyticalAdapter : BaseLabAdapter
{
    public override string LabName => "Ace Analytical Laboratory";

    protected override string[] DetectionTerms =>
    [
        "ACE ANALYTICAL",
        "ACE ANALYTICAL LABORATORY",
        "ACE ANALYTICAL LAB"
    ];

    public override CoaResult Parse(string text)
    {
        var productType = DetectProductType(text);

        return productType switch
        {
            ProductType.Flower => AceFlowerParser.Parse(text, LabName),
            ProductType.PreRoll => AceFlowerParser.Parse(text, LabName),
            ProductType.Unknown => AceFlowerParser.Parse(text, LabName),
            _ => base.Parse(text)
        };
    }
}
