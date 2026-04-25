using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Validation;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CoaValidatorTests
{
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
    public void Flags_High_Total_Terpenes()
    {
        var coa = new CoaResult
        {
            Terpenes = new TerpeneProfile
            {
                TotalTerpenes = 6.2m
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