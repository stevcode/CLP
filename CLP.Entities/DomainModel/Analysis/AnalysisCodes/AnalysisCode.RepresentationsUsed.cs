using System.Collections.Generic;
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

        public static void AddRepresentationUsed(RepresentationsUsedTag tag, UsedRepresentation usedRepresentation)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME_LAX, usedRepresentation.CodedObject);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CODED_ID, usedRepresentation.CodedID);

            var historyStatus = usedRepresentation.IsFinalRepresentation ? Codings.CONSTRAINT_VALUE_HISTORY_STATUS_FINAL : Codings.CONSTRAINT_VALUE_HISTORY_STATUS_DELETED;
            analysisCode.AddConstraint(Codings.CONSTRAINT_HISTORY_STATUS, historyStatus);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, Codings.CorrectnessToCodedCorrectness(usedRepresentation.Correctness));

            if (usedRepresentation.Correctness == Correctness.Correct ||
                usedRepresentation.Correctness == Correctness.Unknown)
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON, Codings.NOT_APPLICABLE);
            }
            else
            {
                if (usedRepresentation.CorrectnessReason == Codings.PARTIAL_REASON_GAPS_AND_OVERLAPS ||
                    usedRepresentation.CorrectnessReason == Codings.PARTIAL_REASON_GAPS ||
                    usedRepresentation.CorrectnessReason == Codings.PARTIAL_REASON_OVERLAPS)
                {
                    analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_GAPS_OR_OVERLAPS);
                }
                else if (usedRepresentation.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED)
                {
                    analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_SWAPPED);
                }
                else
                {
                    switch (usedRepresentation.CodedObject)
                    {
                        case Codings.OBJECT_ARRAY:
                            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON,
                                                       Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_INCORRECT_DIMENSIONS);
                            break;
                        case Codings.OBJECT_NUMBER_LINE:
                            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON,
                                                       Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_INCORRECT_JUMPS);
                            break;
                        case Codings.OBJECT_STAMP:
                        case Codings.OBJECT_STAMPED_OBJECT:
                        case Codings.OBJECT_BINS:
                            if (tag.ParentPage.OwnerID == "eO9HFRoY-0aLtcL2iA5-tQ" &&
                                tag.ParentPage.PageNumber == 11)
                            {
                                analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON,
                                                           Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_UNKNOWN_GROUPS);
                            }
                            else
                            {
                                analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON,
                                                           Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_INCORRECT_GROUPS);
                            }
                            break;
                        default:
                            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON, Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_UNKNOWN);
                            break;
                    }
                }
            }

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddRepresentationsUsedSummary(RepresentationsUsedTag tag)
        {
            var typeCount = tag.RepresentationsUsed.Select(r => r.CodedObject).Distinct().Count();
            var repCount = tag.RepresentationsUsed.Count;

            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED_SUMMARY);
            if (tag.RepresentationsUsed.All(r => r.Correctness == Correctness.Correct))
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_OVERALL_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_ALL);
            }
            else if (tag.RepresentationsUsed.All(r => r.Correctness == Correctness.Incorrect))
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_OVERALL_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_NONE);
            }
            else
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_OVERALL_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_SOME);
            }
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_TYPE_COUNT, typeCount.ToString());
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_COUNT, repCount.ToString());

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddIncorrectnessSummary(RepresentationsUsedTag tag)
        {
            var incorrectnessReasons = new List<string>
                                       {
                                           Codings.PARTIAL_REASON_GAPS_AND_OVERLAPS,
                                           Codings.PARTIAL_REASON_GAPS,
                                           Codings.PARTIAL_REASON_OVERLAPS,
                                           Codings.PARTIAL_REASON_SWAPPED
                                       };

            var isFinalInc = tag.RepresentationsUsed.Any(r => !incorrectnessReasons.Contains(r.CorrectnessReason) && r.IsFinalRepresentation);
            var isAnyInc = tag.RepresentationsUsed.Any(r => !incorrectnessReasons.Contains(r.CorrectnessReason));

            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_INCORRECTNESS_REASONS);


            analysisCode.AddConstraint(Codings.CONSTRAINT_FINAL_INCORRECT_REASONS, isFinalInc ? Codings.CONSTRAINT_VALUE_YES : Codings.CONSTRAINT_VALUE_NO);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ALL_INCORRECT_REASONS, isAnyInc ? Codings.CONSTRAINT_VALUE_YES :  Codings.CONSTRAINT_VALUE_NO);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddMultipleRepresentations2Step(RepresentationsUsedTag tag)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP);
            AddMRCorrectnessConstraintValue(analysisCode, tag.RepresentationsUsed);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddMultipleRepresentations1Step(RepresentationsUsedTag tag)
        {
            var leftSideRepresentationTypes =
                tag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_LEFT).Select(r => r.CodedObject).Distinct().ToList();
            var rightSideRepresentationTypes =
                tag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_RIGHT).Select(r => r.CodedObject).Distinct().ToList();
            var alternativeSideRepresentationTypes = tag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_ALTERNATIVE)
                                                        .Select(r => r.CodedObject)
                                                        .Distinct()
                                                        .ToList();
            var unmatchedRepresentationTypes =
                tag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_NONE).Select(r => r.CodedObject).Distinct().ToList();

            if (leftSideRepresentationTypes.Count > 1)
            {
                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP);
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_MATCHED_STEP, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_1);
                AddMRCorrectnessConstraintValue(analysisCode, tag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_LEFT).ToList());
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_TYPES, string.Join(", ", leftSideRepresentationTypes));

                tag.QueryCodes.Add(analysisCode);
            }

            if (rightSideRepresentationTypes.Count > 1)
            {
                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP);
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_MATCHED_STEP, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_2);
                AddMRCorrectnessConstraintValue(analysisCode, tag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_RIGHT).ToList());
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_TYPES, string.Join(", ", rightSideRepresentationTypes));

                tag.QueryCodes.Add(analysisCode);
            }

            if (alternativeSideRepresentationTypes.Count > 1)
            {
                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP);
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_MATCHED_STEP, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_ALT);
                AddMRCorrectnessConstraintValue(analysisCode, tag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_ALTERNATIVE).ToList());
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_TYPES, string.Join(", ", alternativeSideRepresentationTypes));

                tag.QueryCodes.Add(analysisCode);
            }

            if (unmatchedRepresentationTypes.Count > 1)
            {
                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP);
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_MATCHED_STEP, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_NONE);
                AddMRCorrectnessConstraintValue(analysisCode, tag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_NONE).ToList());
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_TYPES, string.Join(", ", unmatchedRepresentationTypes));

                tag.QueryCodes.Add(analysisCode);
            }
        }

        private static void AddMRCorrectnessConstraintValue(AnalysisCode analysisCode, List<UsedRepresentation> representationsUsedForMR)
        {
            if (representationsUsedForMR.All(r => r.Correctness == Correctness.Correct))
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_ALL);
            }
            else if (representationsUsedForMR.All(r => r.Correctness == Correctness.Incorrect))
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_NONE);
            }
            else
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS, Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_SOME);
            }
        }

        public static void AddMultipleApproaches(RepresentationsUsedTag tag)
        {
            var sideGroups = tag.RepresentationsUsed.Where(r => !r.AdditionalInformation.Any(i => i.Contains("Deleted by Snap"))).GroupBy(r => r.MatchedRelationSide).ToList();
            foreach (var sideGroup in sideGroups)
            {
                var side = sideGroup.Key;
                string codedSide;
                switch (side)
                {
                    case Codings.MATCHED_RELATION_LEFT:
                        codedSide = Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_1;
                        break;
                    case Codings.MATCHED_RELATION_RIGHT:
                        codedSide = Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_2;
                        break;
                    case Codings.MATCHED_RELATION_ALTERNATIVE:
                        codedSide = Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_ALT;
                        break;
                    case Codings.MATCHED_RELATION_NONE:
                        codedSide = Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_NONE;
                        break;
                    default:
                        codedSide = Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_MATCHED_STEP_NONE;
                        break;
                }

                var typeGroups = sideGroup.GroupBy(r => r.CodedObject).ToList();
                foreach (var typeGroup in typeGroups)
                {
                    if (typeGroup.Count() <= 1)
                    {
                        continue;
                    }

                    var type = typeGroup.Key;

                    var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_APPROACHES);
                    analysisCode.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_MATCHED_STEP, codedSide);
                    analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME, type);
                    analysisCode.AddConstraint(Codings.CONSTRAINT_COUNT, typeGroup.Count().ToString());

                    tag.QueryCodes.Add(analysisCode);
                }
            }
        }

        public static void AddNLJE(RepresentationsUsedTag tag)
        {
            var nljeCount = tag.RepresentationsUsed.Count(usedRep => usedRep.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
            if (nljeCount <= 0)
            {
                return;
            }

            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_NUMBER_LINE_JUMP_ERASURES);
            analysisCode.AddConstraint(Codings.CONSTRAINT_COUNT, nljeCount.ToString());

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddArrayEquation(IAnalysis tag, UsedRepresentation usedRepresentation)
        {
            var arrayEquationInformation = usedRepresentation.AdditionalInformation.FirstOrDefault(i => i.StartsWith("eqn"));
            if (arrayEquationInformation == null)
            {
                return;
            }
            
            var codedID = usedRepresentation.CodedID;
            var interpretationOnPage = arrayEquationInformation.Substring(4);

            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_ARRAY_EQUATION);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CODED_ID, codedID);
            analysisCode.AddConstraint(Codings.CONSTRAINT_INTERPRETATION, interpretationOnPage);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddArraySkipStrategies(IAnalysis tag, UsedRepresentation usedRepresentation)
        {
            var isSkip = false;
            var isArith = false;
            foreach (var additionalInfo in usedRepresentation.AdditionalInformation.Where(i => i.Contains("skip")))
            {
                isSkip = true;
                var location = additionalInfo.Contains("bottom") ? Codings.CONSTRAINT_VALUE_LOCATION_BOTTOM : Codings.CONSTRAINT_VALUE_LOCATION_RIGHT;

                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP);

                if (additionalInfo.Contains("+arith"))
                {
                    isArith = true;
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

            if (isSkip)
            {
                var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_SKIP_CONSOLIDATION);
                analysisCode.AddConstraint(Codings.CONSTRAINT_ANY_ARITH, isArith ? Codings.CONSTRAINT_VALUE_ARITH_STATUS_PLUS_ARITH : Codings.CONSTRAINT_VALUE_ARITH_STATUS_NO_ARITH);
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