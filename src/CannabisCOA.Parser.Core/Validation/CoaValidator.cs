using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Validation;

public static class CoaValidator
{
    public static ValidationResult Validate(CoaResult coa)
    {
        var result = new ValidationResult();

        if (coa.IsAmended)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "AMENDED_COA",
                Message = "COA is marked as amended, revised, or corrected.",
                Severity = "warning"
            });
        }

        if (coa.Cannabinoids.TotalTHC > 40m)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "TOTAL_THC_HIGH",
                Message = "Total THC is unusually high.",
                Severity = "warning"
            });
        }

        if (coa.Cannabinoids.TotalCBD > 100m)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "TOTAL_CBD_HIGH",
                Message = "Total CBD is unusually high.",
                Severity = "warning"
            });
        }

        if (coa.Terpenes.TotalTerpenes > 25m)
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