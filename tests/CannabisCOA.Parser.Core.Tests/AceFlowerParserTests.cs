using CannabisCOA.Parser.Core.Enums;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class AceFlowerParserTests
{
    [Fact]
    public void Parses_Ace_Cannabinoid_Table_By_Mass_Percent()
    {
        var text = BuildAceCannabinoidTable(
            "THCa 0.060 22.959 229.59",
            "Δ9-THC 0.060 0.285 2.85",
            "CBDa 0.060 <LOQ <LOQ",
            "CBD 0.060 <LOQ <LOQ",
            "Total THC Total CBD Analysis Date: 11/17/2025");

        var result = CoaParser.Parse(text);

        Assert.Equal("Ace Analytical Laboratory", result.LabName);
        Assert.Equal(ProductType.Flower, result.ProductType);
        Assert.Equal(22.959m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.285m, result.Cannabinoids.THC.Value);
        Assert.Equal(0m, result.Cannabinoids.CBDA.Value);
        Assert.Equal(0m, result.Cannabinoids.CBD.Value);
    }

    [Fact]
    public void Parses_Ace_Terpene_Table_By_Mass_Percent()
    {
        var text = BuildAceTerpeneTable(
            "δ-Limonene 0.04 12.61 1.261",
            "β-Myrcene 0.04 10.63 1.063",
            "β-Caryophyllene 0.04 2.93 0.293",
            "Linalool 0.04 2.11 0.211",
            "α-Terpinene 0.04 <LOQ <LOQ",
            "Total 34.36 3.436");

        var result = CoaParser.Parse(text);

        Assert.Equal(1.261m, result.Terpenes.Terpenes["δ-Limonene"]);
        Assert.Equal(1.063m, result.Terpenes.Terpenes["β-Myrcene"]);
        Assert.Equal(0.293m, result.Terpenes.Terpenes["β-Caryophyllene"]);
        Assert.Equal(0.211m, result.Terpenes.Terpenes["Linalool"]);
        Assert.Equal(0m, result.Terpenes.Terpenes["α-Terpinene"]);
        Assert.Equal(3.436m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void Parses_Ace_Passing_Heavy_Metals_As_Contaminant_Pass()
    {
        var text = BuildAceHeavyMetalTable(
            "Lead 50 1200 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Fails_Ace_Heavy_Metal_Result_Above_Limit()
    {
        var text = BuildAceHeavyMetalTable(
            "Arsenic 50 2000 2500 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Parses_Ace_Binary_Microbial_Nd_As_Pass()
    {
        var text = BuildAceMicrobialTable(
            "Aspergillus flavus ND Pass",
            "E. Coli ND Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Parses_Ace_Quantitative_Microbial_Numeric_Pass()
    {
        var text = BuildAceMicrobialTable(
            "Yeast & Mold 100 10000 300.00 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Fails_Ace_Quantitative_Microbial_Result_Above_Limit()
    {
        var text = BuildAceMicrobialTable(
            "Yeast & Mold 100 10000 12000 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Fails_Ace_Binary_Microbial_Detected_Result()
    {
        var text = BuildAceMicrobialTable(
            "Salmonella Detected Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Parses_Ace_Mycotoxins_As_Contaminant_Pass()
    {
        var text = BuildAceMycotoxinTable(
            "Aflatoxins 5.00 20.00 <LOQ Pass",
            "Ochratoxin A 5.00 20.00 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Fails_Ace_Mycotoxin_Result_Above_Limit()
    {
        var text = BuildAceMycotoxinTable(
            "Aflatoxins 5.00 20.00 25.00 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Parses_Ace_Pesticide_Short_Row_As_Pass()
    {
        var text = BuildAcePesticideTable(
            "Acequinocyl 0.020 4.000 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Parses_Ace_Pesticide_Lod_Loq_Row_As_Pass()
    {
        var text = BuildAcePesticideTable(
            "Abamectin 0.010 0.020 >LOD <LOD Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Fails_Ace_Pesticide_Result_Above_Limit()
    {
        var text = BuildAcePesticideTable(
            "Bifenazate 0.020 0.400 0.500 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Parses_Ace_Split_Piperonyl_Butoxide_Row()
    {
        var text = BuildAcePesticideTable(
            "Piperonyl",
            "Butoxide 0.020 3.000 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Preserves_Ace_Explicit_Overall_Pass()
    {
        var text = $"""
            {BuildAceMycotoxinTable("Aflatoxins 5.00 20.00 <LOQ Pass")}
            Overall Result: PASS
            """;

        var result = CoaParser.Parse(text);

        Assert.True(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("pass", result.Compliance.Status);
    }

    private static string BuildAceCannabinoidTable(params string[] rows)
    {
        return $"""
            Ace Analytical Laboratory
            Product Type: Plant, Flower - Cured, Indoor
            Amended
            Harvest Date: 10/23/2025
            Cannabinoid Test Results
            Analyte LOQ Mass Mass
            % % mg/g
            {string.Join('\n', rows)}
            Terpene Test Results
            """;
    }

    private static string BuildAceTerpeneTable(params string[] rows)
    {
        return $"""
            Ace Analytical Laboratory
            Product Type: Flower
            Terpene Test Results
            Analyte LOQ Mass Mass
            mg/g mg/g %
            {string.Join('\n', rows)}
            Safety & Quality Tests
            """;
    }

    private static string BuildAcePesticideTable(params string[] rows)
    {
        return BuildAceSafetySection("Pesticides", "Analyte LOD LOQ Limit Mass Status", rows);
    }

    private static string BuildAceHeavyMetalTable(params string[] rows)
    {
        return BuildAceSafetySection("Heavy Metals", "Analyte LOQ Limit Mass Status", rows);
    }

    private static string BuildAceMicrobialTable(params string[] rows)
    {
        return BuildAceSafetySection("Microbials", "Analyte LOQ Limit Mass Status", rows);
    }

    private static string BuildAceMycotoxinTable(params string[] rows)
    {
        return BuildAceSafetySection("Mycotoxins", "Analyte LOQ Limit Mass Status", rows);
    }

    private static string BuildAceSafetySection(string sectionName, string header, params string[] rows)
    {
        return $"""
            Ace Analytical Laboratory
            Product Type: Flower
            Safety & Quality Tests
            {sectionName}
            {header}
            {string.Join('\n', rows)}
            """;
    }
}
