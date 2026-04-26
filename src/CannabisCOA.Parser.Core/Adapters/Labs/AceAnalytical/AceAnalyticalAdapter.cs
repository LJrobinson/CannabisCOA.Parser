using CannabisCOA.Parser.Core.Adapters;

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
}