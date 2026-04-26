using CannabisCOA.Parser.Core.Analysis;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaMetadataParserTests
{
    [Fact]
    public void Analyze_Flags_Amended_COA()
    {
        var text = @"
            Digipath Labs
            AMENDED REPORT
            Product Type: Flower
            THC: 0.42%
            THCA: 24.88%
            Result: PASS
        ";

        var result = CoaAnalyzer.Analyze(text);

        Assert.True(result.Coa.IsAmended);
        Assert.Contains(result.Validation.Warnings, w => w.Code == "AMENDED_COA");
    }
}