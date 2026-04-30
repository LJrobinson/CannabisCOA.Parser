using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Validation;

public static class CoaValidator
{
    public static ValidationResult Validate(CoaResult coa)
    {
        var result = new ValidationResult();
        var profile = ResolveProfile(coa.ProductType);

        if (coa.IsAmended)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "AMENDED_COA",
                Message = "COA is marked as amended, revised, or corrected.",
                Severity = "warning"
            });
        }

        if (profile.TotalThcHighThreshold is { } totalThcHighThreshold &&
            coa.Cannabinoids.TotalTHC > totalThcHighThreshold)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "TOTAL_THC_HIGH",
                Message = "Total THC is unusually high.",
                Severity = "warning"
            });
        }

        if (profile.TotalCbdHighThreshold is { } totalCbdHighThreshold &&
            coa.Cannabinoids.TotalCBD > totalCbdHighThreshold)
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

        if (coa.Terpenes?.TotalTerpenes > 0m &&
            (coa.Terpenes.Terpenes == null || coa.Terpenes.Terpenes.Count == 0))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "TERPENE_BREAKDOWN_MISSING",
                Message = "Total terpenes reported without individual breakdown.",
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

    private static ValidationProfile ResolveProfile(ProductType productType)
    {
        return productType switch
        {
            ProductType.Unknown or ProductType.Flower or ProductType.PreRoll => new ValidationProfile
            {
                TotalThcHighThreshold = 40m,
                TotalCbdHighThreshold = 100m,
                TotalTerpenesHighThreshold = 25m
            },
            _ => new ValidationProfile
            {
                TotalTerpenesHighThreshold = 25m
            }
        };
    }
}
