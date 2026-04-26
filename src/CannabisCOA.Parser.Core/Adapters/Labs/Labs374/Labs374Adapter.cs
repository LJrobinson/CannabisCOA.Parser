using CannabisCOA.Parser.Core.Adapters;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Labs374;

public class Labs374Adapter : BaseLabAdapter
{
    public override string LabName => "374Labs";

    protected override string[] DetectionTerms =>
    [
        "374LABS",
        "374 LABS",
        "374 LABORATORIES"
    ];
}