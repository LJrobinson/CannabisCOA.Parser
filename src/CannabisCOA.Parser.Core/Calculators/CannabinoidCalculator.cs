using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Calculators;

public static class CannabinoidCalculator
{
    public static void CalculateTotals(CannabinoidProfile c)
    {
        c.TotalTHC = c.THC.Value + (c.THCA.Value * 0.877m);
        c.TotalCBD = c.CBD.Value + (c.CBDA.Value * 0.877m);
    }
}