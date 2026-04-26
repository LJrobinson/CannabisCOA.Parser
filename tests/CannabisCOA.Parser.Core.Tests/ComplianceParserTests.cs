using CannabisCOA.Parser.Core.Parsers;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class ComplianceParserTests
{
    [Fact]
    public void Detects_Pass_FromExplicitOverallResult()
    {
        var text = "Overall Result: PASS";

        var result = ComplianceParser.Parse(text);

        Assert.True(result.Passed);
        Assert.Equal("pass", result.Status);
        Assert.True(result.ContaminantsPassed);
    }

    [Fact]
    public void Detects_Fail_FromExplicitOverallResult()
    {
        var text = "Final Result: FAIL";

        var result = ComplianceParser.Parse(text);

        Assert.False(result.Passed);
        Assert.Equal("fail", result.Status);
        Assert.False(result.ContaminantsPassed);
    }

    [Fact]
    public void Ignores_AnalytePassRows_WhenNoOverallResultExists()
    {
        var text = """
        Salmonella: PASS
        E. Coli: PASS
        Residual Solvents: PASS
        Heavy Metals: PASS
        """;

        var result = ComplianceParser.Parse(text);

        Assert.False(result.Passed);
        Assert.Equal("unknown", result.Status);
        Assert.Null(result.ContaminantsPassed);
    }

    [Fact]
    public void Allows_CleanStandalonePassLine()
    {
        var text = """
        Compliance Summary
        PASS
        """;

        var result = ComplianceParser.Parse(text);

        Assert.True(result.Passed);
        Assert.Equal("pass", result.Status);
        Assert.True(result.ContaminantsPassed);
    }

    [Fact]
    public void EmptyText_ReturnsUnknown()
    {
        var result = ComplianceParser.Parse("");

        Assert.False(result.Passed);
        Assert.Equal("unknown", result.Status);
        Assert.Null(result.ContaminantsPassed);
    }
}