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

    private static Process StartCli(
        string repoRoot,
        string cliProject,
        string inputDir,
        string outputPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(cliProject);
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add("--batch");
        startInfo.ArgumentList.Add(inputDir);
        startInfo.ArgumentList.Add("--out");
        startInfo.ArgumentList.Add(outputPath);

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start CLI process.");
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
