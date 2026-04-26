using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Profiles;

public static class TerpeneProfileAnalyzer
{
    public static TerpeneProfileAnalysis Analyze(TerpeneProfile profile)
    {
        if (profile.Terpenes.Count == 0)
        {
            return new TerpeneProfileAnalysis();
        }

        var ordered = profile.Terpenes
            .OrderByDescending(t => t.Value)
            .ToList();

        var dominant = ordered.First().Key;

        return new TerpeneProfileAnalysis
        {
            DominantTerpene = dominant,
            TopTerpenes = ordered.Take(3).Select(t => t.Key).ToList(),
            ProfileType = DetermineProfileType(ordered.Select(t => t.Key).ToList()),
            Lean = DetermineLean(dominant)
        };
    }

    private static string DetermineProfileType(List<string> terpenes)
    {
        if (terpenes.Contains("Beta-Myrcene") && terpenes.Contains("Limonene"))
            return "Earthy / Citrus";

        if (terpenes.Contains("Limonene") && terpenes.Contains("Beta-Caryophyllene"))
            return "Citrus / Spice";

        if (terpenes.Contains("Alpha-Pinene") || terpenes.Contains("Beta-Pinene"))
            return "Pine / Herbal";

        if (terpenes.Contains("Linalool"))
            return "Floral";

        return "Mixed";
    }

    private static string DetermineLean(string dominantTerpene)
    {
        return dominantTerpene switch
        {
            "Beta-Myrcene" => "Indica-Leaning",
            "Limonene" => "Hybrid-Leaning",
            "Terpinolene" => "Sativa-Leaning",
            "Alpha-Pinene" => "Sativa-Leaning",
            "Beta-Pinene" => "Sativa-Leaning",
            "Linalool" => "Indica-Leaning",
            _ => "Unknown"
        };
    }
}