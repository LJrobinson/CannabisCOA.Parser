using CannabisCOA.Parser.Core.Scoring;
using CannabisCOA.Parser.Core.Validation;

namespace CannabisCOA.Parser.Core.Analysis;

public static class CoaAnalyzer
{
    public static CoaAnalysisResult Analyze(string text)
    {
        var coa = CoaParser.Parse(text);
        var validation = CoaValidator.Validate(coa);
        var score = CoaScorer.Score(coa);

        return new CoaAnalysisResult
        {
            Coa = coa,
            Validation = validation,
            Score = score
        };
    }
}