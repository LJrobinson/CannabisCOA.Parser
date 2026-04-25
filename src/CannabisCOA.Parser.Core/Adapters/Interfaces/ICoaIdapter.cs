using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Interfaces;

public interface ICoaAdapter
{
    bool CanParse(string text);

    CoaResult Parse(string text);

    string LabName { get; }
}