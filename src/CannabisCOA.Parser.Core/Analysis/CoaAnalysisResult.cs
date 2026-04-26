using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Profiles;
using CannabisCOA.Parser.Core.Scoring;
using CannabisCOA.Parser.Core.Validation;

namespace CannabisCOA.Parser.Core.Analysis;

public class CoaAnalysisResult
{
    public CoaResult Coa { get; set; } = new();
    public ValidationResult Validation { get; set; } = new();
    public CoaScoreResult Score { get; set; } = new();
    public TerpeneProfileAnalysis Profile { get; set; } = new();
}