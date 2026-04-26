using CannabisCOA.Parser.Core.Adapters.Generic;
using CannabisCOA.Parser.Core.Adapters.Interfaces;
using CannabisCOA.Parser.Core.Adapters.Labs.AceAnalytical;
using CannabisCOA.Parser.Core.Adapters.Labs.Digipath;
using CannabisCOA.Parser.Core.Adapters.Labs.G3Labs;
using CannabisCOA.Parser.Core.Adapters.Labs.KaychaLabs;
using CannabisCOA.Parser.Core.Adapters.Labs.Labs374;
using CannabisCOA.Parser.Core.Adapters.Labs.MAAnalytics;
using CannabisCOA.Parser.Core.Adapters.Labs.NVCannLabs;
using CannabisCOA.Parser.Core.Adapters.Labs.RSRAnalytical;

namespace CannabisCOA.Parser.Core.Adapters;

public static class AdapterResolver
{
    private static readonly List<ICoaAdapter> Adapters = new()
    {
        new Labs374Adapter(),
        new G3LabsAdapter(),
        new NVCannLabsAdapter(),
        new AceAnalyticalAdapter(),
        new KaychaLabsAdapter(),
        new DigipathAdapter(),
        new MAAnalyticsAdapter(),
        new RSRAnalyticalAdapter()
    };

    private static readonly GenericCoaAdapter GenericAdapter = new();

    public static ICoaAdapter Resolve(string text)
    {
        var matches = Adapters
            .Where(adapter => adapter.CanParse(text))
            .ToList();

        if (matches.Count > 0)
            return matches[0];

        return GenericAdapter;
    }
}