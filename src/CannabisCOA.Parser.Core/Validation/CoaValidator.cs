using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Validation;

public static class CoaValidator
{
    public static ValidationResult Validate(CoaResult coa)
    {
        var result = new ValidationResult();

        if (coa.Cannabinoids.TotalTHC > 40m)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "TOTAL_THC_HIGH",
                Message = "Total THC is unusually high.",
                Severity = "warning"
            });
        }

        if (coa.Terpenes.TotalTerpenes > 5m)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "TOTAL_TERPENES_HIGH",
                Message = "Total terpenes are unusually high.",
                Severity = "warning"
            });
        }

        if (coa.TestDate == null)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "MISSING_TEST_DATE",
                Message = "Test date was not found.",
                Severity = "warning"
            });
        }

        if (coa.Cannabinoids.THCA.Value == 0m && coa.Cannabinoids.THC.Value == 0m)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "MISSING_THC_VALUES",
                Message = "THC and THCA values were not found.",
                Severity = "warning"
            });
        }

        if (coa.Compliance.Status == "fail")
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "COMPLIANCE_FAIL",
                Message = "COA indicates a failed compliance result.",
                Severity = "critical"
            });
        }

        return result;
    }
}