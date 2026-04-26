using CannabisCOA.Parser.Core.Parsers;
using CannabisCOA.Parser.Core.Profiles;
using CannabisCOA.Parser.Core.Scoring;
using CannabisCOA.Parser.Core.Validation;

namespace CannabisCOA.Parser.Core.Analysis;

public static class CoaAnalyzer
{
    public static CoaAnalysisResult Analyze(string text)
    {
        var coa = CoaParser.Parse(text);

        coa.IsAmended = CoaMetadataParser.IsAmended(text);

        var validation = CoaValidator.Validate(coa);
        var score = CoaScorer.Score(coa);
        var profile = TerpeneProfileAnalyzer.Analyze(coa.Terpenes);

        return new CoaAnalysisResult
        {
            Coa = coa,
            Validation = validation,
            Score = score,
            Profile = profile
        };
    }
}