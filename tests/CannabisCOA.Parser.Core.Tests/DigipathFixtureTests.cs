using System.IO;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Validation;
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

    [Fact]
    public void Parses_Digipath_Vape_Fixture_Cannabinoids_From_Mg_Per_Gram_Column()
    {
        var text = LoadFixture("digipath-vape-real-001.txt");

        var result = CoaParser.Parse(text);

        Assert.Contains(result.ProductType, new[] { ProductType.Vape, ProductType.Concentrate });
        Assert.Equal(887.395m, result.Cannabinoids.THC.Value);
        Assert.Equal(1.335m, result.Cannabinoids.THCA.Value);
        Assert.Equal(2.495m, result.Cannabinoids.CBD.Value);
        Assert.InRange(result.Cannabinoids.TotalTHC, 888.565m, 888.567m);
        Assert.Equal(2.495m, result.Cannabinoids.TotalCBD);
        Assert.Contains("Δ9-THC", result.Cannabinoids.THC.SourceText);
        Assert.DoesNotContain("Total Potential THC", result.Cannabinoids.THC.SourceText);
    }

    [Fact]
    public void Parses_Digipath_SideBySide_Vape_Fixture_Cannabinoids_From_Mg_Per_Gram_Column()
    {
        var text = LoadFixture("digipath-vape-side-by-side-real-001.txt");

        var result = CoaParser.Parse(text);
        var validation = CoaValidator.Validate(result);

        Assert.Contains(result.ProductType, new[] { ProductType.Vape, ProductType.Concentrate });
        Assert.Equal(887.395m, result.Cannabinoids.THC.Value);
        Assert.Equal(1.335m, result.Cannabinoids.THCA.Value);
        Assert.Equal(2.495m, result.Cannabinoids.CBD.Value);
        Assert.InRange(result.Cannabinoids.TotalTHC, 888.565m, 888.567m);
        Assert.Equal(2.495m, result.Cannabinoids.TotalCBD);
        Assert.Contains("Δ9-THC", result.Cannabinoids.THC.SourceText);
        Assert.DoesNotContain("Total Potential THC", result.Cannabinoids.THC.SourceText);
        Assert.DoesNotContain(validation.Warnings, warning => warning.Code == "MISSING_THC_VALUES");
    }
}
