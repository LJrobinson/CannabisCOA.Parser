using CannabisCOA.Parser.Core.Adapters;

namespace CannabisCOA.Parser.Core.Adapters.Labs.G3Labs;

public class G3LabsAdapter : BaseLabAdapter
{
    public override string LabName => "G3Labs";

    protected override string[] DetectionTerms =>
    [
        "G3LABS",
        "G3 LABS",
        "G3 LABORATORIES"
    ];
}