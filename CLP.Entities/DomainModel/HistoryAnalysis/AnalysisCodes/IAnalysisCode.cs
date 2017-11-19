using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CLP.Entities
{
    public interface IAnalysisCode
    {
        string Alias { get; set; }
        string AnalysisLabel { get; set; }
        ObservableCollection<AnalysisConstraint> ConstraintValues { get; set; }
        string FormattedValue { get; }

        void AddConstraint(string constraintLabel, string constraintValue);
    }
}