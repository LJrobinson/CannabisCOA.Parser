namespace CannabisCOA.Parser.Core.Models;

public sealed class CoaParserMetadata
{
    public string? SourceFileName { get; set; }
    public string? DetectedLab { get; set; }
    public string? ParserName { get; set; }
    public decimal ConfidenceScore { get; set; }

    public List<string> Warnings { get; set; } = [];
    public List<string> MissingFields { get; set; } = [];
}