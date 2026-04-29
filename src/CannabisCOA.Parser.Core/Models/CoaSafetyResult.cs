namespace CannabisCOA.Parser.Core.Models;

public sealed class CoaSafetyResult
{
    public string Category { get; set; } = "";
    public string? AnalyteName { get; set; }

    public string? Status { get; set; }
    public decimal? Value { get; set; }
    public string? Unit { get; set; }

    public decimal? Limit { get; set; }
    public string? LimitUnit { get; set; }

    public string? SourceText { get; set; }
}