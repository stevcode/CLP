using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IAnalysis
    {
        List<string> SpreadSheetCodes { get; set; }

        List<IAnalysisCode> QueryCodes { get; set; }
        List<string> SemanticEventIDs { get; set; }
        List<ISemanticEvent> SemanticEvents { get; }
    }
}