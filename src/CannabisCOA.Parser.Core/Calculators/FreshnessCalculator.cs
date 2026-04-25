using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Calculators;

public static class FreshnessCalculator
{
    public static FreshnessResult Calculate(DateTime? testDate)
    {
        if (testDate == null)
        {
            return new FreshnessResult
            {
                DaysSinceTest = null,
                Score = 0,
                Band = "Unknown"
            };
        }

        var days = (DateTime.UtcNow - testDate.Value).Days;

        var band = days switch
        {
            <= 30 => "Excellent",
            <= 90 => "Good",
            <= 180 => "Aging",
            _ => "Old"
        };

        var score = days switch
        {
            <= 30 => 100,
            <= 60 => 85,
            <= 90 => 70,
            <= 120 => 50,
            <= 180 => 30,
            _ => 10
        };

        return new FreshnessResult
        {
            DaysSinceTest = days,
            Score = score,
            Band = band
        };
    }
}