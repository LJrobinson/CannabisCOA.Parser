using CannabisCOA.Parser.Core.Analysis;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaAnalyzerTests
{
    [Fact]
    public void Analyze_Returns_Coa_And_Validation()
    {
        var text = @"
            THC: 0.42%
            THCA: 24.88%
            Test Date: 01/01/2026
            Result: PASS
        ";

        var result = CoaAnalyzer.Analyze(text);

        Assert.NotNull(result.Coa);
        Assert.NotNull(result.Validation);
    }
}