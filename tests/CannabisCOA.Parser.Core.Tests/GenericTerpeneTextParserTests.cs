using CannabisCOA.Parser.Core.Parsers;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class GenericTerpeneTextParserTests
{
    [Fact]
    public void Parses_Common_Terpenes()
    {
        var text = @"
            Beta-Myrcene: 0.82%
            Limonene: 0.41%
            Beta-Caryophyllene: 0.38%
        ";

        var result = GenericTerpeneTextParser.Parse(text);

        Assert.Equal(0.82m, result.Terpenes["Beta-Myrcene"]);
        Assert.Equal(0.41m, result.Terpenes["Limonene"]);
        Assert.Equal(0.38m, result.Terpenes["Beta-Caryophyllene"]);
        Assert.Equal(1.61m, result.TotalTerpenes);
    }

    [Fact]
    public void Uses_Total_Terpenes_When_Provided()
    {
        var text = @"
            Beta-Myrcene: 0.82%
            Limonene: 0.41%
            Total Terpenes: 2.14%
        ";

        var result = GenericTerpeneTextParser.Parse(text);

        Assert.Equal(2.14m, result.TotalTerpenes);
    }

    [Fact]
    public void Converts_MgPerG_To_Percentage()
    {
        var text = @"
            Beta-Myrcene: 8.2 mg/g
        ";

        var result = GenericTerpeneTextParser.Parse(text);

        Assert.Equal(0.82m, result.Terpenes["Beta-Myrcene"]);
    }

    [Fact]
    public void Parses_Terpene_Table_Format()
    {
        var text = @"
            Terpene           Result (%)
            ----------------------------
            β-Myrcene         0.82
            Limonene          0.41
            β-Caryophyllene   0.38
        ";

        var result = GenericTerpeneTextParser.Parse(text);

        Assert.Equal(0.82m, result.Terpenes["Beta-Myrcene"]);
        Assert.Equal(0.41m, result.Terpenes["Limonene"]);
        Assert.Equal(0.38m, result.Terpenes["Beta-Caryophyllene"]);
    }
}