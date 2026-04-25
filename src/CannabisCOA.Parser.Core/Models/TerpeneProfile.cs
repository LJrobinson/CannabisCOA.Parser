namespace CannabisCOA.Parser.Core.Models;

public class TerpeneProfile
{
    public Dictionary<string, decimal> Terpenes { get; set; } = new();

    public decimal TotalTerpenes { get; set; }
}