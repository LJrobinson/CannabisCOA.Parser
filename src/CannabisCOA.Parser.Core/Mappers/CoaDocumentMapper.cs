using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Mappers;

public static class CoaDocumentMapper
{
    public static CoaDocument FromCoaResult(
        CoaResult result,
        string? sourceFileName = null,
        string? parserName = null)
    {
        var document = new CoaDocument
        {
            LabName = result.LabName,
            ProductName = result.ProductName,
            ProductType = result.ProductType.ToString(),
            DocumentClassification = result.DocumentClassification,
            IsFullComplianceCoa = result.IsFullComplianceCoa,
            BatchId = result.BatchId,
            HarvestDate = result.HarvestDate,
            TestDate = result.TestDate,
            PackageDate = result.PackageDate,
            OverallStatus = result.Compliance?.Status,

            ParserMetadata =
            {
                SourceFileName = sourceFileName,
                DetectedLab = result.LabName,
                ParserName = parserName,
                ConfidenceScore = 0m
            }
        };

        AddCannabinoid(document, "THC", result.Cannabinoids?.THC);
        AddCannabinoid(document, "THCA", result.Cannabinoids?.THCA);
        AddCannabinoid(document, "CBD", result.Cannabinoids?.CBD);
        AddCannabinoid(document, "CBDA", result.Cannabinoids?.CBDA);

        document.TotalThcPercent = result.Cannabinoids?.TotalTHC;
        document.TotalCbdPercent = result.Cannabinoids?.TotalCBD;

        AddTerpenes(document, result.Terpenes);

        AddCompliance(document, result.Compliance);

        document.ParserMetadata.MissingFields = GetMissingFields(document);
        document.ParserMetadata.ConfidenceScore = ScoreConfidence(document);

        return document;
    }

    private static void AddCannabinoid(
        CoaDocument document,
        string name,
        ParsedField<decimal>? field)
    {
        if (field is null || field.Confidence <= 0m)
            return;

        document.Cannabinoids.Add(new CoaAnalyteResult
        {
            Name = name,
            NormalizedName = name,
            Value = field.Value,
            Unit = "%",
            Percent = field.Value,
            SourceText = field.SourceText
        });
    }

    private static void AddTerpenes(CoaDocument document, TerpeneProfile? profile)
    {
        if (profile is null)
            return;

        foreach (var terpene in profile.Terpenes)
        {
            document.Terpenes.Add(new CoaAnalyteResult
            {
                Name = terpene.Key,
                NormalizedName = terpene.Key,
                Value = terpene.Value,
                Unit = "%",
                Percent = terpene.Value,
                SourceText = terpene.Key
            });
        }

        document.TotalTerpenesPercent = profile.TotalTerpenes;
    }

    private static void AddCompliance(CoaDocument document, ComplianceResult? compliance)
    {
        if (compliance is null)
            return;

        document.SafetyResults.Add(new CoaSafetyResult
        {
            Category = "Overall Compliance",
            Status = compliance.Status,
            SourceText = compliance.Status
        });

        if (compliance.ContaminantsPassed is not null)
        {
            document.SafetyResults.Add(new CoaSafetyResult
            {
                Category = "Contaminants",
                Status = compliance.ContaminantsPassed == true ? "Pass" : "Fail",
                SourceText = $"ContaminantsPassed: {compliance.ContaminantsPassed}"
            });
        }
    }

    private static List<string> GetMissingFields(CoaDocument document)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(document.LabName)) missing.Add(nameof(document.LabName));
        if (string.IsNullOrWhiteSpace(document.ProductName)) missing.Add(nameof(document.ProductName));
        if (string.IsNullOrWhiteSpace(document.ProductType)) missing.Add(nameof(document.ProductType));
        if (document.IsFullComplianceCoa && string.IsNullOrWhiteSpace(document.BatchId))
            missing.Add(nameof(document.BatchId));
        if (document.TestDate is null) missing.Add(nameof(document.TestDate));
        if (string.IsNullOrWhiteSpace(document.OverallStatus)) missing.Add(nameof(document.OverallStatus));
        if (document.IsFullComplianceCoa && document.Cannabinoids.Count == 0)
            missing.Add(nameof(document.Cannabinoids));

        return missing;
    }

    private static decimal ScoreConfidence(CoaDocument document)
    {
        var total = 7;
        var present = total - document.ParserMetadata.MissingFields.Count;

        return Math.Round((decimal)present / total, 2);
    }
}
