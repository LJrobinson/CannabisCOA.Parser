using System;
using System.IO;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class RSRFixtureTests
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
    public void Parses_RSR_Flower_Fixture_Correctly()
    {
        var text = LoadFixture("RSR_Flower.txt");

        var result = CoaParser.Parse(text);

        Assert.Equal("RSR Analytical Laboratories", result.LabName);

        Assert.Equal(37.74m, result.Cannabinoids.THCA.Value);
        Assert.Equal(1.12m, result.Cannabinoids.THC.Value);

        Assert.NotNull(result.TestDate);
        Assert.Equal(new DateTime(2026, 4, 1), result.TestDate);

        Assert.True(result.Compliance.Passed);

        Assert.Equal(2.143m, result.Terpenes.TotalTerpenes);
    }
}