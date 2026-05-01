using System.Globalization;
using CannabisCOA.Parser.Core.Analysis;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Mappers;
using CannabisCOA.Parser.Core.Models;

internal static class BatchCsvWriter
{
    private const string AuditProfile = "FlowerV1BatchAudit";

    private static readonly string[] Header =
    [
        "SourceFile",
        "AuditProfile",
        "IsFlowerV1Candidate",
        "MapperSchemaVersion",
        "LabName",
        "ProductType",
        "ProductName",
        "BatchId",
        "TestDate",
        "HarvestDate",
        "PackageDate",
        "OverallStatus",
        "TotalTHC",
        "TotalCBD",
        "TotalTerpenes",
        "CannabinoidCount",
        "TerpeneCount",
        "HasWarnings",
        "WarningCount",
        "MissingCoreFields",
        "IsAmended",
        "THC",
        "THCA",
        "CBD",
        "CBDA",
        "THCSourceText",
        "THCConfidence",
        "THCASourceText",
        "THCAConfidence",
        "CBDSourceText",
        "CBDConfidence",
        "CBDASourceText",
        "CBDAConfidence",
        "Warnings"
    ];

    public static void WriteHeader(TextWriter writer)
    {
        writer.WriteLine(string.Join(",", Header));
    }

    public static void WriteRow(TextWriter? writer, string filePath, CoaAnalysisResult result)
    {
        if (writer is null)
            return;

        var coa = result.Coa;
        var cannabinoids = coa.Cannabinoids;
        var sourceFile = Path.GetFileName(filePath);
        var document = CoaDocumentMapper.FromCoaResult(
            coa,
            sourceFileName: sourceFile,
            parserName: nameof(BatchCsvWriter));
        var isFlowerV1Candidate = coa.ProductType == ProductType.Flower;
        var warningCodes = result.Validation.Warnings
            .Select(w => w.Code)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToList();

        var row = new[]
        {
            sourceFile,
            AuditProfile,
            isFlowerV1Candidate ? "true" : "false",
            document.SchemaVersion,
            document.LabName,
            document.ProductType,
            document.ProductName,
            document.BatchId,
            FormatDate(document.TestDate),
            FormatDate(document.HarvestDate),
            FormatDate(document.PackageDate),
            document.OverallStatus,
            FormatDecimal(document.TotalThcPercent),
            FormatDecimal(document.TotalCbdPercent),
            FormatDecimal(document.TotalTerpenesPercent),
            document.Cannabinoids.Count.ToString(CultureInfo.InvariantCulture),
            document.Terpenes.Count.ToString(CultureInfo.InvariantCulture),
            warningCodes.Count > 0 ? "true" : "false",
            warningCodes.Count.ToString(CultureInfo.InvariantCulture),
            string.Join("|", document.ParserMetadata.MissingFields),
            coa.IsAmended ? "true" : "false",
            FormatParsedValue(cannabinoids.THC),
            FormatParsedValue(cannabinoids.THCA),
            FormatParsedValue(cannabinoids.CBD),
            FormatParsedValue(cannabinoids.CBDA),
            cannabinoids.THC.SourceText,
            FormatParsedConfidence(cannabinoids.THC),
            cannabinoids.THCA.SourceText,
            FormatParsedConfidence(cannabinoids.THCA),
            cannabinoids.CBD.SourceText,
            FormatParsedConfidence(cannabinoids.CBD),
            cannabinoids.CBDA.SourceText,
            FormatParsedConfidence(cannabinoids.CBDA),
            string.Join("|", warningCodes)
        };

        writer.WriteLine(string.Join(",", row.Select(Escape)));
    }

    private static string FormatDate(DateTime? value)
    {
        return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatDecimal(decimal? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture) ?? "";
    }

    private static string FormatParsedValue(ParsedField<decimal> field)
    {
        return IsMissing(field)
            ? ""
            : field.Value.ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatParsedConfidence(ParsedField<decimal> field)
    {
        return IsMissing(field)
            ? ""
            : field.Confidence.ToString(CultureInfo.InvariantCulture);
    }

    private static bool IsMissing(ParsedField<decimal> field)
    {
        return field.Value == 0m &&
               field.Confidence == 0m &&
               string.IsNullOrEmpty(field.SourceText);
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.IndexOfAny([',', '"', '\r', '\n', '|']) < 0)
            return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
