using CannabisCOA.Parser.Core.Adapters;

namespace CannabisCOA.Parser.Core.Adapters.Labs.RSRAnalytical;

public class RSRAnalyticalAdapter : BaseLabAdapter
{
    public override string LabName => "RSR Analytical Laboratories";

    protected override string[] DetectionTerms =>
    [
        "RSR ANALYTICAL",
        "RSR ANALYTICAL LABORATORIES",
        "RSR LABORATORIES"
    ];
}