namespace CannabisCOA.Parser.Core.Models;

public class ComplianceResult
{
    public bool Passed { get; set; }
    public bool? ContaminantsPassed { get; set; }
    public string Status { get; set; } = "unknown";
}