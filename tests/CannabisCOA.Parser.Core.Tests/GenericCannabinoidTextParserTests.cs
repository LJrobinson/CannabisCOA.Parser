using CannabisCOA.Parser.Core.Parsers;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class GenericCannabinoidTextParserTests
{
    [Fact]
    public void Parses_Basic_Cannabinoids_Correctly()
    {
        var text = @"
            THC: 0.42%
            THCA: 24.88%
            CBD: 0.05%
            CBDA: 0.12%
        ";

        var result = GenericCannabinoidTextParser.Parse(text);

        Assert.Equal(0.42m, result.THC.Value);
        Assert.Equal(24.88m, result.THCA.Value);
        Assert.Equal(0.05m, result.CBD.Value);
        Assert.Equal(0.12m, result.CBDA.Value);
    }
}