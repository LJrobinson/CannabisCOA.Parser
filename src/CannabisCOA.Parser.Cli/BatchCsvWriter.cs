using System.Globalization;
using CannabisCOA.Parser.Core.Analysis;
using CannabisCOA.Parser.Core.Models;

internal static class BatchCsvWriter
{
    private static readonly string[] Header =
    [
        "FileName",
        "ProductType",
        "LabName",
        "ProductName",
        "BatchId",
        "IsAmended",
        "HarvestDate",
        "TestDate",
        "PackageDate",
        "THC",
        "THCA",
        "CBD",
        "CBDA",
        "TotalTHC",
        "TotalCBD",
        "TotalTerpenes",
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

        var row = new[]
        {
            Path.GetFileName(filePath),
            coa.ProductType.ToString(),
            coa.LabName,
            coa.ProductName,
            coa.BatchId,
            coa.IsAmended ? "true" : "false",
            FormatDate(coa.HarvestDate),
            FormatDate(coa.TestDate),
            FormatDate(coa.PackageDate),
            FormatParsedValue(cannabinoids.THC),
            FormatParsedValue(cannabinoids.THCA),
            FormatParsedValue(cannabinoids.CBD),
            FormatParsedValue(cannabinoids.CBDA),
            FormatDecimal(coa.Cannabinoids.TotalTHC),
            FormatDecimal(coa.Cannabinoids.TotalCBD),
            FormatDecimal(coa.Terpenes.TotalTerpenes),
            cannabinoids.THC.SourceText,
            FormatParsedConfidence(cannabinoids.THC),
            cannabinoids.THCA.SourceText,
            FormatParsedConfidence(cannabinoids.THCA),
            cannabinoids.CBD.SourceText,
            FormatParsedConfidence(cannabinoids.CBD),
            cannabinoids.CBDA.SourceText,
            FormatParsedConfidence(cannabinoids.CBDA),
            string.Join("|", result.Validation.Warnings
                .Select(w => w.Code)
                .Where(code => !string.IsNullOrWhiteSpace(code)))
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
