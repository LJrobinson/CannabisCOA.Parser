using CannabisCOA.Parser.Core.Adapters;

namespace CannabisCOA.Parser.Core.Adapters.Labs.KaychaLabs;

public class KaychaLabsAdapter : BaseLabAdapter
{
    public override string LabName => "Kaycha Labs";

    protected override string[] DetectionTerms =>
    [
        "KAYCHA",
        "KAYCHA LABS",
        "KAYCHA LABORATORIES"
    ];
}