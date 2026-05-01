using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace CannabisCOA.Parser.Core.Tests;

public class CliBatchJsonlTests
{
    [Fact]
    public async Task BatchOutput_WritesCompactJsonObjectPerParsedFileLine()
    {
        var repoRoot = FindRepoRoot();
        var cliProject = Path.Combine(
            repoRoot,
            "src",
            "CannabisCOA.Parser.Cli",
            "CannabisCOA.Parser.Cli.csproj");

        var tempRoot = Path.Combine(Path.GetTempPath(), "cannabis-coa-cli-tests", Guid.NewGuid().ToString("N"));
        var inputDir = Path.Combine(tempRoot, "input");
        var outputPath = Path.Combine(tempRoot, "parsed.jsonl");

        Directory.CreateDirectory(inputDir);

        await File.WriteAllTextAsync(Path.Combine(inputDir, "one.txt"), BuildG3FlowerText("30.00", "0.50"));
        await File.WriteAllTextAsync(Path.Combine(inputDir, "two.txt"), BuildG3FlowerText("24.00", "0.40"));

        try
        {
            using var process = StartCli(repoRoot, cliProject, inputDir, outputPath);

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            Assert.True(
                process.WaitForExit(60_000),
                "CLI timed out.");

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            Assert.True(
                process.ExitCode == 0,
                $"Expected CLI exit code 0 but was {process.ExitCode}. stdout: {stdout} stderr: {stderr}");

            var lines = File.ReadAllLines(outputPath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            Assert.Equal(2, lines.Length);
            Assert.DoesNotContain(lines, line => line.StartsWith("  \"", StringComparison.Ordinal));

            foreach (var line in lines)
            {
                Assert.StartsWith("{", line);
                Assert.EndsWith("}", line);

                using var json = JsonDocument.Parse(line);

                Assert.True(json.RootElement.TryGetProperty("Coa", out _));
                Assert.True(json.RootElement.TryGetProperty("Validation", out _));
            }
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public async Task BatchCsvOutput_WritesFlowerV1AuditColumns()
    {
        var repoRoot = FindRepoRoot();
        var cliProject = Path.Combine(
            repoRoot,
            "src",
            "CannabisCOA.Parser.Cli",
            "CannabisCOA.Parser.Cli.csproj");

        var tempRoot = Path.Combine(Path.GetTempPath(), "cannabis-coa-cli-tests", Guid.NewGuid().ToString("N"));
        var inputDir = Path.Combine(tempRoot, "input");
        var csvPath = Path.Combine(tempRoot, "audit.csv");
        var defaultJsonlPath = Path.Combine(tempRoot, "output.jsonl");
        var sourceFile = "flower, one.txt";
        var edibleFile = "edible.txt";
        var singlePanelFile = "digipath-single-panel.txt";

        Directory.CreateDirectory(inputDir);

        await File.WriteAllTextAsync(Path.Combine(inputDir, sourceFile), BuildG3FlowerText("30.00", "0.50"));
        await File.WriteAllTextAsync(Path.Combine(inputDir, edibleFile), BuildG3EdibleText());
        await File.WriteAllTextAsync(Path.Combine(inputDir, singlePanelFile), BuildDigipathSinglePanelText());

        try
        {
            using var process = StartCli(
                repoRoot,
                cliProject,
                inputDir,
                outputPath: null,
                csvPath,
                workingDirectory: tempRoot);

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            Assert.True(
                process.WaitForExit(60_000),
                "CLI timed out.");

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            Assert.True(
                process.ExitCode == 0,
                $"Expected CLI exit code 0 but was {process.ExitCode}. stdout: {stdout} stderr: {stderr}");

            Assert.Contains("Processed 3 files", stdout);
            Assert.Contains(csvPath, stdout);
            Assert.DoesNotContain("output.jsonl", stdout);
            Assert.False(File.Exists(defaultJsonlPath));

            var csvLines = File.ReadAllLines(csvPath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            Assert.Equal(4, csvLines.Length);

            var header = SplitCsvLine(csvLines[0]);
            var rows = csvLines
                .Skip(1)
                .Select(SplitCsvLine)
                .ToList();
            var flowerRow = rows.Single(row => Value(row, "SourceFile") == sourceFile);
            var edibleRow = rows.Single(row => Value(row, "SourceFile") == edibleFile);
            var singlePanelRow = rows.Single(row => Value(row, "SourceFile") == singlePanelFile);

            Assert.Equal("SourceFile", header[0]);
            Assert.Contains("AuditProfile", header);
            Assert.Contains("IsFlowerV1Candidate", header);
            Assert.Contains("MapperSchemaVersion", header);
            Assert.Contains("DocumentClassification", header);
            Assert.Contains("IsFullComplianceCoa", header);
            Assert.DoesNotContain("SchemaVersion", header);
            Assert.Contains("ProductName", header);
            Assert.Contains("BatchId", header);
            Assert.Contains("CannabinoidCount", header);
            Assert.Contains("TerpeneCount", header);
            Assert.Contains("HasWarnings", header);
            Assert.Contains("WarningCount", header);
            Assert.Contains("MissingCoreFields", header);

            Assert.All(rows, row => Assert.Equal(header.Count, row.Count));

            Assert.Equal("FlowerV1BatchAudit", Value(flowerRow, "AuditProfile"));
            Assert.Equal("true", Value(flowerRow, "IsFlowerV1Candidate"));
            Assert.Equal("flower-coa-v1", Value(flowerRow, "MapperSchemaVersion"));
            Assert.Equal("FullComplianceCoa", Value(flowerRow, "DocumentClassification"));
            Assert.Equal("true", Value(flowerRow, "IsFullComplianceCoa"));
            Assert.Equal("G3 Labs", Value(flowerRow, "LabName"));
            Assert.Equal("Flower", Value(flowerRow, "ProductType"));
            Assert.Equal("2026-01-02", Value(flowerRow, "TestDate"));
            Assert.Equal("pass", Value(flowerRow, "OverallStatus"));
            Assert.Equal("26.81", Value(flowerRow, "TotalTHC"));
            Assert.Equal("2", Value(flowerRow, "CannabinoidCount"));
            Assert.Equal("0", Value(flowerRow, "TerpeneCount"));
            Assert.Equal("false", Value(flowerRow, "HasWarnings"));
            Assert.Equal("0", Value(flowerRow, "WarningCount"));
            Assert.Contains("ProductName", Value(flowerRow, "MissingCoreFields"));
            Assert.Contains("BatchId", Value(flowerRow, "MissingCoreFields"));

            Assert.Equal("FlowerV1BatchAudit", Value(edibleRow, "AuditProfile"));
            Assert.Equal("false", Value(edibleRow, "IsFlowerV1Candidate"));
            Assert.Equal("flower-coa-v1", Value(edibleRow, "MapperSchemaVersion"));
            Assert.Equal("Edible", Value(edibleRow, "ProductType"));

            Assert.Equal("FlowerV1BatchAudit", Value(singlePanelRow, "AuditProfile"));
            Assert.Equal("true", Value(singlePanelRow, "IsFlowerV1Candidate"));
            Assert.Equal("SinglePanelTest", Value(singlePanelRow, "DocumentClassification"));
            Assert.Equal("false", Value(singlePanelRow, "IsFullComplianceCoa"));
            Assert.Equal("Digipath", Value(singlePanelRow, "LabName"));
            Assert.Equal("Flower", Value(singlePanelRow, "ProductType"));
            Assert.Equal("20260126HBD-11", Value(singlePanelRow, "BatchId"));
            Assert.Equal("0", Value(singlePanelRow, "CannabinoidCount"));
            Assert.DoesNotContain("Cannabinoids", Value(singlePanelRow, "MissingCoreFields"));
            Assert.Contains("SINGLE_PANEL_TEST", Value(singlePanelRow, "Warnings"));

            string Value(IReadOnlyList<string> row, string columnName)
            {
                var index = header.IndexOf(columnName);
                Assert.True(index >= 0, $"Missing CSV column {columnName}.");
                return row[index];
            }
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static Process StartCli(
        string repoRoot,
        string cliProject,
        string inputDir,
        string? outputPath,
        string? csvPath = null,
        string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workingDirectory ?? repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(cliProject);
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add("--batch");
        startInfo.ArgumentList.Add(inputDir);

        if (outputPath is not null)
        {
            startInfo.ArgumentList.Add("--out");
            startInfo.ArgumentList.Add(outputPath);
        }

        if (csvPath is not null)
        {
            startInfo.ArgumentList.Add("--csv");
            startInfo.ArgumentList.Add(csvPath);
        }

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start CLI process.");
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        values.Add(current.ToString());
        return values;
    }

    private static string BuildG3FlowerText(string thca, string thc)
    {
        var thcaMgPerGram = (decimal.Parse(thca, CultureInfo.InvariantCulture) * 10m)
            .ToString(CultureInfo.InvariantCulture);
        var thcMgPerGram = (decimal.Parse(thc, CultureInfo.InvariantCulture) * 10m)
            .ToString(CultureInfo.InvariantCulture);

        return $"""
        G3 Labs
        Certificate of Analysis
        Product Type: Plant, Flower - Cured
        Analysis Date: 01/02/2026
        Cannabinoids
        THCa 0.00016 {thca} {thcaMgPerGram}
        Δ9-THC 0.00016 {thc} {thcMgPerGram}
        Result Status: PASS
        """;
    }

    private static string BuildG3EdibleText()
    {
        return """
        G3 Labs
        Certificate of Analysis
        Product Type: Edible
        Gummy
        Analysis Date: 01/03/2026
        Cannabinoids
        THCa 0.00016 0.00 0.0
        Δ9-THC 0.00016 10.00 100.0
        Result Status: PASS
        """;
    }

    private static string BuildDigipathSinglePanelText()
    {
        return """
        1 of 1
        20260126HBD-11 (702 Headband Flower) Sample: DIGP2602.0230.P.01288
        G3 Labs (Digi) Sample Date: 02/27/2026 Report Date: 02/27/2026
        MME ID: 46100778561169443516 - L007 METRC Sample: 1A4040300000087000018685
        Plant, Flower - Cured
        Pesticides Not Tested Microbials Not Tested
        Mycotoxins Not Tested
        Solvents Not Tested
        Heavy Metals Pass
        Arsenic 4.6 2000.0 22.3 Pass
        Cadmium 6.9 820.0 <LOQ Pass
        Lead 4.5 1200.0 7.4 Pass
        Mercury 3.6 400.0 3.6 Pass
        Heavy Metals analyzed per Digipath Labs SOP-321 using an Agilent 7700 or 7900 ICP.MS.
        I certify that this sample has been tested by DigiPath Labs.
        """;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "CannabisCOA.Parser.slnx")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
