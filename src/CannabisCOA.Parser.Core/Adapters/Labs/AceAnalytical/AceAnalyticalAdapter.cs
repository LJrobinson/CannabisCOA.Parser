using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Adapters.Generic;
using CannabisCOA.Parser.Core.Adapters.Labs.AceAnalytical.ProductParsers;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Labs.AceAnalytical;

public class AceAnalyticalAdapter : BaseLabAdapter
{
    public override string LabName => "Ace Analytical Laboratory";

    protected override string[] DetectionTerms =>
    [
        "ACE ANALYTICAL LABORATORY",
        "ACE ANALYTICAL LABS",
        "LIC# 91781014075623623744",
        "7151 CASCADE VALLEY CT"
    ];

    public override CoaResult Parse(string text)
    {
        if (!CanParse(text))
            return new GenericCoaAdapter().Parse(text);

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
