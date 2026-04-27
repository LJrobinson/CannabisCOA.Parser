using System.IO;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class DigipathFixtureTests
{
    private static string LoadFixture(string name)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Labs",
            name);

        return File.ReadAllText(path);
    }

    [Fact]
    public void Parses_Digipath_Flower_Fixture_Correctly()
    {
        var text = LoadFixture("Digipath_Flower.txt");

        var result = CoaParser.Parse(text);

        Assert.Equal("Digipath", result.LabName);

        Assert.Equal(26.564m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.225m, result.Cannabinoids.THC.Value);

        Assert.NotNull(result.TestDate);
        Assert.Equal(new DateTime(2025, 12, 17), result.TestDate);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);

        Assert.True(result.Terpenes.TotalTerpenes > 0);
    }
}
