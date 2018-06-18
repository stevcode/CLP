using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CLP.Entities
{
    public interface IAnalysisCode
    {
        string AnalysisCodeLabel { get; }
        ObservableCollection<AnalysisConstraint> Constraints { get; }
        string AnalysisCodeName { get; }
        string AnalysisCodeShortName { get; }
        List<string> ConstraintLabels { get; }
        string FormattedValue { get; }

        void AddConstraint(string constraintLabel, string constraintValue);
    }
}