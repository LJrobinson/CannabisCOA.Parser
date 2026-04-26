using CannabisCOA.Parser.Core.Adapters;

namespace CannabisCOA.Parser.Core.Adapters.Labs.NVCannLabs;

public class NVCannLabsAdapter : BaseLabAdapter
{
    public override string LabName => "NV CannLabs";

    protected override string[] DetectionTerms =>
    [
        "NV CANNLABS",
        "NV CANN LABS",
        "NEVADA CANNLABS",
        "NEVADA CANN LABS"
    ];
}