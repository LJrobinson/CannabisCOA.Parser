using CannabisCOA.Parser.Core.Parsers;
using CannabisCOA.Parser.Core.Adapters.Labs.Digipath.ProductParsers;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class DigipathFlowerParserTests
{
    private static string FixturePath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..","..",
            "fixtures",
            fileName));
    }

    [Fact]
    public void Parses_Cannabinoid_Side_Of_Combined_Digipath_Row()
    {
        var text = BuildDigipathTable(
            "Δ9-THC 0.003 0.366 3.66 δ-3-Carene 13466-78-9 0.0100 <LOQ <LOQ");

        var result = CoaParser.Parse(text);

        Assert.Equal(0.366m, result.Cannabinoids.THC.Value);
    }

    [Fact]
    public void Ignores_Terpene_Side_Values_After_Cannabinoid_Window()
    {
        var text = BuildDigipathTable(
            "Δ9-THC 0.003 0.201 2.01 Eucalyptol 470-82-6 0.0053 <LOQ <LOQ");

        var result = CoaParser.Parse(text);

        Assert.Equal(0.201m, result.Cannabinoids.THC.Value);
    }

    [Fact]
    public void Rejects_Collapsed_Cannabinoid_And_Terpene_Row()
    {
        var text = BuildDigipathTable(
            "THCa CBDa α-Humulene 6753-98-6 0.0059 0.0494 0.494");

        var result = CoaParser.Parse(text);

        Assert.Equal(0m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0m, result.Cannabinoids.THCA.Confidence);
        Assert.Equal(0m, result.Cannabinoids.CBDA.Value);
        Assert.Equal(0m, result.Cannabinoids.CBDA.Confidence);
    }

    [Fact]
    public void Keeps_Cannabinoid_Loq_From_Using_Terpene_Side_Loq()
    {
        var text = BuildDigipathTable(
            "CBD 0.014 <LOQ <LOQ Ocimene 13877-91-3 0.0083 <LOQ <LOQ");

        var result = CoaParser.Parse(text);

        Assert.Equal(0m, result.Cannabinoids.CBD.Value);
        Assert.Equal(0m, result.Cannabinoids.CBD.Confidence);
    }

    [Fact]
    public void Parses_Digipath_Cannabinoid_Table_Rows_By_Mass_Percent()
    {
        var text = BuildDigipathTable(
            "THCa 0.004 26.564 265.64",
            "Δ9-THC 0.003 0.225 2.25",
            "CBDa 0.004 0.056 0.56",
            "CBD 0.014 <LOQ <LOQ");

        var result = CoaParser.Parse(text);

        Assert.Equal(26.564m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.225m, result.Cannabinoids.THC.Value);
        Assert.Equal(0.056m, result.Cannabinoids.CBDA.Value);
        Assert.Equal(0m, result.Cannabinoids.CBD.Value);
    }

    [Theory]
    [InlineData("CBD", "CBD 0.014 <LOQ <LOQ")]
    [InlineData("CBDA", "CBDa 0.004 <LOQ <LOQ")]
    [InlineData("CBD", "CBD 0.014 ND ND")]
    public void DigipathFlowerParser_Parse_CbdNonDetectRows_MapToZeroConfidence(string cannabinoidName, string cannabinoidRow)
    {
        var text = BuildDigipathTable(
            "THCa 0.004 26.564 265.64",
            "Δ9-THC 0.003 0.225 2.25",
            cannabinoidRow);

        var result = CoaParser.Parse(text);
        var field = cannabinoidName == "CBDA"
            ? result.Cannabinoids.CBDA
            : result.Cannabinoids.CBD;

        Assert.Equal(0m, field.Value);
        Assert.Equal(0m, field.Confidence);
    }

    [Fact]
    public void Validates_Mass_Percent_Against_Mg_Per_Gram()
    {
        var text = BuildDigipathTable(
            "THCa 0.060 28.738 287.38",
            "CBDa 0.060 0.064 0.64 β-Pinene 0.04 0.27 0.027");

        var result = CoaParser.Parse(text);

        Assert.Equal(28.738m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0.064m, result.Cannabinoids.CBDA.Value);
    }

    [Fact]
    public void Rejects_Row_When_Mass_Percent_Does_Not_Match_Mg_Per_Gram()
    {
        var text = BuildDigipathTable(
            "THCa 0.004 26.564 123.45");

        var result = CoaParser.Parse(text);

        Assert.Equal(0m, result.Cannabinoids.THCA.Value);
        Assert.Equal(0m, result.Cannabinoids.THCA.Confidence);
    }

    [Fact]
    public void Parses_Digipath_Terpene_Table_Rows_By_Mass_Percent()
    {
        var text = BuildDigipathTerpeneTable(
            "β-Caryophyllene 87-44-5 0.0100 0.4242 4.242",
            "δ -Limonene 5989-27-5 0.0100 0.3497 3.497",
            "Linalool 78-70-6 0.0100 0.2787 2.787",
            "β-Myrcene 123-35-3 0.0100 0.1678 1.678",
            "α-Humulene 6753-98-6 0.0100 0.1455 1.455",
            "β-Pinene 18172-67-3 0.0100 0.0511 0.511",
            "α-Pinene 80-56-8 0.0100 0.0304 0.304",
            "Total 1.4474 14.474");

        var result = CoaParser.Parse(text);

        Assert.Equal(0.4242m, result.Terpenes.Terpenes["β-Caryophyllene"]);
        Assert.Equal(0.3497m, result.Terpenes.Terpenes["δ-Limonene"]);
        Assert.Equal(0.2787m, result.Terpenes.Terpenes["Linalool"]);
        Assert.Equal(0.1678m, result.Terpenes.Terpenes["β-Myrcene"]);
        Assert.Equal(0.1455m, result.Terpenes.Terpenes["α-Humulene"]);
        Assert.Equal(0.0511m, result.Terpenes.Terpenes["β-Pinene"]);
        Assert.Equal(0.0304m, result.Terpenes.Terpenes["α-Pinene"]);
        Assert.Equal(1.4474m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void Includes_Digipath_Terpene_Qualified_Rows_As_Zero()
    {
        var text = BuildDigipathTerpeneTable(
            "Eucalyptol 470-82-6 0.0100 <LOQ <LOQ",
            "Farnesene 502-61-4 0.0044 NR NR",
            "Ocimene 13877-91-3 0.0100 ND ND",
            "Total 0.0000 0.000");

        var result = CoaParser.Parse(text);

        Assert.Equal(0m, result.Terpenes.Terpenes["Eucalyptol"]);
        Assert.Equal(0m, result.Terpenes.Terpenes["Farnesene"]);
        Assert.Equal(0m, result.Terpenes.Terpenes["Ocimene"]);
    }

    [Fact]
    public void Does_Not_Select_Cas_Numbers_As_Digipath_Terpene_Values()
    {
        var text = BuildDigipathTerpeneTable(
            "Eucalyptol 470-82-6 0.0100 <LOQ <LOQ",
            "β-Caryophyllene 87-44-5 0.0100 0.4242 4.242",
            "Total 0.4242 4.242");

        var result = CoaParser.Parse(text);

        Assert.Equal(0.4242m, result.Terpenes.Terpenes["β-Caryophyllene"]);
        Assert.Equal(0m, result.Terpenes.Terpenes["Eucalyptol"]);
    }

    [Fact]
    public void Rejects_Digipath_Terpene_Mixed_Qualified_And_Numeric_Row()
    {
        var text = BuildDigipathTerpeneTable(
            "Eucalyptol 470-82-6 0.0100 <LOQ 0.123",
            "Farnesene 502-61-4 0.0044 NR 0.123",
            "Total 0.0000 0.000");

        var result = CoaParser.Parse(text);

        Assert.False(result.Terpenes.Terpenes.ContainsKey("Eucalyptol"));
        Assert.False(result.Terpenes.Terpenes.ContainsKey("Farnesene"));
    }

    [Fact]
    public void Rejects_Digipath_Terpene_Mismatched_Qualified_Row()
    {
        var text = BuildDigipathTerpeneTable(
            "Ocimene 13877-91-3 0.0100 ND <LOQ",
            "Total 0.0000 0.000");

        var result = CoaParser.Parse(text);

        Assert.False(result.Terpenes.Terpenes.ContainsKey("Ocimene"));
    }

    [Fact]
    public void Rejects_Digipath_Terpene_Row_When_Math_Does_Not_Validate()
    {
        var text = BuildDigipathTerpeneTable(
            "β-Caryophyllene 87-44-5 0.0100 0.4242 9.999",
            "Total 0.4242 4.242");

        var result = CoaParser.Parse(text);

        Assert.False(result.Terpenes.Terpenes.ContainsKey("β-Caryophyllene"));
    }

    [Fact]
    public void Rejects_Digipath_Terpene_Total_When_Math_Does_Not_Validate()
    {
        var text = BuildDigipathTerpeneTable(
            "β-Caryophyllene 87-44-5 0.0100 0.4242 4.242",
            "Total 1.4474 99.999");

        var result = CoaParser.Parse(text);

        Assert.Equal(0m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void Parses_Terpene_Side_Total_From_Flattened_Dual_Total_Row()
    {
        var text = BuildDigipathTerpeneTable(
            "Total 36.782 367.82 Total 1.4474 14.474");

        var result = CoaParser.Parse(text);

        Assert.Equal(1.4474m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void Parses_Single_Digipath_Terpene_Total_Row()
    {
        var text = BuildDigipathTerpeneTable(
            "Total 1.4474 14.474");

        var result = CoaParser.Parse(text);

        Assert.Equal(1.4474m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void Rejects_Invalid_Terpene_Side_Total_From_Flattened_Dual_Total_Row()
    {
        var text = BuildDigipathTerpeneTable(
            "Total 36.782 367.82 Total 1.4474 99.999");

        var result = CoaParser.Parse(text);

        Assert.Equal(0m, result.Terpenes.TotalTerpenes);
    }

    [Fact]
    public void Flattened_Dual_Total_Row_Does_Not_Create_Total_Terpenes_High_Warning()
    {
        var text = BuildDigipathTerpeneTable(
            "Total 36.782 367.82 Total 1.4474 14.474");

        var result = CoaParser.Parse(text);
        var validation = CannabisCOA.Parser.Core.Validation.CoaValidator.Validate(result);

        Assert.DoesNotContain(validation.Warnings, warning => warning.Code == "TOTAL_TERPENES_HIGH");
    }

    [Fact]
    public void Pesticide_Row_With_Unknown_Microbial_Boundary_Returns_Unknown_Contaminants()
    {
        var text = BuildDigipathPesticideTable(
            "Abamectin 0.44320 0.00000 <LOD Pass Aerobic Bacteria 100 10000 NR NT");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Pesticide_Loq_Row_With_Microbial_Boundary_Passes_Contaminants()
    {
        var text = BuildDigipathPesticideTable(
            "Acequinocyl 0.40000 4.00000 <LOQ Pass Bile-Tolerant Gram-Negative Bacteria 100 1000 <100 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Pesticide_Row_Stops_Before_Right_Side_Aspergillus_Boundary()
    {
        var text = BuildDigipathPesticideTable(
            "Etoxazole 0.40000 0.40000 <LOQ Pass Aspergillus fumigatus Negative Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Multi_Word_Pesticide_Row_Passes_Contaminants()
    {
        var text = BuildDigipathPesticideTable(
            "Piperonyl Butoxide 0.40000 3.00000 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Pesticide_Numeric_Mass_Above_Limit_Fails_Compliance()
    {
        var text = BuildDigipathPesticideTable(
            "Bifenazate 0.40000 0.40000 0.50000 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Pesticide_Explicit_Fail_Status_Fails_Compliance()
    {
        var text = BuildDigipathPesticideTable(
            "Bifenazate 0.40000 0.40000 0.30000 Fail");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Malformed_Pesticide_Row_Does_Not_Guess_From_Microbial_Side()
    {
        var text = BuildDigipathPesticideTable(
            "Bifenazate 0.40000 Aerobic Bacteria 100");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Pesticide_Table_Preserves_Explicit_Overall_Pass()
    {
        var text = BuildDigipathPesticideTableWithOverall(
            "PASS",
            "Abamectin 0.44320 0.00000 <LOD Pass Aerobic Bacteria 100 10000 NR NT");

        var result = CoaParser.Parse(text);

        Assert.True(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("pass", result.Compliance.Status);
    }

    [Fact]
    public void Pesticide_Table_Preserves_Explicit_Overall_Fail()
    {
        var text = BuildDigipathPesticideTableWithOverall(
            "FAIL",
            "Abamectin 0.44320 0.00000 <LOD Pass Aerobic Bacteria 100 10000 NR NT");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Passing_Pesticide_Table_With_Unknown_Overall_Marks_Contaminants_Only()
    {
        var text = BuildDigipathPesticideTable(
            "Acequinocyl 0.40000 4.00000 <LOQ Pass Bile-Tolerant Gram-Negative Bacteria 100 1000 <100 Pass",
            "Piperonyl Butoxide 0.40000 3.00000 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Heavy_Metal_Numeric_Row_Passes_Contaminants()
    {
        var text = BuildDigipathHeavyMetalTable(
            "Arsenic 4.6 2000.0 7.1 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Heavy_Metal_Qualified_Row_Passes_Contaminants()
    {
        var text = BuildDigipathHeavyMetalTable(
            "Cadmium 6.9 820.0 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Full_Digipath_Heavy_Metal_Table_Passes_Contaminants()
    {
        var text = BuildDigipathHeavyMetalTable(
            "Arsenic 4.6 2000.0 7.1 Pass",
            "Cadmium 6.9 820.0 <LOQ Pass",
            "Lead 4.5 1200.0 6.3 Pass",
            "Mercury 3.6 400.0 3.9 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Heavy_Metal_Numeric_Result_Above_Limit_Fails_Compliance()
    {
        var text = BuildDigipathHeavyMetalTable(
            "Arsenic 4.6 2000.0 2500.0 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Heavy_Metal_Explicit_Fail_Status_Fails_Compliance()
    {
        var text = BuildDigipathHeavyMetalTable(
            "Lead 4.5 1200.0 6.3 Fail");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Malformed_Heavy_Metal_Row_Does_Not_Guess_From_Nearby_Text()
    {
        var text = BuildDigipathHeavyMetalTable(
            "Mercury 3.6 Heavy Metals Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Passing_Pesticides_And_Failing_Heavy_Metals_Fail_Compliance()
    {
        var text = BuildDigipathPesticideAndHeavyMetalTable(
            "Acequinocyl 0.40000 4.00000 <LOQ Pass Bile-Tolerant Gram-Negative Bacteria 100 1000 <100 Pass",
            "Arsenic 4.6 2000.0 2500.0 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Heavy_Metal_Table_Preserves_Explicit_Overall_Fail()
    {
        var text = BuildDigipathHeavyMetalTableWithOverall(
            "FAIL",
            "Arsenic 4.6 2000.0 7.1 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Heavy_Metal_Table_Preserves_Explicit_Overall_Pass()
    {
        var text = BuildDigipathHeavyMetalTableWithOverall(
            "PASS",
            "Arsenic 4.6 2000.0 7.1 Pass");

        var result = CoaParser.Parse(text);

        Assert.True(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("pass", result.Compliance.Status);
    }

    [Fact]
    public void Passing_Heavy_Metal_Table_With_Unknown_Overall_Marks_Contaminants_Only()
    {
        var text = BuildDigipathHeavyMetalTable(
            "Arsenic 4.6 2000.0 7.1 Pass",
            "Cadmium 6.9 820.0 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Quantitative_Microbial_Nr_Nt_Row_Is_Unknown()
    {
        var text = BuildDigipathMicrobialTable(
            "Aerobic Bacteria 100 10000 NR NT");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Quantitative_Microbial_Less_Than_Row_Passes()
    {
        var text = BuildDigipathMicrobialTable(
            "Bile-Tolerant Gram-Negative Bacteria 100 1000 <100 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Coliforms_Less_Than_Row_Passes()
    {
        var text = BuildDigipathMicrobialTable(
            "Coliforms 100 1000 <100 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Yeast_And_Mold_Less_Than_Row_Passes()
    {
        var text = BuildDigipathMicrobialTable(
            "Yeast & Mold 100 10000 <100 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Powdery_Mildew_Nr_Nt_Row_Is_Unknown()
    {
        var text = BuildDigipathMicrobialTable(
            "Powdery Mildew 0 NR NT");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Stec_Ecoli_Negative_Row_Passes()
    {
        var text = BuildDigipathMicrobialTable(
            "STEC E. coli Negative Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Salmonella_Negative_Row_Passes()
    {
        var text = BuildDigipathMicrobialTable(
            "Salmonella Negative Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Aspergillus_Negative_Row_Passes()
    {
        var text = BuildDigipathMicrobialTable(
            "Aspergillus niger Negative Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Quantitative_Microbial_Numeric_Result_Above_Limit_Fails_Compliance()
    {
        var text = BuildDigipathMicrobialTable(
            "Coliforms 100 1000 1200 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Quantitative_Microbial_Explicit_Fail_Status_Fails_Compliance()
    {
        var text = BuildDigipathMicrobialTable(
            "Yeast & Mold 100 10000 5000 Fail");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Binary_Microbial_Positive_Result_Fails_Compliance()
    {
        var text = BuildDigipathMicrobialTable(
            "Salmonella Positive Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Binary_Microbial_Detected_Result_Fails_Compliance()
    {
        var text = BuildDigipathMicrobialTable(
            "Aspergillus fumigatus Detected Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Malformed_Microbial_Row_Does_Not_Guess_From_Pesticide_Side()
    {
        var text = BuildDigipathMicrobialTable(
            "Bile-Tolerant Gram-Negative Bacteria 100 Abamectin 0.44320");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Passing_Pesticides_Heavy_Metals_And_Failing_Microbials_Fail_Compliance()
    {
        var text = BuildDigipathPesticideHeavyMetalAndMicrobialTable(
            "Acequinocyl 0.40000 4.00000 <LOQ Pass",
            "Arsenic 4.6 2000.0 7.1 Pass",
            "Salmonella Positive Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Passing_Microbial_Table_With_Unknown_Overall_Marks_Contaminants_Only()
    {
        var text = BuildDigipathMicrobialTable(
            "Bile-Tolerant Gram-Negative Bacteria 100 1000 <100 Pass",
            "Salmonella Negative Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Microbial_Table_With_Unknown_Rows_And_No_Fail_Returns_Unknown_Contaminants()
    {
        var text = BuildDigipathMicrobialTable(
            "Aerobic Bacteria 100 10000 NR NT",
            "Salmonella Negative Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Mycotoxin_Aflatoxins_Qualified_Row_Passes_Contaminants()
    {
        var text = BuildDigipathMycotoxinTable(
            "Aflatoxins 4.90 20.00 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Mycotoxin_Ochratoxin_Qualified_Row_Passes_Contaminants()
    {
        var text = BuildDigipathMycotoxinTable(
            "Ochratoxin A 4.40 20.00 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Full_Digipath_Mycotoxin_Table_Passes_Contaminants()
    {
        var text = BuildDigipathMycotoxinTable(
            "Aflatoxins 4.90 20.00 <LOQ Pass",
            "Ochratoxin A 4.40 20.00 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Mycotoxins_Pass_Section_Label_Does_Not_Set_Overall_Passed()
    {
        var text = """
            Digipath Labs
            Product Type: Flower
            Safety & Quality Tests
            Mycotoxins Pass
            Analyte LOQ Limit Mass Status
            PPB PPB PPB
            Aflatoxins 4.90 20.00 <LOQ Pass
            Residual Solvents Pass
            """;

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Mycotoxin_Numeric_Mass_Above_Limit_Fails_Compliance()
    {
        var text = BuildDigipathMycotoxinTable(
            "Aflatoxins 4.90 20.00 25.00 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Mycotoxin_Explicit_Fail_Status_Fails_Compliance()
    {
        var text = BuildDigipathMycotoxinTable(
            "Ochratoxin A 4.40 20.00 5.00 Fail");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Malformed_Mycotoxin_Row_Does_Not_Guess_From_Nearby_Text()
    {
        var text = BuildDigipathMycotoxinTable(
            "Ochratoxin A 4.40 Mycotoxins Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Mycotoxin_Prose_Does_Not_Create_Compliance_Result()
    {
        var text = """
            Digipath Labs
            Product Type: Flower
            Tested Mycotoxins: aflatoxin B1, aflatoxin B2, aflatoxin G1, aflatoxin G2, & Ochratoxin.
            """;

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.Null(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    [Fact]
    public void Passing_Contaminants_And_Failing_Mycotoxins_Fail_Compliance()
    {
        var text = BuildDigipathAllContaminantsTable(
            "Acequinocyl 0.40000 4.00000 <LOQ Pass",
            "Arsenic 4.6 2000.0 7.1 Pass",
            "Salmonella Negative Pass",
            "Aflatoxins 4.90 20.00 25.00 Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Mycotoxin_Table_Preserves_Explicit_Overall_Fail()
    {
        var text = BuildDigipathMycotoxinTableWithOverall(
            "FAIL",
            "Aflatoxins 4.90 20.00 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.False(result.Compliance.ContaminantsPassed);
        Assert.Equal("fail", result.Compliance.Status);
    }

    [Fact]
    public void Mycotoxin_Table_Preserves_Explicit_Overall_Pass()
    {
        var text = BuildDigipathMycotoxinTableWithOverall(
            "PASS",
            "Aflatoxins 4.90 20.00 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.True(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("pass", result.Compliance.Status);
    }

    [Fact]
    public void Passing_Mycotoxin_Table_With_Unknown_Overall_Marks_Contaminants_Only()
    {
        var text = BuildDigipathMycotoxinTable(
            "Aflatoxins 4.90 20.00 <LOQ Pass",
            "Ochratoxin A 4.40 20.00 <LOQ Pass");

        var result = CoaParser.Parse(text);

        Assert.False(result.Compliance.Passed);
        Assert.True(result.Compliance.ContaminantsPassed);
        Assert.Equal("unknown", result.Compliance.Status);
    }

    private static string BuildDigipathTable(params string[] rows)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Cannabinoid Test Results
            Analyte LOQ Mass Mass Analyte LOQ Mass Mass
            % % mg/g mg/g mg/g %
            {string.Join('\n', rows)}
            Total Potential THC = (THCa * 0.877) + d9-THC + d8-THC
            Overall Result: PASS
            """;
    }

    private static string BuildDigipathTerpeneTable(params string[] rows)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Terpene Test Results
            Analyte CAS LOQ Mass Mass
            % mg/g
            {string.Join('\n', rows)}
            Safety & Quality Tests
            Overall Result: PASS
            """;
    }

    private static string BuildDigipathPesticideTable(params string[] rows)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Safety & Quality Tests
            Pesticides Pass Microbials Pass
            Analyte LOQ Limit Mass Status Analyte LOQ Limit Units Status
            PPM PPM PPM CFU/g CFU/g CFU/g
            {string.Join('\n', rows)}
            Heavy Metals Pass
            """;
    }

    private static string BuildDigipathPesticideTableWithOverall(string overall, params string[] rows)
    {
        return $"""
            {BuildDigipathPesticideTable(rows)}
            Overall Result: {overall}
            """;
    }

    private static string BuildDigipathHeavyMetalTable(params string[] rows)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Safety & Quality Tests
            Heavy Metals Pass
            Analyte LOQ Limit Units Status
            PPB PPB PPB
            {string.Join('\n', rows)}
            Mycotoxins Pass
            """;
    }

    private static string BuildDigipathHeavyMetalTableWithOverall(string overall, params string[] rows)
    {
        return $"""
            {BuildDigipathHeavyMetalTable(rows)}
            Overall Result: {overall}
            """;
    }

    private static string BuildDigipathPesticideAndHeavyMetalTable(string pesticideRow, string heavyMetalRow)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Safety & Quality Tests
            Pesticides Pass Microbials Pass
            Analyte LOQ Limit Mass Status Analyte LOQ Limit Units Status
            PPM PPM PPM CFU/g CFU/g CFU/g
            {pesticideRow}
            Heavy Metals Pass
            Analyte LOQ Limit Units Status
            PPB PPB PPB
            {heavyMetalRow}
            Mycotoxins Pass
            """;
    }

    private static string BuildDigipathMycotoxinTable(params string[] rows)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Safety & Quality Tests
            Mycotoxins
            Pass
            Analyte LOQ Limit Mass Status
            PPB PPB PPB
            {string.Join('\n', rows)}
            Residual Solvents Pass
            """;
    }

    private static string BuildDigipathMycotoxinTableWithOverall(string overall, params string[] rows)
    {
        return $"""
            {BuildDigipathMycotoxinTable(rows)}
            Overall Result: {overall}
            """;
    }

    private static string BuildDigipathMicrobialTable(params string[] rows)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Safety & Quality Tests
            Pesticides Pass Microbials Pass
            Analyte LOQ Limit Mass Status Analyte LOQ Limit Units Status
            PPM PPM PPM CFU/g CFU/g CFU/g
            {string.Join('\n', rows)}
            Heavy Metals Pass
            """;
    }

    private static string BuildDigipathPesticideHeavyMetalAndMicrobialTable(
        string pesticideRow,
        string heavyMetalRow,
        string microbialRow)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Safety & Quality Tests
            Pesticides Pass Microbials Pass
            Analyte LOQ Limit Mass Status Analyte LOQ Limit Units Status
            PPM PPM PPM CFU/g CFU/g CFU/g
            {pesticideRow}
            {microbialRow}
            Heavy Metals Pass
            Analyte LOQ Limit Units Status
            PPB PPB PPB
            {heavyMetalRow}
            Mycotoxins Pass
            """;
    }

    private static string BuildDigipathAllContaminantsTable(
        string pesticideRow,
        string heavyMetalRow,
        string microbialRow,
        string mycotoxinRow)
    {
        return $"""
            Digipath Labs
            Product Type: Flower
            Safety & Quality Tests
            Pesticides Pass Microbials Pass
            Analyte LOQ Limit Mass Status Analyte LOQ Limit Units Status
            PPM PPM PPM CFU/g CFU/g CFU/g
            {pesticideRow}
            {microbialRow}
            Heavy Metals Pass
            Analyte LOQ Limit Units Status
            PPB PPB PPB
            {heavyMetalRow}
            Mycotoxins
            Pass
            Analyte LOQ Limit Mass Status
            PPB PPB PPB
            {mycotoxinRow}
            Residual Solvents Pass
            """;
    }

    [Fact]
    public void DigipathFlowerParser_ParseDocument_ReturnsCoaDocument()
    {
        var text = File.ReadAllText(FixturePath("digipath-flower.txt"));

        var document = DigipathFlowerParser.ParseDocument(
            text,
            "Digipath Labs",
            "digipath-flower.txt");

        Assert.Equal("Digipath Labs", document.LabName);
        Assert.Equal("Flower", document.ProductType);
        Assert.NotEmpty(document.Cannabinoids);
        Assert.Equal("DigipathFlowerParser", document.ParserMetadata.ParserName);
    }

    [Fact]
    public void DigipathFlowerParser_ParseDocument_MapsFlowerCoaV1CoreFields()
    {
        var text = File.ReadAllText(FixturePath("digipath-flower.txt"));

        var document = DigipathFlowerParser.ParseDocument(
            text,
            "Digipath Labs",
            "digipath-flower.txt");

        Assert.Equal("flower-coa-v1", document.SchemaVersion);
        Assert.Equal("Digipath Labs", document.LabName);
        Assert.Equal("Flower", document.ProductType);
        Assert.Equal("digipath-flower.txt", document.ParserMetadata.SourceFileName);
        Assert.Equal("Digipath Labs", document.ParserMetadata.DetectedLab);
        Assert.Equal(nameof(DigipathFlowerParser), document.ParserMetadata.ParserName);
        Assert.NotEmpty(document.Cannabinoids);
        Assert.NotNull(document.TotalThcPercent);
        Assert.True(document.TotalThcPercent > 0m);
        Assert.NotNull(document.TotalTerpenesPercent);
        Assert.True(document.TotalTerpenesPercent > 0m);
        Assert.Contains(document.SafetyResults, result => result.Category == "Overall Compliance");
        Assert.True(document.ParserMetadata.ConfidenceScore > 0m);
    }

    [Fact]
    public void DigipathFlowerParser_ParseDocument_MapsExpectedCannabinoidValues()
    {
        var text = File.ReadAllText(FixturePath("digipath-flower.txt"));

        var document = DigipathFlowerParser.ParseDocument(
            text,
            "Digipath Labs",
            "digipath-flower.txt");

        var thca = FindCannabinoid("THCA");
        var thc = FindCannabinoid("THC");
        var cbda = FindCannabinoid("CBDA");

        Assert.Equal(24.88m, thca.Percent);
        Assert.Equal(0.42m, thc.Percent);
        Assert.Equal(0.12m, cbda.Percent);
        Assert.Equal("%", thca.Unit);
        Assert.Equal("%", thc.Unit);
        Assert.Equal("%", cbda.Unit);
        Assert.False(string.IsNullOrWhiteSpace(thca.SourceText));
        Assert.False(string.IsNullOrWhiteSpace(thc.SourceText));
        Assert.False(string.IsNullOrWhiteSpace(cbda.SourceText));

        CannabisCOA.Parser.Core.Models.CoaAnalyteResult FindCannabinoid(string name)
        {
            return document.Cannabinoids.Single(cannabinoid =>
                cannabinoid.Name == name ||
                cannabinoid.NormalizedName == name);
        }
    }

    [Fact]
    public void DigipathFlowerParser_ParseDocument_CannabinoidPercentMatchesMgPerGram()
    {
        var text = File.ReadAllText(FixturePath("digipath-flower.txt"));

        var document = DigipathFlowerParser.ParseDocument(
            text,
            "Digipath Labs",
            "digipath-flower.txt");

        foreach (var cannabinoid in document.Cannabinoids)
        {
            if (cannabinoid.Percent is null || cannabinoid.MgPerGram is null)
                continue;

            Assert.Equal(cannabinoid.Percent.Value * 10m, cannabinoid.MgPerGram.Value, 2);
        }
    }

    [Fact]
    public void DigipathFlowerParser_ParseDocument_MapsExpectedTotals()
    {
        var text = File.ReadAllText(FixturePath("digipath-flower.txt"));

        var document = DigipathFlowerParser.ParseDocument(
            text,
            "Digipath Labs",
            "digipath-flower.txt");

        Assert.Equal(22.24m, Math.Round(document.TotalThcPercent!.Value, 2));
        Assert.Equal(0.16m, Math.Round(document.TotalCbdPercent!.Value, 2));
        Assert.True(document.TotalTerpenesPercent > 0m);
    }

    [Fact]
    public void DigipathFlowerParser_ParseDocument_TotalCbdMatchesFormula()
    {
        var text = File.ReadAllText(FixturePath("digipath-flower.txt"));

        var document = DigipathFlowerParser.ParseDocument(
            text,
            "Digipath Labs",
            "digipath-flower.txt");

        var cbd = FindCannabinoidPercent("CBD");
        var cbda = FindCannabinoidPercent("CBDA");
        var expectedTotalCbd = cbd + (cbda * 0.877m);

        Assert.Equal(
            Math.Round(expectedTotalCbd, 4),
            Math.Round(document.TotalCbdPercent!.Value, 4));

        decimal FindCannabinoidPercent(string name)
        {
            return document.Cannabinoids.Single(cannabinoid =>
                    cannabinoid.Name == name ||
                    cannabinoid.NormalizedName == name)
                .Percent!.Value;
        }
    }
}
