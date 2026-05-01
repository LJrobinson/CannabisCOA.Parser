using System.IO;
using CannabisCOA.Parser.Core.Mappers;
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
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("FullComplianceCoa", result.DocumentClassification);
        Assert.True(result.IsFullComplianceCoa);
        Assert.Equal("Kush Mints", result.ProductName);
        Assert.Equal("1A40403000044C1000008993", result.BatchId);

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
    public void Parses_Digipath_Flower_SinglePanelHeavyMetals_ReportClassification()
    {
        var text = LoadFixture("digipath-flower-single-panel-heavy-metals-headband.txt");

        var result = CoaParser.Parse(text);
        var validation = CoaValidator.Validate(result);
        var document = CoaDocumentMapper.FromCoaResult(result);

        Assert.Equal("Digipath", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("20260126HBD-11 (702 Headband Flower)", result.ProductName);
        Assert.Equal("20260126HBD-11", result.BatchId);
        Assert.Equal("SinglePanelTest", result.DocumentClassification);
        Assert.False(result.IsFullComplianceCoa);
        Assert.Equal("SinglePanelTest", document.DocumentClassification);
        Assert.False(document.IsFullComplianceCoa);
        Assert.Empty(document.Cannabinoids);
        Assert.DoesNotContain(nameof(document.Cannabinoids), document.ParserMetadata.MissingFields);
        Assert.Contains(validation.Warnings, warning => warning.Code == "SINGLE_PANEL_TEST");
        Assert.DoesNotContain(validation.Warnings, warning => warning.Code == "MISSING_THC_VALUES");
    }

    [Fact]
    public void Parses_Digipath_Flower_GenericDisplayName_FallsBackToStrain()
    {
        var text = LoadFixture("digipath-flower-generic-display-blue-dream.txt");

        var result = CoaParser.Parse(text);

        Assert.Equal("Digipath", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("BLUE DREAM", result.ProductName);
        Assert.Equal("DP-BATCH-001", result.BatchId);
    }

    [Fact]
    public void Parses_Digipath_Flower_SampleHeaderProductName()
    {
        var text = LoadFixture("digipath-flower-sample-header-super-boof.txt");

        var result = CoaParser.Parse(text);

        Assert.Equal("Digipath", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("Super Boof", result.ProductName);
        Assert.Equal("SB 12.10.25 FR3", result.BatchId);
    }

    [Fact]
    public void Parses_Digipath_Flower_PopcornBudsIndoor_AsFullFlowerCoa()
    {
        var text = LoadFixture("digipath-flower-popcorn-buds-indoor-trop-cherry.txt");

        var result = CoaParser.Parse(text);
        var document = CoaDocumentMapper.FromCoaResult(result);

        Assert.Equal("Digipath", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("FullComplianceCoa", result.DocumentClassification);
        Assert.True(result.IsFullComplianceCoa);
        Assert.Equal("FullComplianceCoa", document.DocumentClassification);
        Assert.True(document.IsFullComplianceCoa);
        Assert.Equal("Trop Cherry Popcorn", result.ProductName);
        Assert.Equal("TCP-042", result.BatchId);
        Assert.True(result.Cannabinoids.TotalTHC > 0m);
    }

    [Fact]
    public void Parses_Digipath_Flower_ShakeDuff_AsFullFlowerCoa()
    {
        var text = LoadFixture("digipath-flower-shake-duff-blue-dream.txt");

        var result = CoaParser.Parse(text);
        var document = CoaDocumentMapper.FromCoaResult(result);

        Assert.Equal("Digipath", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal("FullComplianceCoa", result.DocumentClassification);
        Assert.True(result.IsFullComplianceCoa);
        Assert.Equal("FullComplianceCoa", document.DocumentClassification);
        Assert.True(document.IsFullComplianceCoa);
        Assert.Equal("Blue Dream Shake", result.ProductName);
        Assert.Equal("SHK-117", result.BatchId);
        Assert.True(result.Cannabinoids.TotalTHC > 0m);
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
