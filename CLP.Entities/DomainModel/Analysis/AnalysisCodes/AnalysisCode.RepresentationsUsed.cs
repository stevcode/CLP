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

            if (usedRepresentation.Correctness == Correctness.Correct)
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON, Codings.NOT_APPLICABLE);
            }
            else
            {
                if (usedRepresentation.CorrectnessReason == Codings.PARTIAL_REASON_GAPS_AND_OVERLAPS)
                {
                    analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_GAPS_OR_OVERLAPS);
                }
                else if (usedRepresentation.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED)
                {
                    analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_SWAPPED);
                }
                else
                {
                    analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_UNKNOWN);
                }
            }

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddMultipleRepresentations2Step(RepresentationsUsedTag tag)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP);
            AddMRCorrectnessConstraintValue(tag, analysisCode);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddMultipleRepresentations1Step(RepresentationsUsedTag tag)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP);
            AddMRCorrectnessConstraintValue(tag, analysisCode);

            tag.QueryCodes.Add(analysisCode);
        }

        private static void AddMRCorrectnessConstraintValue(RepresentationsUsedTag tag, AnalysisCode analysisCode)
        {
            if (tag.RepresentationsUsed.All(r => r.Correctness == Correctness.Correct))
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_ALL);
            }
            else if (tag.RepresentationsUsed.All(r => r.Correctness == Correctness.Incorrect))
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_NONE);
            }
            else
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_SOME);
            }
        }

        public static void AddArraySkipStrategies(IAnalysis tag, UsedRepresentation usedRepresentation)
        {
            foreach (var additionalInfo in usedRepresentation.AdditionalInformation.Where(i => i.Contains("skip")))
            {
                var location = additionalInfo.Contains("bottom") ? Codings.CONSTRAINT_VALUE_LOCATION_BOTTOM : Codings.CONSTRAINT_VALUE_LOCATION_RIGHT;

                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP);

                if (additionalInfo.Contains("+arith"))
                {
                    analysisCode.AddConstraint(Codings.CONSTRAINT_ARITH_STATUS, Codings.CONSTRAINT_VALUE_ARITH_STATUS_PLUS_ARITH);
                }
                else
                {
                    analysisCode.AddConstraint(Codings.CONSTRAINT_ARITH_STATUS, Codings.CONSTRAINT_VALUE_ARITH_STATUS_NO_ARITH);
                }
                
                analysisCode.AddConstraint(Codings.CONSTRAINT_LOCATION, location);

                if (additionalInfo.Contains("bottom"))
                {
                    if (additionalInfo.Contains("correct"))
                    {
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS, Codings.CORRECTNESS_CODED_CORRECT);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON, Codings.NOT_APPLICABLE);
                    }
                    else
                    {
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS, Codings.CORRECTNESS_CODED_PARTIAL);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_WRONG_DIMENSION);
                    }
                }
                else
                {
                    if (additionalInfo.Contains("No Heuristic Adjustment Possible") ||
                        additionalInfo.Contains("Heuristic Adjustment Error"))
                    {
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS, Codings.CORRECTNESS_CODED_INCORRECT);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_UNKNOWN);
                    }
                    else if (additionalInfo.Contains("Skip Counted by Wrong Dimension"))
                    {
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS, Codings.CORRECTNESS_CODED_PARTIAL);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_WRONG_DIMENSION);
                    }
                    else if (additionalInfo.Contains("Likely arithmetic error"))
                    {
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS, Codings.CORRECTNESS_CODED_PARTIAL);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_LIKELY_ARITH_ERROR);
                    }
                    else
                    {
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS, Codings.CORRECTNESS_CODED_CORRECT);
                        analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON, Codings.NOT_APPLICABLE);
                    }
                }

                tag.QueryCodes.Add(analysisCode);
            }
        }

        public static void AddArrayPartialProductStrategies(IAnalysis tag, UsedRepresentation usedRepresentation)
        {
            if (string.IsNullOrWhiteSpace(usedRepresentation.RepresentationInformation) &&
                !usedRepresentation.AdditionalInformation.Any(a => a.Contains("Created by Snap")))
            {
                return;
            }

            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_PARTIAL_PRODUCT);

            if (usedRepresentation.AdditionalInformation.Any(a => a.Contains("Total Ink Divides")))
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_TECHNIQUE, Codings.CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_INK_DIVIDE);
            }
            else if (usedRepresentation.AdditionalInformation.Any(a => a.Contains("Created by Snap")))
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_TECHNIQUE, Codings.CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_CUT_AND_SNAP);
            }
            else
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_TECHNIQUE, Codings.CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_DIVIDE);
            }

            analysisCode.AddConstraint(Codings.CONSTRAINT_STRATEGY_FRIENDLY_NUMBERS, Codings.NOT_APPLICABLE);

            tag.QueryCodes.Add(analysisCode);
        }
    }
}