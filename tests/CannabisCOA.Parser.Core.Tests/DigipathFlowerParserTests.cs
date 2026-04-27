using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class DigipathFlowerParserTests
{
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
        Assert.True(result.Cannabinoids.CBD.Confidence > 0m);
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
}
