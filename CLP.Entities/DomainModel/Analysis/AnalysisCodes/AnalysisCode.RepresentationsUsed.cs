using System.Linq;

namespace CLP.Entities
{
    public partial class AnalysisCode
    {
        public static void AddRepresentationUsedBlankPage(IAnalysis tag)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME_LAX, Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_BLANK_PAGE);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CODED_ID, Codings.NOT_APPLICABLE);
            analysisCode.AddConstraint(Codings.CONSTRAINT_HISTORY_STATUS, Codings.NOT_APPLICABLE);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, Codings.NOT_APPLICABLE);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddRepresentationUsedInkOnly(IAnalysis tag)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME_LAX, Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_INK_ONLY);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CODED_ID, Codings.NOT_APPLICABLE);
            analysisCode.AddConstraint(Codings.CONSTRAINT_HISTORY_STATUS, Codings.NOT_APPLICABLE);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, Codings.NOT_APPLICABLE);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddRepresentationUsed(IAnalysis tag, UsedRepresentation usedRepresentation)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME_LAX, usedRepresentation.CodedObject);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CODED_ID, usedRepresentation.CodedID);

            var historyStatus = usedRepresentation.IsFinalRepresentation ? Codings.CONSTRAINT_VALUE_HISTORY_STATUS_FINAL : Codings.CONSTRAINT_VALUE_HISTORY_STATUS_DELETED;
            analysisCode.AddConstraint(Codings.CONSTRAINT_HISTORY_STATUS, historyStatus);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, Codings.CorrectnessToCodedCorrectness(usedRepresentation.Correctness));

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddMultipleRepresentations2Step(IAnalysis tag)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddMultipleRepresentations1Step(IAnalysis tag)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddStrategies(IAnalysis tag, UsedRepresentation usedRepresentation)
        {
            foreach (var additionalInfo in usedRepresentation.AdditionalInformation.Where(i => i.Contains("skip")))
            {
                var location = additionalInfo.Contains("bottom") ? Codings.CONSTRAINT_VALUE_LOCATION_BOTTOM : Codings.CONSTRAINT_VALUE_LOCATION_SIDE;

                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP);
                analysisCode.AddConstraint(Codings.CONSTRAINT_ARITH_STATUS, Codings.NOT_APPLICABLE);
                analysisCode.AddConstraint(Codings.CONSTRAINT_LOCATION, location);
                analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS, Codings.NOT_APPLICABLE);
                analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON, Codings.NOT_APPLICABLE);

                tag.QueryCodes.Add(analysisCode);
            }
        }
    }
}