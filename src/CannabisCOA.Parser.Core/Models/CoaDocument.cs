namespace CannabisCOA.Parser.Core.Models;

public sealed class CoaDocument
{
    public string SchemaVersion { get; set; } = "flower-coa-v1";

    public string? LabName { get; set; }
    public string? LabLicenseNumber { get; set; }

    public string? ProductName { get; set; }
    public string? ProductType { get; set; } = "Flower";
    public string? StrainName { get; set; }
    public string DocumentClassification { get; set; } = "FullComplianceCoa";
    public bool IsFullComplianceCoa { get; set; } = true;

    public string? BatchId { get; set; }
    public string? LotId { get; set; }
    public string? MetrcBatchId { get; set; }
    public string? MetrcSampleId { get; set; }

    public DateTime? HarvestDate { get; set; }
    public DateTime? SampleReceivedDate { get; set; }
    public DateTime? TestDate { get; set; }
    public DateTime? ReportCreatedDate { get; set; }
    public DateTime? PackageDate { get; set; }

    public string? OverallStatus { get; set; }

    public List<CoaAnalyteResult> Cannabinoids { get; set; } = [];
    public List<CoaAnalyteResult> Terpenes { get; set; } = [];

    public List<CoaSafetyResult> SafetyResults { get; set; } = [];

    public decimal? TotalThcPercent { get; set; }
    public decimal? TotalCbdPercent { get; set; }
    public decimal? TotalCannabinoidsPercent { get; set; }
    public decimal? TotalTerpenesPercent { get; set; }

    public CoaParserMetadata ParserMetadata { get; set; } = new();
}
