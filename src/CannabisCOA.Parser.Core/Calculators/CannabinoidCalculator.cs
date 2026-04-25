using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Calculators;

public static class CannabinoidCalculator
{
    public static void CalculateTotals(CannabinoidProfile c)
    {
        c.TotalTHC = c.THC + (c.THCA * 0.877m);
        c.TotalCBD = c.CBD + (c.CBDA * 0.877m);
    }
}