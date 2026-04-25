using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class CannabinoidCalculatorTests
{
    [Fact]
    public void Calculates_Total_THC_Correctly()
    {
        var profile = new CannabinoidProfile
        {
            THC = new ParsedField<decimal> { FieldName = "THC", Value = 0.42m },
            THCA = new ParsedField<decimal> { FieldName = "THCA", Value = 24.88m }
        };

        CannabinoidCalculator.CalculateTotals(profile);

        Assert.Equal(22.24m, Math.Round(profile.TotalTHC, 2));
    }
}