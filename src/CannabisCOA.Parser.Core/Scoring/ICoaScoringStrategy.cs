using CannabisCOA.Parser.Core.Models;

namespace CannabisCOA.Parser.Core.Scoring;

public interface ICoaScoringStrategy
{
    CoaScoreResult Score(CoaResult coa);
}