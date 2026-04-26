using CannabisCOA.Parser.Core.Adapters;

namespace CannabisCOA.Parser.Core.Adapters.Labs.MAAnalytics;

public class MAAnalyticsAdapter : BaseLabAdapter
{
    public override string LabName => "MA Analytics";

    protected override string[] DetectionTerms =>
    [
        "MA ANALYTICS",
        "M.A. ANALYTICS"
    ];
}