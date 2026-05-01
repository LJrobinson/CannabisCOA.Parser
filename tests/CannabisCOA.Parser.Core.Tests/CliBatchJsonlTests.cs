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

        Directory.CreateDirectory(inputDir);

        await File.WriteAllTextAsync(Path.Combine(inputDir, sourceFile), BuildG3FlowerText("30.00", "0.50"));
        await File.WriteAllTextAsync(Path.Combine(inputDir, edibleFile), BuildG3EdibleText());

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

            Assert.Contains("Processed 2 files", stdout);
            Assert.Contains(csvPath, stdout);
            Assert.DoesNotContain("output.jsonl", stdout);
            Assert.False(File.Exists(defaultJsonlPath));

            var csvLines = File.ReadAllLines(csvPath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            Assert.Equal(3, csvLines.Length);

            var header = SplitCsvLine(csvLines[0]);
            var rows = csvLines
                .Skip(1)
                .Select(SplitCsvLine)
                .ToList();
            var flowerRow = rows.Single(row => Value(row, "SourceFile") == sourceFile);
            var edibleRow = rows.Single(row => Value(row, "SourceFile") == edibleFile);

            Assert.Equal("SourceFile", header[0]);
            Assert.Contains("AuditProfile", header);
            Assert.Contains("IsFlowerV1Candidate", header);
            Assert.Contains("MapperSchemaVersion", header);
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
