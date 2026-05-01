using CannabisCOA.Parser.Core.Enums;

namespace CannabisCOA.Parser.Core.Models;

public class CoaResult
{
    public ProductType ProductType { get; set; } = ProductType.Unknown;
    public bool IsAmended { get; set; }

    public string LabName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string BatchId { get; set; } = "";
    public string DocumentClassification { get; set; } = "FullComplianceCoa";
    public bool IsFullComplianceCoa { get; set; } = true;

    public DateTime? HarvestDate { get; set; }
    public DateTime? TestDate { get; set; }
    public DateTime? PackageDate { get; set; }

    public CannabinoidProfile Cannabinoids { get; set; } = new();
    public TerpeneProfile Terpenes { get; set; } = new();
    public ComplianceResult Compliance { get; set; } = new();
    public FreshnessResult Freshness { get; set; } = new();
}
