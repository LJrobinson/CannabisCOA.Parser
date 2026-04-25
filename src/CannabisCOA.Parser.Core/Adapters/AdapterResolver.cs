using CannabisCOA.Parser.Core.Adapters.Generic;
using CannabisCOA.Parser.Core.Adapters.Interfaces;
using CannabisCOA.Parser.Core.Adapters.Labs.Digipath;

namespace CannabisCOA.Parser.Core.Adapters;

public static class AdapterResolver
{
    private static readonly List<ICoaAdapter> Adapters = new()
    {
        new DigipathAdapter(),
        new GenericCoaAdapter()
    };

    public static ICoaAdapter Resolve(string text)
    {
        foreach (var adapter in Adapters)
        {
            if (adapter.CanParse(text))
                return adapter;
        }

        return new GenericCoaAdapter();
    }
}