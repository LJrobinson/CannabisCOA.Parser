using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Validation;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaValidatorTests
{
    private static CoaResult BuildCoaWithTotalThc(decimal totalThc)
    {
        return new CoaResult
        {
            TestDate = new DateTime(2026, 1, 1),
            Cannabinoids = new CannabinoidProfile
            {
                THC = new ParsedField<decimal>
                {
                    FieldName = "THC",
                    Value = 1m,
                    Confidence = 0.95m
                },
                TotalTHC = totalThc
            }
        };
    }

    [Fact]
    public void Flags_High_Total_THC()
    {
        var coa = new CoaResult
        {
            Cannabinoids = new CannabinoidProfile
            {
                TotalTHC = 42.5m
            }
        };

        var result = CoaValidator.Validate(coa);

        Assert.Contains(result.Warnings, w => w.Code == "TOTAL_THC_HIGH");
    }

    [Fact]
    public void Does_Not_Flag_TotalThcHigh_When_TotalThc_Is_Normal()
    {
        var coa = BuildCoaWithTotalThc(28m);

        var result = CoaValidator.Validate(coa);

        Assert.DoesNotContain(result.Warnings, w => w.Code == "TOTAL_THC_HIGH");
    }

    [Fact]
    public void Does_Not_Flag_TotalThcHigh_When_TotalThc_Equals_Threshold()
    {
        var coa = BuildCoaWithTotalThc(40m);

        var result = CoaValidator.Validate(coa);

        Assert.DoesNotContain(result.Warnings, w => w.Code == "TOTAL_THC_HIGH");
    }

    [Fact]
    public void Flags_TotalThcHigh_When_TotalThc_Exceeds_Threshold()
    {
        var coa = BuildCoaWithTotalThc(40.01m);

        var result = CoaValidator.Validate(coa);

        Assert.Contains(result.Warnings, w => w.Code == "TOTAL_THC_HIGH");
    }

    [Fact]
    public void TotalThcHigh_Warning_Has_Stable_Code_Message_And_Severity()
    {
        var coa = BuildCoaWithTotalThc(42.5m);

        var result = CoaValidator.Validate(coa);
        var warning = Assert.Single(result.Warnings, w => w.Code == "TOTAL_THC_HIGH");

        Assert.Equal("TOTAL_THC_HIGH", warning.Code);
        Assert.Equal("Total THC is unusually high.", warning.Message);
        Assert.Equal("warning", warning.Severity);
    }

    [Fact]
    public void Warnings_Make_ValidationResult_Invalid()
    {
        var coa = BuildCoaWithTotalThc(42.5m);

        var result = CoaValidator.Validate(coa);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Flags_High_Total_Terpenes()
    {
        var coa = new CoaResult
        {
            Terpenes = new TerpeneProfile
            {
                TotalTerpenes = 26.2m
            }
        };

        var result = CoaValidator.Validate(coa);

        Assert.Contains(result.Warnings, w => w.Code == "TOTAL_TERPENES_HIGH");
    }

    [Fact]
    public void Flags_Missing_Test_Date()
    {
        var coa = new CoaResult();

        var result = CoaValidator.Validate(coa);

        Assert.Contains(result.Warnings, w => w.Code == "MISSING_TEST_DATE");
    }

    [Fact]
    public void Flags_Compliance_Fail()
    {
        var coa = new CoaResult
        {
            Compliance = new ComplianceResult
            {
                Status = "fail",
                Passed = false
            }
        };

        var result = CoaValidator.Validate(coa);

        Assert.Contains(result.Warnings, w => w.Code == "COMPLIANCE_FAIL");
    }
}
