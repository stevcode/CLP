using System.Linq;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    

    public class AnalysisService
    {
        public static void GenerateAnalysisEntryForPage(CLPPage page)
        {
            #region Page Identification

            var entry = new AnalysisEntry(page.Owner.FullName, page.PageNumber);
            entry.SubmissionTime = page.SubmissionTime == null ? AnalysisEntry.UNSUBMITTED : $"{page.SubmissionTime:yyyy-MM-dd HH:mm:ss}";

            #endregion // Page Identification

            #region Set up variables

            var pass3Event = page.History.SemanticEvents.FirstOrDefault(h => h.CodedObject == "PASS" && h.CodedObjectID == "3");
            var pass3Index = page.History.SemanticEvents.IndexOf(pass3Event);
            var pass3 = page.History.SemanticEvents.Skip(pass3Index + 1).ToList();

            #endregion // Set up variables

            #region Problem Characteristics

            var pageDefinition = page.Tags.FirstOrDefault(t => t.Category == Category.Definition);
            if (pageDefinition == null)
            {
                entry.ProblemType = AnalysisEntry.NONE;
                entry.LeftSideOperation = AnalysisEntry.NA;
                entry.RightSideOperation = AnalysisEntry.NA;
                entry.DivisionType = AnalysisEntry.NA;
                entry.IsMultiplicationProblemUsingGroups = AnalysisEntry.NA;
            }
            else if (pageDefinition is AdditionRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_2_PART;
                var additionDefinition = pageDefinition as AdditionRelationDefinitionTag;
                var leftSide = additionDefinition.Addends[0];
                var rightSide = additionDefinition.Addends[1];

                if (leftSide is DivisionRelationDefinitionTag)
                {
                    entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                }
                else if (leftSide is MultiplicationRelationDefinitionTag)
                {
                    entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                }
                else
                {
                    entry.LeftSideOperation = AnalysisEntry.NA;
                }

                if (rightSide is DivisionRelationDefinitionTag)
                {
                    entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                }
                else if (rightSide is MultiplicationRelationDefinitionTag)
                {
                    entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                }
                else
                {
                    entry.RightSideOperation = AnalysisEntry.NA;
                }

                entry.DivisionType = AnalysisEntry.NA;
                entry.IsMultiplicationProblemUsingGroups = AnalysisEntry.NA;
            }
            else if (pageDefinition is EquivalenceRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_EQUIVALENCE;
                var equivalenceDefinition = pageDefinition as EquivalenceRelationDefinitionTag;
                if (equivalenceDefinition.LeftRelationPart is DivisionRelationDefinitionTag)
                {
                    var divisionDefinition = equivalenceDefinition.LeftRelationPart as DivisionRelationDefinitionTag;
                    var dividend = divisionDefinition.Dividend as NumericValueDefinitionTag;
                    var divisor = divisionDefinition.Divisor as NumericValueDefinitionTag;
                    if (dividend.IsNotGiven)
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_DIVIDEND;
                    }
                    else if (divisor.IsNotGiven)
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_DIVISOR;
                    }
                    else
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                    }
                }
                else if (equivalenceDefinition.LeftRelationPart is MultiplicationRelationDefinitionTag)
                {
                    var multiplicationDefinition = equivalenceDefinition.LeftRelationPart as MultiplicationRelationDefinitionTag;
                    var firstFactor = multiplicationDefinition.Factors[0] as NumericValueDefinitionTag;
                    var secondFactor = multiplicationDefinition.Factors[1] as NumericValueDefinitionTag;
                    if (firstFactor.IsNotGiven)
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_FIRST_FACTOR;
                    }
                    else if (secondFactor.IsNotGiven)
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_LAST_FACTOR;
                    }
                    else
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                    }
                }
                else
                {
                    entry.LeftSideOperation = AnalysisEntry.NA;
                }

                if (equivalenceDefinition.RightRelationPart is DivisionRelationDefinitionTag)
                {
                    var divisionDefinition = equivalenceDefinition.RightRelationPart as DivisionRelationDefinitionTag;
                    var dividend = divisionDefinition.Dividend as NumericValueDefinitionTag;
                    var divisor = divisionDefinition.Divisor as NumericValueDefinitionTag;
                    if (dividend.IsNotGiven)
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_DIVIDEND;
                    }
                    else if (divisor.IsNotGiven)
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_DIVISOR;
                    }
                    else
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                    }
                }
                else if (equivalenceDefinition.RightRelationPart is MultiplicationRelationDefinitionTag)
                {
                    var multiplicationDefinition = equivalenceDefinition.RightRelationPart as MultiplicationRelationDefinitionTag;
                    var firstFactor = multiplicationDefinition.Factors[0] as NumericValueDefinitionTag;
                    var secondFactor = multiplicationDefinition.Factors[1] as NumericValueDefinitionTag;
                    if (firstFactor.IsNotGiven)
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_FIRST_FACTOR;
                    }
                    else if (secondFactor.IsNotGiven)
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_LAST_FACTOR;
                    }
                    else
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                    }
                }
                else
                {
                    entry.RightSideOperation = AnalysisEntry.NA;
                }

                entry.DivisionType = AnalysisEntry.NA;
                entry.IsMultiplicationProblemUsingGroups = AnalysisEntry.NA;
            }
            else if (pageDefinition is MultiplicationRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_1_PART;
                entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                entry.RightSideOperation = AnalysisEntry.NA;
                entry.DivisionType = AnalysisEntry.NA;

                var multiplicationDefinition = pageDefinition as MultiplicationRelationDefinitionTag;
                if (multiplicationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups)
                {
                    entry.IsMultiplicationProblemUsingGroups = AnalysisEntry.YES;
                }
                else
                {
                    entry.IsMultiplicationProblemUsingGroups = AnalysisEntry.NO;
                }
            }
            else if (pageDefinition is DivisionRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_1_PART;
                entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                entry.RightSideOperation = AnalysisEntry.NA;

                var divisionDefinition = pageDefinition as DivisionRelationDefinitionTag;
                if (divisionDefinition.RelationType == DivisionRelationDefinitionTag.RelationTypes.Partitive)
                {
                    entry.DivisionType = AnalysisEntry.DIVISION_TYPE_PARTATIVE;
                }
                else if (divisionDefinition.RelationType == DivisionRelationDefinitionTag.RelationTypes.Quotative)
                {
                    entry.DivisionType = AnalysisEntry.DIVISION_TYPE_QUOTATIVE;
                }
                else
                {
                    entry.DivisionType = AnalysisEntry.DIVISION_TYPE_GENERAL;
                }

                entry.IsMultiplicationProblemUsingGroups = AnalysisEntry.NA;
            }
            else
            {
                entry.ProblemType = AnalysisEntry.NONE;
                entry.LeftSideOperation = AnalysisEntry.NA;
                entry.RightSideOperation = AnalysisEntry.NA;
                entry.DivisionType = AnalysisEntry.NA;
                entry.IsMultiplicationProblemUsingGroups = AnalysisEntry.NA;
            }


            var metaDataViewModel = new MetaDataTagsViewModel(page);
            entry.WordType = metaDataViewModel.IsWordProblem ? AnalysisEntry.WORD_TYPE_WORD : AnalysisEntry.WORD_TYPE_NON_WORD;



            switch (metaDataViewModel.DifficultyLevel)
            {
                case MetaDataTagsViewModel.DifficultyLevels.None:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_NONE;
                    break;
                case MetaDataTagsViewModel.DifficultyLevels.Easy:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_EASY;
                    break;
                case MetaDataTagsViewModel.DifficultyLevels.Medium:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_MEDIUM;
                    break;
                case MetaDataTagsViewModel.DifficultyLevels.Hard:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_HARD;
                    break;
                default:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_NONE;
                    break;
            }

            if (pageDefinition == null)
            {
                entry.PageDefinitionEquation = AnalysisEntry.NONE;
            }
            else
            {
                var equivalenceDefinition = pageDefinition as EquivalenceRelationDefinitionTag;
                if (equivalenceDefinition != null)
                {
                    entry.PageDefinitionEquation = $"{equivalenceDefinition.LeftRelationPart.ExpandedFormattedRelation} = {equivalenceDefinition.RightRelationPart.ExpandedFormattedRelation}";
                }

                var relationPartDefinition = pageDefinition as IRelationPart;
                if (relationPartDefinition != null)
                {
                    entry.PageDefinitionEquation = relationPartDefinition.ExpandedFormattedRelation;
                    var additionDefinition = pageDefinition as AdditionRelationDefinitionTag;
                    if (additionDefinition != null &&
                        !string.IsNullOrWhiteSpace(additionDefinition.AlternateFormattedRelation))
                    {
                        entry.PageDefinitionEquation += $" AND {additionDefinition.AlternateFormattedRelation}";
                    }
                }
            }


            entry.IsMultipleChoiceBoxOnPage = page.PageObjects.OfType<MultipleChoice>().Any() ? AnalysisEntry.YES : AnalysisEntry.NO;

            if (metaDataViewModel.IsArrayRequired)
            {
                entry.RequiredRepresentations = "ARR";
            }
            else if (metaDataViewModel.IsNumberLineRequired)
            {
                entry.RequiredRepresentations = "NL";
            }
            else if (metaDataViewModel.IsStampRequired)
            {
                entry.RequiredRepresentations = "ST";
            }
            else if (metaDataViewModel.IsArrayOrNumberLineRequired)
            {
                entry.RequiredRepresentations = "ARR or NL";
            }
            else if (metaDataViewModel.IsArrayAndStampRequired)
            {
                entry.RequiredRepresentations = "ARR&ST";
            }
            else
            {
                entry.RequiredRepresentations = AnalysisEntry.NONE;
            }

            if (metaDataViewModel.IsCommutativeEquivalence)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_CE);
            }
            if (metaDataViewModel.IsMultiplicationWithZero)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_ZERO);
            }
            if (metaDataViewModel.IsScaffolded)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_SCAF);
            }
            if (metaDataViewModel.Is2PSF)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_2PSF);
            }
            if (metaDataViewModel.Is2PSS)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_2PSS);
            }
            if (!entry.SpecialInterestGroups.Any())
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.NONE);
            }

            #endregion // Problem Characteristics

            #region Whole Page Characteristics

            var representationsUsedTag = page.Tags.OfType<RepresentationsUsedTag>().FirstOrDefault();
            if (representationsUsedTag == null)
            {
                entry.IsInkOnly = AnalysisEntry.UNKOWN_ERROR;
                entry.IsBlank = AnalysisEntry.UNKOWN_ERROR;
            }
            else
            {
                switch (representationsUsedTag.RepresentationsUsedType)
                {
                    case RepresentationsUsedTypes.BlankPage:
                        entry.IsInkOnly = AnalysisEntry.NO;
                        entry.IsBlank = AnalysisEntry.YES;
                        break;
                    case RepresentationsUsedTypes.InkOnly:
                        entry.IsInkOnly = AnalysisEntry.YES;
                        entry.IsBlank = AnalysisEntry.NO;
                        break;
                    case RepresentationsUsedTypes.RepresentationsUsed:
                        entry.IsInkOnly = AnalysisEntry.NO;
                        entry.IsBlank = AnalysisEntry.NO;
                        break;
                    default:
                        entry.IsInkOnly = AnalysisEntry.UNKOWN_ERROR;
                        entry.IsBlank = AnalysisEntry.UNKOWN_ERROR;
                        break;
                }
            }

            entry.ArrayDeletedCount = pass3.Count(e => e.CodedObject == Codings.OBJECT_ARRAY && e.EventType == Codings.EVENT_OBJECT_DELETE);
            entry.NumberLineCreatedCount = pass3.Count(e => e.CodedObject == Codings.OBJECT_NUMBER_LINE && e.EventType == Codings.EVENT_OBJECT_ADD);
            entry.NumberLineDeletedCount = pass3.Count(e => e.CodedObject == Codings.OBJECT_NUMBER_LINE && e.EventType == Codings.EVENT_OBJECT_DELETE);
            entry.StampDeletedCount = pass3.Count(e => e.CodedObject == Codings.OBJECT_STAMP && e.EventType == Codings.EVENT_OBJECT_DELETE);
            entry.IndividualStampImageDeletedCount = pass3.Count(e => e.CodedObject == Codings.OBJECT_STAMPED_OBJECTS && e.EventType == Codings.EVENT_OBJECT_DELETE);

            #endregion // Whole Page Characteristics

            #region Left Side

            var leftRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_LEFT).ToList();

            #region Number Lines

            var leftNumberLines = leftRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var leftUsedNumberLines = leftNumberLines.Where(r => r.IsUsed).ToList();

            entry.LeftNumberLineUsedCount = leftUsedNumberLines.Count;

            if (entry.LeftNumberLineUsedCount == 0)
            {
                entry.LeftNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = leftUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.LeftNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.LeftNumberLineSwitched = leftUsedNumberLines.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            entry.LeftNumberLineBlank = leftNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH)) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Number Lines

            #region Stamps

            var leftStampImages = leftRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            foreach (var usedRepresentation in leftStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("From"));
                var parentStampParts = parentStampAdditionalInfo.Split(' ');
                if (parentStampParts.Length == 3)
                {
                    var parentStampCount = (int)parentStampParts[1].ToInt();
                    entry.LeftStampCreatedCount += parentStampCount;
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.LeftStampImagesCreatedCount += stampImageCount;
                }
            }

            entry.LeftStampImagesSwitched = leftStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            entry.LeftRepresentationsAndCorrectness = leftRepresentations.Select(r => $"{r.CodedObject} [{r.CodedID}] {Codings.CorrectnessToCoding(r.Correctness)}").ToList();

            entry.IsLeftMR = leftRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Left Side

            #region Right Side

            var rightRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_RIGHT).ToList();

            #region Number Lines

            var rightNumberLines = rightRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var rightUsedNumberLines = rightNumberLines.Where(r => r.IsUsed).ToList();

            entry.RightNumberLineUsedCount = rightUsedNumberLines.Count;

            if (entry.RightNumberLineUsedCount == 0)
            {
                entry.RightNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = rightUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.RightNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.RightNumberLineSwitched = rightUsedNumberLines.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            entry.RightNumberLineBlank = rightNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH)) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Number Lines

            #region Stamps

            var rightStampImages = rightRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            foreach (var usedRepresentation in rightStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("From"));
                var parentStampParts = parentStampAdditionalInfo.Split(' ');
                if (parentStampParts.Length == 3)
                {
                    var parentStampCount = (int)parentStampParts[1].ToInt();
                    entry.RightStampCreatedCount += parentStampCount;
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.RightStampImagesCreatedCount += stampImageCount;
                }
            }

            entry.RightStampImagesSwitched = rightStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            entry.RightRepresentationsAndCorrectness = rightRepresentations.Select(r => $"{r.CodedObject} [{r.CodedID}] {Codings.CorrectnessToCoding(r.Correctness)}").ToList();

            entry.IsRightMR = rightRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Right Side

            #region Alternative Side

            var alternativeRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_ALTERNATIVE).ToList();

            #region Number Lines

            var alternativeNumberLines = alternativeRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var alternativeUsedNumberLines = alternativeNumberLines.Where(r => r.IsUsed).ToList();

            entry.AlternativeNumberLineUsedCount = alternativeUsedNumberLines.Count;

            if (entry.AlternativeNumberLineUsedCount == 0)
            {
                entry.AlternativeNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = alternativeUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.AlternativeNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.AlternativeNumberLineSwitched = alternativeUsedNumberLines.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            entry.AlternativeNumberLineBlank = alternativeNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH)) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Number Lines

            #region Stamps

            var alternativeStampImages = alternativeRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            foreach (var usedRepresentation in alternativeStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("From"));
                var parentStampParts = parentStampAdditionalInfo.Split(' ');
                if (parentStampParts.Length == 3)
                {
                    var parentStampCount = (int)parentStampParts[1].ToInt();
                    entry.AlternativeStampCreatedCount += parentStampCount;
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.AlternativeStampImagesCreatedCount += stampImageCount;
                }
            }

            entry.AlternativeStampImagesSwitched = alternativeStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            entry.AlternativeRepresentationsAndCorrectness = alternativeRepresentations.Select(r => $"{r.CodedObject} [{r.CodedID}] {Codings.CorrectnessToCoding(r.Correctness)}").ToList();

            entry.IsAlternativeMR = alternativeRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Alternative Side

            #region Unmatched

            var unmatchedRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_NONE).ToList();

            #region Number Lines

            var unmatchedNumberLines = unmatchedRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var unmatchedUsedNumberLines = unmatchedNumberLines.Where(r => r.IsUsed).ToList();

            entry.UnmatchedNumberLineUsedCount = unmatchedUsedNumberLines.Count;

            if (entry.UnmatchedNumberLineUsedCount == 0)
            {
                entry.UnmatchedNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = unmatchedUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.UnmatchedNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.UnmatchedNumberLineSwitched = unmatchedUsedNumberLines.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            entry.UnmatchedNumberLineBlank = unmatchedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH)) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Number Lines

            #region Stamps

            var unmatchedStampImages = unmatchedRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            foreach (var usedRepresentation in unmatchedStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("From"));
                var parentStampParts = parentStampAdditionalInfo.Split(' ');
                if (parentStampParts.Length == 3)
                {
                    var parentStampCount = (int)parentStampParts[1].ToInt();
                    entry.UnmatchedStampCreatedCount += parentStampCount;
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.UnmatchedStampImagesCreatedCount += stampImageCount;
                }
            }

            entry.UnmatchedStampImagesSwitched = unmatchedStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            entry.UnmatchedRepresentationsAndCorrectness = unmatchedRepresentations.Select(r => $"{r.CodedObject} [{r.CodedID}] {Codings.CorrectnessToCoding(r.Correctness)}").ToList();

            entry.IsUnmatchedMR = unmatchedRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Unmatched

            #region Whole Page Analysis

            entry.IsMR2STEP = AnalysisEntry.NA;
            if (representationsUsedTag != null &&
                representationsUsedTag.RepresentationsUsedType == RepresentationsUsedTypes.RepresentationsUsed &&
                entry.ProblemType != AnalysisEntry.PROBLEM_TYPE_1_PART)
            {
                var isMR2STEP = representationsUsedTag.AnalysisCodes.Contains(Codings.REPRESENTATIONS_MR2STEP);
                entry.IsMR2STEP = isMR2STEP ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            var finalAnswerCorrectnessTag = page.Tags.OfType<FinalAnswerCorrectnessTag>().FirstOrDefault();
            if (finalAnswerCorrectnessTag == null)
            {
                entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
            }
            else
            {
                switch (finalAnswerCorrectnessTag.FinalAnswerCorrectness)
                {
                    case Codings.CORRECTNESS_CORRECT:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_CORRECT;
                        break;
                    case Codings.CORRECTNESS_INCORRECT:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_INCORRECT;
                        break;
                    case Codings.CORRECTNESS_PARTIAL:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_PARTIAL;
                        break;
                    default:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
                        break;
                }
            }

            var correctnessSummaryTag = page.Tags.OfType<CorrectnessTag>().FirstOrDefault();
            if (correctnessSummaryTag == null)
            {
                entry.CorrectnessSummary = AnalysisEntry.CORRECTNESS_UNKNOWN;
            }
            else
            {
                switch (correctnessSummaryTag.Correctness)
                {
                    case Correctness.Correct:
                        entry.CorrectnessSummary = AnalysisEntry.CORRECTNESS_CORRECT;
                        break;
                    case Correctness.Incorrect:
                        entry.CorrectnessSummary = AnalysisEntry.CORRECTNESS_INCORRECT;
                        break;
                    case Correctness.PartiallyCorrect:
                        entry.CorrectnessSummary = AnalysisEntry.CORRECTNESS_PARTIAL;
                        break;
                    default:
                        entry.CorrectnessSummary = AnalysisEntry.CORRECTNESS_UNKNOWN;
                        break;
                }
            }

            var studentInkStrokes = page.InkStrokes.Concat(page.History.TrashedInkStrokes).Where(s => s.GetStrokeOwnerID() == page.Owner.ID).ToList();
            var colorsUsed = studentInkStrokes.Select(s => s.DrawingAttributes.Color).Distinct();
            entry.InkColorsUsedCount = colorsUsed.Count();

            #endregion // Whole Page Analysis

            #region Total History

            var pass3CodedValues = pass3.Select(h => h.CodedValue).ToList();
            entry.FinalSemanticEvents = pass3CodedValues;

            #endregion // Total History
        }
    }
}
