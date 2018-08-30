using System.Collections.Generic;
using System.Linq;
using Catel;

namespace CLP.Entities
{
    public partial class AnalysisCode
    {
        public static void AddIsWordProblem(IAnalysis tag, bool isWordProblem)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_WORD_PROBLEM);
            analysisCode.AddConstraint(Codings.CONSTRAINT_IS_WORD_PROBLEM, isWordProblem ? Codings.CONSTRAINT_VALUE_YES : Codings.CONSTRAINT_VALUE_NO);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddPageDefinition(IAnalysis tag, List<IDefinition> definitionTags)
        {
            foreach (var definitionTag in definitionTags)
            {
                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_PAGE_DEFINITION);

                switch (definitionTag) {
                    case MultiplicationRelationDefinitionTag _:
                        analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_TYPE, Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_MULTIPLICATION);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_STEP_COUNT, 1.ToString());
                        break;
                    case DivisionRelationDefinitionTag _:
                        analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_TYPE, Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_DIVISION);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_STEP_COUNT, 1.ToString());
                        break;
                    case EquivalenceRelationDefinitionTag _:
                        analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_TYPE, Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_EQUIVALENCE);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_STEP_COUNT, 2.ToString());
                        break;
                    default:
                        analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_TYPE, Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_OTHER);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_STEP_COUNT, 2.ToString());
                        break;
                }

                tag.QueryCodes.Add(analysisCode);
            }

            if (!definitionTags.Any())
            {
                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_PAGE_DEFINITION);
                analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_TYPE, Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_NONE);
                analysisCode.AddConstraint(Codings.CONSTRAINT_PROBLEM_STEP_COUNT, Codings.CONSTRAINT_VALUE_PROBLEM_STEP_COUNT_UNKNOWN);
                tag.QueryCodes.Add(analysisCode);
            }
        }
    }
}
