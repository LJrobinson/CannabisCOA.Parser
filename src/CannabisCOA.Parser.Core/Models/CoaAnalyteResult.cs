namespace CannabisCOA.Parser.Core.Models;

public sealed class CoaAnalyteResult
{
    public string Name { get; set; } = "";
    public string? NormalizedName { get; set; }

    public decimal? Value { get; set; }
    public string? Unit { get; set; }

    public decimal? MgPerGram { get; set; }
    public decimal? Percent { get; set; }

    public string? Loq { get; set; }
    public string? Lod { get; set; }

    public string? SourceText { get; set; }
}