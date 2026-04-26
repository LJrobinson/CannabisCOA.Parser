using CannabisCOA.Parser.Core.Adapters.Interfaces;
using CannabisCOA.Parser.Core.Adapters.Generic;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters;

public abstract class BaseLabAdapter : ICoaAdapter
{
    public abstract string LabName { get; }
    protected abstract string[] DetectionTerms { get; }

    public virtual bool CanParse(string text)
    {
        var upper = text.ToUpperInvariant();

        return DetectionTerms.Any(term =>
            upper.Contains(term.ToUpperInvariant()));
    }

    public virtual ProductType DetectProductType(string text)
    {
        return ProductTypeDetector.Detect(text);
    }

    public virtual CoaResult Parse(string text)
    {
        var generic = new GenericCoaAdapter();
        var result = generic.Parse(text);

        result.LabName = LabName;
        result.ProductType = DetectProductType(text);

        return result;
    }

    public virtual int MatchScore(string text)
    {
        var upper = text.ToUpperInvariant();
        return DetectionTerms.Count(t => upper.Contains(t.ToUpperInvariant()));
    }
}