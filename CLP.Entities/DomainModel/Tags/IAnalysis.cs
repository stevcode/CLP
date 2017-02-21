using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IAnalysis
    {
        List<string> AnalysisCodes { get; set; }
        string AnalysisCodesReport { get; }
        List<string> SemanticEventIDs { get; set; }
        List<ISemanticEvent> SemanticEvents { get; }
    }
}