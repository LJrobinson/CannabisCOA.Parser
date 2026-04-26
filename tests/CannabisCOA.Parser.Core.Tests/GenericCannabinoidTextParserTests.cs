using CannabisCOA.Parser.Core.Parsers;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class GenericCannabinoidTextParserTests
{
    [Fact]
    public void Parse_ExtractsBasicCannabinoidRows()
    {
        var text = """
        THC 0.251 %
        THCa 25.37 %
        CBD 0.083 %
        CBDa 0.120 %
        """;

        var result = GenericCannabinoidTextParser.Parse(text);

        Assert.Equal(0.251m, result.THC.Value);
        Assert.Equal(25.37m, result.THCA.Value);
        Assert.Equal(0.083m, result.CBD.Value);
        Assert.Equal(0.120m, result.CBDA.Value);
    }

    [Fact]
    public void Parse_IgnoresFormulaRows()
    {
        var text = """
        Total THC = THC + THCa * 0.877
        THC / 1
        CBD / 1
        MME ID: 950484000000000000000000
        THCa * 0.877
        THCa 24.50 %
        THC 0.44 %
        """;

        var result = GenericCannabinoidTextParser.Parse(text);

        Assert.Equal(24.50m, result.THCA.Value);
        Assert.Equal(0.44m, result.THC.Value);
    }

    [Fact]
    public void Parse_UsesLastNumericValueForMultiColumnRows()
    {
        var text = """
        Analyte LOQ LOD Result Unit
        THCa 0.010 0.003 26.42 %
        THC 0.010 0.003 0.58 %
        CBD 0.010 0.003 0.12 %
        CBDa 0.010 0.003 0.09 %
        """;

        var result = GenericCannabinoidTextParser.Parse(text);

        Assert.Equal(26.42m, result.THCA.Value);
        Assert.Equal(0.58m, result.THC.Value);
        Assert.Equal(0.12m, result.CBD.Value);
        Assert.Equal(0.09m, result.CBDA.Value);
    }

    [Fact]
    public void Parse_ConvertsMgPerGramToPercent()
    {
        var text = """
        THCa 212.5 mg/g
        THC 4.4 mg/g
        CBD 1.2 mg/g
        CBDa 0.9 mg/g
        """;

        var result = GenericCannabinoidTextParser.Parse(text);

        Assert.Equal(21.25m, result.THCA.Value);
        Assert.Equal(0.44m, result.THC.Value);
        Assert.Equal(0.12m, result.CBD.Value);
        Assert.Equal(0.09m, result.CBDA.Value);
    }

    [Fact]
    public void Parse_MissingCannabinoidsReturnZeroConfidence()
    {
        var text = """
        No cannabinoid result rows present.
        Total THC = THC + THCa * 0.877
        MME ID: 950484000000000000000000
        """;

        var result = GenericCannabinoidTextParser.Parse(text);

        Assert.Equal(0m, result.THC.Value);
        Assert.Equal(0m, result.THCA.Value);
        Assert.Equal(0m, result.CBD.Value);
        Assert.Equal(0m, result.CBDA.Value);

        Assert.Equal(0m, result.THC.Confidence);
        Assert.Equal(0m, result.THCA.Confidence);
        Assert.Equal(0m, result.CBD.Confidence);
        Assert.Equal(0m, result.CBDA.Confidence);
    }
}