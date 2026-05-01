using System.Text.Json;
using System.Text.Json.Serialization;
using CannabisCOA.Parser.Core.Analysis;

const string GenericLabName = "Generic";
const string UnknownLabsLogPath = "unknown-labs.txt";
const string FailuresLogPath = "failures.txt";

var argsList = args.ToList();
var scoreOnly = argsList.Contains("--score-only");
var raw = argsList.Contains("--raw");
var csv = argsList.Contains("--csv");
var dumpText = argsList.Contains("--dump-text");
var loggedUnknownLabs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = !raw,
    Converters = { new JsonStringEnumConverter() }
};

var batchJsonLineOptions = new JsonSerializerOptions
{
    WriteIndented = false,
    Converters = { new JsonStringEnumConverter() }
};

if (args.Length == 0)
{
    Console.WriteLine("CannabisCOA.Parser CLI");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  cannabis-coa \"THC: 0.42% THCA: 24.88%\"");
    Console.WriteLine("  cannabis-coa --file fixtures/digipath-flower.txt");
    Console.WriteLine("  cannabis-coa --file fixtures/digipath-flower.txt --score-only");
    Console.WriteLine("  cannabis-coa --file fixtures/digipath-flower.txt --raw");
    Console.WriteLine("  cannabis-coa --file sample.pdf --dump-text");
    Console.WriteLine("  cannabis-coa --batch G:\\COAs --out parsed.jsonl");
    Console.WriteLine("  cannabis-coa --batch G:\\COAs --csv audit.csv");
    Console.WriteLine("  cannabis-coa --batch G:\\COAs --out parsed.jsonl --csv parsed.csv");
    return;
}

