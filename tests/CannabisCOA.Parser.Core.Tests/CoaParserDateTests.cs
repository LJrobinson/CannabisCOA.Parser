using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaParserDateTests
{
    [Fact]
    public void Extracts_TestDate_FromStandardLabel()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Test Date: 01/01/2026
        """;

        var result = CoaParser.Parse(text);

        Assert.NotNull(result.TestDate);
        Assert.Equal(2026, result.TestDate!.Value.Year);
        Assert.Equal(1, result.TestDate.Value.Month);
        Assert.Equal(1, result.TestDate.Value.Day);
    }

    [Fact]
    public void Extracts_TestDate_FromReportedLabel()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Date Reported: 2026-02-15
        """;

        var result = CoaParser.Parse(text);

        Assert.NotNull(result.TestDate);
        Assert.Equal(2026, result.TestDate!.Value.Year);
        Assert.Equal(2, result.TestDate.Value.Month);
        Assert.Equal(15, result.TestDate.Value.Day);
    }

    [Fact]
    public void Extracts_TestDate_FromMonthNameFormat()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Completed: April 5, 2026
        """;

        var result = CoaParser.Parse(text);

        Assert.NotNull(result.TestDate);
        Assert.Equal(2026, result.TestDate!.Value.Year);
        Assert.Equal(4, result.TestDate.Value.Month);
        Assert.Equal(5, result.TestDate.Value.Day);
    }

    [Fact]
    public void Extracts_HarvestDate_FromHarvestLabel()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Harvest Date: 03/10/2026
        Test Date: 04/01/2026
        """;

        var result = CoaParser.Parse(text);

        Assert.NotNull(result.HarvestDate);
        Assert.Equal(2026, result.HarvestDate!.Value.Year);
        Assert.Equal(3, result.HarvestDate.Value.Month);
        Assert.Equal(10, result.HarvestDate.Value.Day);
    }

    [Fact]
    public void Extracts_PackageDate_FromPackageLabel()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Package Date: 03/20/2026
        Test Date: 04/01/2026
        """;

        var result = CoaParser.Parse(text);

        Assert.NotNull(result.PackageDate);
        Assert.Equal(2026, result.PackageDate!.Value.Year);
        Assert.Equal(3, result.PackageDate.Value.Month);
        Assert.Equal(20, result.PackageDate.Value.Day);
    }

    [Fact]
    public void Extracts_All_Date_Types_Independently()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Harvest Date: 2026-03-10
        Package Date: 2026-03-20
        Test Date: 2026-04-01
        """;

        var result = CoaParser.Parse(text);

        Assert.Equal(new DateTime(2026, 3, 10), result.HarvestDate);
        Assert.Equal(new DateTime(2026, 3, 20), result.PackageDate);
        Assert.Equal(new DateTime(2026, 4, 1), result.TestDate);
    }

    [Fact]
    public void Does_Not_Use_HarvestDate_Or_PackageDate_As_TestDate()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Harvest Date: 01/01/2024
        Package Date: 02/01/2024
        """;

        var result = CoaParser.Parse(text);

        Assert.Null(result.TestDate);
        Assert.Equal(new DateTime(2024, 1, 1), result.HarvestDate);
        Assert.Equal(new DateTime(2024, 2, 1), result.PackageDate);
    }

    [Fact]
    public void Calculates_Freshness_When_TestDate_Exists()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Test Date: 01/01/2026
        """;

        var result = CoaParser.Parse(text);

        Assert.NotNull(result.TestDate);
        Assert.True(result.Freshness.DaysSinceTest >= 0);
        Assert.NotEqual("Unknown", result.Freshness.Band);
    }

    [Fact]
    public void Freshness_Is_Unknown_When_TestDate_Missing()
    {
        var text = """
        THC: 1.0%
        THCA: 20.0%
        Harvest Date: 01/01/2024
        """;

        var result = CoaParser.Parse(text);

        Assert.Null(result.TestDate);
        Assert.Equal("Unknown", result.Freshness.Band);
    }
}