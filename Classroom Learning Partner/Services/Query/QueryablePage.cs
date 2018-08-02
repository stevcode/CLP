using System;
using System.Collections.Generic;
using System.Linq;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public class QueryablePage
    {
        public QueryablePage()
        {
            MatchingAnalysisCodes = new List<IAnalysisCode>();
            AllAnalysisCodes = new List<IAnalysisCode>();
        }

        public string CacheFilePath { get; set; }
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public CLPPage.NameComposite PageNameComposite { get; set; }
        public List<IAnalysisCode> MatchingAnalysisCodes { get; set; }
        public List<IAnalysisCode> AllAnalysisCodes { get; set; }

        public string FormattedValue
        {
            get { return $"Page {PageNameComposite.PageNumber}, {StudentName}\n - {string.Join("\n - ", MatchingAnalysisCodes.Select(q => q.FormattedValue))}"; }
        }

        #region Methods

        public double Distance(QueryablePage otherPage)
        {
            var distance = 0.0;

            // Dist v 1, Label Count
            //var analysisCodeTypes = AllAnalysisCodes.Select(c => c.AnalysisCodeLabel).Distinct().ToList();
            //var analysisCodeTypesOther = otherPage.AllAnalysisCodes.Select(c => c.AnalysisCodeLabel).Distinct().ToList();
            //distance += Math.Abs(analysisCodeTypes.Count - analysisCodeTypesOther.Count);

            // Dist v 2, ???
            distance += ConstraintSimilarityDistance(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_REPRESENTATION_NAME_LAX);
            distance += ConstraintSimilarityDistance(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_HISTORY_STATUS);
            distance += ConstraintSimilarityDistance(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS);

            // Dist v 3, Pure Hamming between all code constraint values on both pages
            //distance = ALL_PURE(this, otherPage);

            return distance;
        }

        private static double ALL_PURE(QueryablePage page1, QueryablePage page2)
        {
            var distance = 0.0;
            foreach (var code1 in page1.AllAnalysisCodes)
            {
                foreach (var code2 in page2.AllAnalysisCodes)
                {
                    distance += HammingDistance(code1, code2);
                }
            }

            return distance;
        }

        private static double HammingDistance(IAnalysisCode code1, IAnalysisCode code2)
        {
            if (code1.AnalysisCodeLabel != code2.AnalysisCodeLabel)
            {
                return Math.Max(code1.Constraints.Count, code2.Constraints.Count);
            }

            if (code1.Constraints.Count != code2.Constraints.Count)
            {
                // Print error?
                return Math.Max(code1.Constraints.Count, code2.Constraints.Count);
            }

            var distance = 0;
            for (var i = 0; i < code1.Constraints.Count; i++)
            {
                var constraint1 = code1.Constraints[i];
                var constraint2 = code2.Constraints[i];

                if (constraint1.ConstraintValue != constraint2.ConstraintValue)
                {
                    distance += 1;
                }
            }

            return distance;
        }

        private static double ConstraintSimilarityDistance(QueryablePage page1, QueryablePage page2, string analysisCodeLabel, string constraintLabel)
        {
            var codes = page1.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();
            var codesOther = page2.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();

            var groups = codes.Select(c => c.Constraints.First(con => con.ConstraintLabel == constraintLabel).ConstraintValue).GroupBy(t => t);
            var groupsOther = codesOther.Select(c => c.Constraints.First(con => con.ConstraintLabel == constraintLabel).ConstraintValue).GroupBy(t => t);

            var groupings = groups.ToDictionary(@group => @group.Key, @group => @group.Count());
            var groupingsOther = groupsOther.ToDictionary(@group => @group.Key, @group => @group.Count());

            var distance = 0.0;
            foreach (var groupingsKey in groupings.Keys)
            {
                if (groupingsOther.ContainsKey(groupingsKey))
                {
                    distance += Math.Abs(groupings[groupingsKey] - groupingsOther[groupingsKey]);
                }
                else
                {
                    distance += groupings[groupingsKey];
                }
            }

            return distance;
        }

        #endregion // Methods
    }
}