if (argsList.Contains("--batch"))
{
    var dirIndex = argsList.IndexOf("--batch");

    if (dirIndex + 1 >= argsList.Count)
    {
        Console.Error.WriteLine("Missing folder path after --batch");
        Environment.Exit(1);
        return;
    }

    var inputDir = argsList[dirIndex + 1];

    if (!Directory.Exists(inputDir))
    {
        Console.Error.WriteLine($"Directory not found: {inputDir}");
        Environment.Exit(1);
        return;
    }

    string? jsonlOutput = null;
    var outRequested = argsList.Contains("--out");

    if (outRequested)
    {
        var outIdx = argsList.IndexOf("--out");

        if (outIdx + 1 >= argsList.Count)
        {
            Console.Error.WriteLine("Missing file path after --out");
            Environment.Exit(1);
            return;
        }

        jsonlOutput = argsList[outIdx + 1];
    }

    string? csvOutput = null;

    if (argsList.Contains("--csv"))
    {
        var csvIdx = argsList.IndexOf("--csv");

        if (csvIdx + 1 >= argsList.Count)
        {
            Console.Error.WriteLine("Missing file path after --csv");
            Environment.Exit(1);
            return;
        }

        csvOutput = argsList[csvIdx + 1];
    }

    if (!outRequested && csvOutput is null)
    {
        jsonlOutput = "output.jsonl";
    }

    var files = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories)
        .Where(f =>
            f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
            f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (dumpText)
    {
        foreach (var file in files)
        {
            Console.WriteLine($"===== {file} =====");

            var text = file.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? PdfTextExtractor.Extract(file)
                : File.ReadAllText(file);

            Console.WriteLine(text);
        }

        return;
    }

    using var writer = jsonlOutput is null ? null : new StreamWriter(jsonlOutput, append: false);
    using var csvWriter = csvOutput is null ? null : new StreamWriter(csvOutput, append: false);

    if (csvWriter is not null)
    {
        BatchCsvWriter.WriteHeader(csvWriter);
    }

    foreach (var file in files)
    {
        try
        {
            var text = file.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? PdfTextExtractor.Extract(file)
                : File.ReadAllText(file);

            var res = CoaAnalyzer.Analyze(text);

            if (!raw)
            {
                res.Coa.Cannabinoids.TotalTHC = Math.Round(res.Coa.Cannabinoids.TotalTHC, 2);
                res.Coa.Cannabinoids.TotalCBD = Math.Round(res.Coa.Cannabinoids.TotalCBD, 2);
                res.Coa.Terpenes.TotalTerpenes = Math.Round(res.Coa.Terpenes.TotalTerpenes, 2);
            }

            writer?.WriteLine(JsonSerializer.Serialize(res, batchJsonLineOptions));
            BatchCsvWriter.WriteRow(csvWriter, file, res);

            if (IsGenericLab(res.Coa.LabName))
            {
                LogUnknownLabOnce(file);
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText(FailuresLogPath, file + " | " + ex.Message + Environment.NewLine);
        }
    }

    var writtenOutputs = new List<string>();

    if (jsonlOutput is not null)
        writtenOutputs.Add($"JSONL {jsonlOutput}");

    if (csvOutput is not null)
        writtenOutputs.Add($"CSV {csvOutput}");

    Console.WriteLine($"Processed {files.Count} files → {string.Join(", ", writtenOutputs)}");
    return;
}

string inputText;

if (argsList.Contains("--stdin"))
{
    using var reader = new StreamReader(Console.OpenStandardInput());
    inputText = await reader.ReadToEndAsync();
}
else if (argsList.Contains("--file"))
{
    var fileFlagIndex = argsList.IndexOf("--file");

    if (fileFlagIndex + 1 >= argsList.Count)
    {
        Console.Error.WriteLine("Missing file path after --file");
        Environment.Exit(1);
        return;
    }

    var filePath = argsList[fileFlagIndex + 1];

    if (!File.Exists(filePath))
    {
        Console.Error.WriteLine($"File not found: {filePath}");
        Environment.Exit(1);
        return;
    }

    inputText = filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
        ? PdfTextExtractor.Extract(filePath)
        : File.ReadAllText(filePath);
}
else
{
    inputText = string.Join(" ", argsList.Where(a =>
        a != "--score-only" &&
        a != "--raw" &&
        a != "--csv" &&
        a != "--dump-text"
    ));
}

if (dumpText)
{
    Console.Write(inputText);
    return;
}

var result = CoaAnalyzer.Analyze(inputText);

if (!raw)
{
    result.Coa.Cannabinoids.TotalTHC = Math.Round(result.Coa.Cannabinoids.TotalTHC, 2);
    result.Coa.Cannabinoids.TotalCBD = Math.Round(result.Coa.Cannabinoids.TotalCBD, 2);
    result.Coa.Terpenes.TotalTerpenes = Math.Round(result.Coa.Terpenes.TotalTerpenes, 2);
}

if (IsGenericLab(result.Coa.LabName))
{
    LogUnknownLabOnce(CreateUnknownLabPreview(inputText));
}

var hasCritical = result.Validation.Warnings.Any(w => w.Severity == "critical");
Environment.ExitCode = hasCritical ? 3 : (result.Validation.IsValid ? 0 : 2);

if (scoreOnly)
{
    Console.WriteLine(JsonSerializer.Serialize(result.Score, jsonOptions));
    return;
}

if (csv)
{
    Console.WriteLine("Lab,Type,TotalTHC,TotalTerps,Score,Tier");
    Console.WriteLine(string.Join(",",
        result.Coa.LabName,
        result.Coa.ProductType,
        result.Coa.Cannabinoids.TotalTHC,
        result.Coa.Terpenes.TotalTerpenes,
        result.Score.Score,
        result.Score.Tier
    ));
    return;
}

Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));

bool IsGenericLab(string labName)
{
    return string.Equals(labName, GenericLabName, StringComparison.OrdinalIgnoreCase);
}

void LogUnknownLabOnce(string entry)
{
    var cleanEntry = CollapseWhitespace(entry);

    if (string.IsNullOrWhiteSpace(cleanEntry))
        cleanEntry = "Unknown input";

    if (!loggedUnknownLabs.Add(cleanEntry))
        return;

    if (File.Exists(UnknownLabsLogPath))
    {
        var existingEntries = File.ReadLines(UnknownLabsLogPath);

        if (existingEntries.Any(line => string.Equals(line, cleanEntry, StringComparison.OrdinalIgnoreCase)))
            return;
    }

    File.AppendAllText(UnknownLabsLogPath, cleanEntry + Environment.NewLine);
}

static string CreateUnknownLabPreview(string text)
{
    var preview = CollapseWhitespace(text);

    if (preview.Length <= 140)
        return preview;

    return preview[..140] + "...";
}

static string CollapseWhitespace(string text)
{
    return string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
