﻿using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IAnalysisCode
    {
        string Alias { get; set; }
        string AnalysisLabel { get; set; }
        List<AnalysisConstraint> ConstraintValues { get; set; }
        string FormattedValue { get; }

        void AddConstraint(string constraintLabel, string constraintValue);
    }
}