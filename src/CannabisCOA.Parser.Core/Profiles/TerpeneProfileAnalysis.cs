namespace CannabisCOA.Parser.Core.Profiles;

public class TerpeneProfileAnalysis
{
    public string DominantTerpene { get; set; } = "";
    public List<string> TopTerpenes { get; set; } = new();
    public string ProfileType { get; set; } = "Unknown";
    public string Lean { get; set; } = "Unknown";
}