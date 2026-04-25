using CannabisCOA.Parser.Core.Adapters;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core;

public static class CoaParser
{
    public static CoaResult Parse(string text)
    {
        var adapter = AdapterResolver.Resolve(text);
        return adapter.Parse(text);
    }
}