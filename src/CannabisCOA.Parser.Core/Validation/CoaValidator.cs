using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Models;
using System.Linq;

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

        if (coa.DocumentClassification.Equals("SinglePanelTest", StringComparison.OrdinalIgnoreCase))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "SINGLE_PANEL_TEST",
                Message = "COA appears to be a single-panel report, not a full compliance COA.",
                Severity = "warning"
            });
        }
        else if (!coa.IsFullComplianceCoa ||
            coa.DocumentClassification.Equals("PartialPanelReport", StringComparison.OrdinalIgnoreCase))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "PARTIAL_PANEL_REPORT",
                Message = "COA appears to be a partial-panel report, not a full compliance COA.",
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

        if (profile.TotalTerpenesHighThreshold is { } totalTerpenesHighThreshold &&
            coa.Terpenes.TotalTerpenes > totalTerpenesHighThreshold)
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

        // Terpene total vs breakdown consistency (flower-only for now)
        if ((coa.ProductType == ProductType.Flower ||
            coa.ProductType == ProductType.PreRoll ||
            coa.ProductType == ProductType.Unknown) &&
            coa.Terpenes?.TotalTerpenes > 0m &&
            coa.Terpenes.Terpenes != null &&
            coa.Terpenes.Terpenes.Count > 0)
        {
            var sum = coa.Terpenes.Terpenes.Values.Sum();

            if (Math.Abs(sum - coa.Terpenes.TotalTerpenes) > 0.25m)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "TERPENE_TOTAL_MISMATCH",
                    Message = "Total terpenes does not match sum of individual terpenes.",
                    Severity = "warning"
                });
            }
        }

        // THC total consistency check (flower-like products only)
        if ((coa.ProductType == ProductType.Flower ||
            coa.ProductType == ProductType.PreRoll ||
            coa.ProductType == ProductType.Unknown) &&
            coa.Cannabinoids != null)
        {
            var cannabinoids = coa.Cannabinoids;

            var thc = cannabinoids.THC?.Value ?? 0m;
            var thca = cannabinoids.THCA?.Value ?? 0m;
            var totalThc = cannabinoids.TotalTHC;

            // Standard decarboxylation formula
            var calculatedTotalThc = thc + (thca * 0.877m);

            if ((thc > 0m || thca > 0m) &&
                Math.Abs(calculatedTotalThc - totalThc) > 1.0m)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "TOTAL_THC_MISMATCH",
                    Message = "Total THC does not match THC + THCA calculation.",
                    Severity = "warning"
                });
            }
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

        if (coa.IsFullComplianceCoa &&
            (coa.Cannabinoids == null ||
            ((coa.Cannabinoids.THCA?.Value ?? 0m) == 0m &&
            (coa.Cannabinoids.THC?.Value ?? 0m) == 0m)))
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
            _ => new ValidationProfile()
        };
    }
}
