using CannabisCOA.Parser.Core.Adapters.Interfaces;
using CannabisCOA.Parser.Core.Adapters.Generic;

namespace CannabisCOA.Parser.Core.Adapters;

public static class AdapterResolver
{
    private static readonly List<ICoaAdapter> Adapters = new()
    {
        // Future lab adapters go here
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