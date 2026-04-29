// src/CannabisCOA.Parser.Core/Parsers/ICoaParser.cs

using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Parsers;

public interface ICoaParser
{
    bool CanParse(string text);
    CoaDocument Parse(string text, string? sourceFileName = null);
}