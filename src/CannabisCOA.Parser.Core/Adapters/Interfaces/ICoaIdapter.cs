using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Adapters.Interfaces;

public interface ICoaAdapter
{
    string LabName { get; }

    bool CanParse(string text);

    ProductType DetectProductType(string text);

    CoaResult Parse(string text);
}