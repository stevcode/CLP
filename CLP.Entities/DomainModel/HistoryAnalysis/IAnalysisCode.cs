using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IAnalysisCode
    {
        string Alias { get; set; }
        string AnalysisLabel { get; set; }
        List<AnalysisCode.AnalysisConstraint> ConstraintValues { get; set; }
        string FormattedValue { get; }
    }
}