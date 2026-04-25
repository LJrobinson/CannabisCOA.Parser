namespace CannabisCOA.Parser.Core.Models;

public class CannabinoidProfile
{
    public ParsedField<decimal> THC { get; set; } = new();
    public ParsedField<decimal> THCA { get; set; } = new();
    public ParsedField<decimal> CBD { get; set; } = new();
    public ParsedField<decimal> CBDA { get; set; } = new();

    public decimal TotalTHC { get; set; }
    public decimal TotalCBD { get; set; }
}