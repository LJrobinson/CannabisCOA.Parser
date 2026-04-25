using CannabisCOA.Parser.Core.Parsers;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class ComplianceParserTests
{
    [Fact]
    public void Detects_Pass()
    {
        var text = "Overall Result: PASS";

        var result = ComplianceParser.Parse(text);

        Assert.True(result.Passed);
        Assert.Equal("pass", result.Status);
    }

    [Fact]
    public void Detects_Fail()
    {
        var text = "Result: FAIL";

        var result = ComplianceParser.Parse(text);

        Assert.False(result.Passed);
        Assert.Equal("fail", result.Status);
    }
}