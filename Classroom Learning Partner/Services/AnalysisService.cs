using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public class AnalysisService
    {
        public static void RunAnalysis(Notebook notebook)
        {
            var analysisRows = new List<string>();
            foreach (var page in notebook.Pages)
            {
                //if (page.PageNumber != 276)
                //{
                //    continue;
                //}

                HistoryAnalysis.GenerateSemanticEvents(page);
                var analysisEntry = GenerateAnalysisEntryForPage(page);
                var analysisRow = analysisEntry.BuildEntryLine();
                analysisRows.Add(analysisRow);
            }

            var desktopDirectory = DataService.DesktopFolderPath;
            var filePath = Path.Combine(desktopDirectory, "BatchAnalysis.tsv");
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "");

                var headerRow = AnalysisEntry.BuildHeaderEntryLine();
                File.AppendAllText(filePath, headerRow);
            }

            foreach (var analysisRow in analysisRows)
            {
                File.AppendAllText(filePath, Environment.NewLine + analysisRow);
            }
        }

        public static AnalysisEntry GenerateAnalysisEntryForPage(CLPPage page)
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
            }
            else if (pageDefinition is MultiplicationRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_1_PART;
                entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                entry.RightSideOperation = AnalysisEntry.NA;
                entry.DivisionType = AnalysisEntry.NA;
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
            }
            else
            {
                entry.ProblemType = AnalysisEntry.NONE;
                entry.LeftSideOperation = AnalysisEntry.NA;
                entry.RightSideOperation = AnalysisEntry.NA;
                entry.DivisionType = AnalysisEntry.NA;
            }


            var metaDataViewModel = new MetaDataTagsViewModel(page);
            entry.WordType = metaDataViewModel.IsWordProblem ? AnalysisEntry.WORD_TYPE_WORD : AnalysisEntry.WORD_TYPE_NON_WORD;

            entry.IsMultipleChoiceBoxOnPage = page.PageObjects.OfType<MultipleChoice>().Any() ? AnalysisEntry.YES : AnalysisEntry.NO;

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

            entry.ArrayDeletedCount = representationsUsedTag.RepresentationsUsed.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && !r.IsFinalRepresentation && r.IsUsed);
            entry.NumberLineDeletedCount = representationsUsedTag.RepresentationsUsed.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && !r.IsFinalRepresentation && r.IsUsed);
            entry.IndividualStampImageDeletedCount = pass3.Count(e => e.CodedObject == Codings.OBJECT_STAMPED_OBJECT && e.EventType == Codings.EVENT_OBJECT_DELETE);
            entry.StampImageRepresentationDeletedCount = representationsUsedTag.RepresentationsUsed.Count(r => r.CodedObject == Codings.OBJECT_STAMP && !r.IsFinalRepresentation && r.IsUsed);

            #endregion // Whole Page Characteristics

            #region Left Side

            var stampIDsOnPage = page.PageObjects.OfType<Stamp>().Select(s => s.ID).ToList();

            var leftRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_LEFT).ToList();

            #region Arrays

            var leftArrays = leftRepresentations.Where(r => r.CodedObject == Codings.OBJECT_ARRAY).ToList();
            var leftUsedArrays = leftArrays.Where(r => r.IsUsed).ToList();

            entry.LeftArrayCreatedCount = leftUsedArrays.Count;

            entry.LeftArrayCutCount = leftUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Deleted by Cut")));

            entry.LeftArraySnapCount = leftUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Created by Snap")));

            entry.LeftArrayDivideCount = leftUsedArrays.Select(r => r.RepresentationInformation.Count(c => c == ',')).Sum();

            var leftArraysWithSkips = leftUsedArrays.Where(r => r.AdditionalInformation.Any(a => a.Contains("skip"))).ToList();

            entry.LeftArraySkipCount = leftArraysWithSkips.Count;

            foreach (var usedRepresentation in leftArraysWithSkips)
            {
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = usedRepresentation.CodedID;
                var skipInformations = usedRepresentation.AdditionalInformation.Where(a => a.Contains("skip")).ToList();

                var skips = new List<string>();
                foreach (var skipInformation in skipInformations)
                {
                    var skipCorrectness = string.Empty;
                    if (skipInformation.Contains("correct"))
                    {
                        skipCorrectness = "C";
                    }
                    else if (skipInformation.Contains("wrong dimension"))
                    {
                        skipCorrectness = "WD";
                    }
                    else
                    {
                        skipCorrectness = "O";
                    }

                    var skipSide = skipInformation.Contains("bottom") ? "bottom" : "right";
                    var skip = $"{skipSide}, {skipCorrectness}";
                    skips.Add(skip);
                }

                entry.LeftArraySkipCountingCorretness.Add($"{codedObject} [{codedID}] {string.Join("; ", skips)}");
            }

            #endregion // Arrays

            #region Number Lines

            var leftNumberLines = leftRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var leftUsedNumberLines = leftNumberLines.Where(r => r.IsUsed).ToList();

            entry.LeftNumberLineCreatedCount = leftUsedNumberLines.Count;

            if (entry.LeftNumberLineCreatedCount == 0)
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

            var leftStampIDsCreated = new List<string>();
            var leftStampIDsDeleted = new List<string>();
            foreach (var usedRepresentation in leftStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (PSID)"));
                var parentStampInfoParts = parentStampAdditionalInfo.Split(" - ");
                if (parentStampInfoParts.Length == 2)
                {
                    var parentStampIDsComposite = parentStampInfoParts[1];
                    var parentStampIDs = parentStampIDsComposite.Split(" ; ").ToList();
                    leftStampIDsCreated.AddRange(parentStampIDs);
                    leftStampIDsDeleted.AddRange(parentStampIDs.Where(id => !stampIDsOnPage.Contains(id)));
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.LeftStampImagesCreatedCount += stampImageCount;
                }

                var companionStampedObjectAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (COID)"));
                var companionStampedObjectInfoParts = companionStampedObjectAdditionalInfo.Split(" - ");
                if (companionStampedObjectInfoParts.Length == 2)
                {
                    var companionStampedObjectIDsComposite = parentStampInfoParts[1];
                    var companionStampedObjectIDs = companionStampedObjectIDsComposite.Split(" ; ").ToList();
                    entry.LeftStampImagesCreatedCount += companionStampedObjectIDs.Count;
                }
            }

            entry.StampCreatedCount += leftStampIDsCreated.Count;
            entry.StampDeletedCount += leftStampIDsDeleted.Count;

            entry.LeftStampImagesSwitched = leftStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            #region Representation Correctness Counts

            entry.LeftArrayCorrectCount = leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.Correct);
            entry.LeftArrayPartiallyCorrectCount = leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.PartiallyCorrect);

            entry.LeftNumberLineCorrectCount = leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Correct);
            entry.LeftNumberLinePartiallyCorrectCount =
                leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.LeftNumberLinePartiallyCorrectSwappedCount =
                leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            entry.LeftStampCorrectCount = leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.Correct);
            entry.LeftStampPartiallyCorrectCount =
                leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.LeftStampPartiallyCorrectSwappedCount =
                leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            #endregion // Representation Correctness Counts

            entry.LeftRepresentationsAndCorrectness =
                leftRepresentations.Select(
                                           r =>
                                                   $"{r.CodedObject} [{r.CodedID}] {(r.CodedObject == Codings.OBJECT_STAMP ? r.RepresentationInformation : string.Empty)}{(r.CodedObject == Codings.OBJECT_NUMBER_LINE ? r.RepresentationInformation : string.Empty)} {Codings.CorrectnessToCodedCorrectness(r.Correctness)}")
                                   .ToList();

            entry.IsLeftMR = leftRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Left Side

            #region Right Side

            var rightRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_RIGHT).ToList();

            #region Arrays

            var rightArrays = rightRepresentations.Where(r => r.CodedObject == Codings.OBJECT_ARRAY).ToList();
            var rightUsedArrays = rightArrays.Where(r => r.IsUsed).ToList();

            entry.RightArrayCreatedCount = rightUsedArrays.Count;

            entry.RightArrayCutCount = rightUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Deleted by Cut")));

            entry.RightArraySnapCount = rightUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Created by Snap")));

            entry.RightArrayDivideCount = rightUsedArrays.Select(r => r.RepresentationInformation.Count(c => c == ',')).Sum();

            var rightArraysWithSkips = rightUsedArrays.Where(r => r.AdditionalInformation.Any(a => a.Contains("skip"))).ToList();

            entry.RightArraySkipCount = rightArraysWithSkips.Count;

            foreach (var usedRepresentation in rightArraysWithSkips)
            {
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = usedRepresentation.CodedID;
                var skipInformations = usedRepresentation.AdditionalInformation.Where(a => a.Contains("skip")).ToList();

                var skips = new List<string>();
                foreach (var skipInformation in skipInformations)
                {
                    var skipCorrectness = string.Empty;
                    if (skipInformation.Contains("correct"))
                    {
                        skipCorrectness = "C";
                    }
                    else if (skipInformation.Contains("wrong dimension"))
                    {
                        skipCorrectness = "WD";
                    }
                    else
                    {
                        skipCorrectness = "O";
                    }

                    var skipSide = skipInformation.Contains("bottom") ? "bottom" : "right";
                    var skip = $"{skipSide}, {skipCorrectness}";
                    skips.Add(skip);
                }

                entry.RightArraySkipCountingCorretness.Add($"{codedObject} [{codedID}] {string.Join("; ", skips)}");
            }

            #endregion // Arrays

            #region Number Lines

            var rightNumberLines = rightRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var rightUsedNumberLines = rightNumberLines.Where(r => r.IsUsed).ToList();

            entry.RightNumberLineCreatedCount = rightUsedNumberLines.Count;

            if (entry.RightNumberLineCreatedCount == 0)
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

            var rightStampIDsCreated = new List<string>();
            var rightStampIDsDeleted = new List<string>();
            foreach (var usedRepresentation in rightStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (PSID)"));
                var parentStampInfoParts = parentStampAdditionalInfo.Split(" - ");
                if (parentStampInfoParts.Length == 2)
                {
                    var parentStampIDsComposite = parentStampInfoParts[1];
                    var parentStampIDs = parentStampIDsComposite.Split(" ; ").ToList();
                    rightStampIDsCreated.AddRange(parentStampIDs);
                    rightStampIDsDeleted.AddRange(parentStampIDs.Where(id => !stampIDsOnPage.Contains(id)));
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.RightStampImagesCreatedCount += stampImageCount;
                }

                var companionStampedObjectAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (COID)"));
                var companionStampedObjectInfoParts = companionStampedObjectAdditionalInfo.Split(" - ");
                if (companionStampedObjectInfoParts.Length == 2)
                {
                    var companionStampedObjectIDsComposite = parentStampInfoParts[1];
                    var companionStampedObjectIDs = companionStampedObjectIDsComposite.Split(" ; ").ToList();
                    entry.RightStampImagesCreatedCount += companionStampedObjectIDs.Count;
                }
            }

            entry.StampCreatedCount += rightStampIDsCreated.Count;
            entry.StampDeletedCount += rightStampIDsDeleted.Count;

            entry.RightStampImagesSwitched = rightStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            #region Representation Correctness Counts

            entry.RightArrayCorrectCount = rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.Correct);
            entry.RightArrayPartiallyCorrectCount = rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.PartiallyCorrect);

            entry.RightNumberLineCorrectCount = rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Correct);
            entry.RightNumberLinePartiallyCorrectCount =
                rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.RightNumberLinePartiallyCorrectSwappedCount =
                rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            entry.RightStampCorrectCount = rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.Correct);
            entry.RightStampPartiallyCorrectCount =
                rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.RightStampPartiallyCorrectSwappedCount =
                rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            #endregion // Representation Correctness Counts

            entry.RightRepresentationsAndCorrectness =
                rightRepresentations.Select(
                                            r =>
                                                    $"{r.CodedObject} [{r.CodedID}] {(r.CodedObject == Codings.OBJECT_STAMP ? r.RepresentationInformation : string.Empty)}{(r.CodedObject == Codings.OBJECT_NUMBER_LINE ? r.RepresentationInformation : string.Empty)} {Codings.CorrectnessToCodedCorrectness(r.Correctness)}")
                                    .ToList();

            entry.IsRightMR = rightRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Right Side

            #region Alternative Side

            var alternativeRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_ALTERNATIVE).ToList();

            #region Arrays

            var alternativeArrays = alternativeRepresentations.Where(r => r.CodedObject == Codings.OBJECT_ARRAY).ToList();
            var alternativeUsedArrays = alternativeArrays.Where(r => r.IsUsed).ToList();

            entry.AlternativeArrayCreatedCount = alternativeUsedArrays.Count;

            entry.AlternativeArrayCutCount = alternativeUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Deleted by Cut")));

            entry.AlternativeArraySnapCount = alternativeUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Created by Snap")));

            entry.AlternativeArrayDivideCount = alternativeUsedArrays.Select(r => r.RepresentationInformation.Count(c => c == ',')).Sum();

            var alternativeArraysWithSkips = alternativeUsedArrays.Where(r => r.AdditionalInformation.Any(a => a.Contains("skip"))).ToList();

            entry.AlternativeArraySkipCount = alternativeArraysWithSkips.Count;

            foreach (var usedRepresentation in alternativeArraysWithSkips)
            {
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = usedRepresentation.CodedID;
                var skipInformations = usedRepresentation.AdditionalInformation.Where(a => a.Contains("skip")).ToList();

                var skips = new List<string>();
                foreach (var skipInformation in skipInformations)
                {
                    var skipCorrectness = string.Empty;
                    if (skipInformation.Contains("correct"))
                    {
                        skipCorrectness = "C";
                    }
                    else if (skipInformation.Contains("wrong dimension"))
                    {
                        skipCorrectness = "WD";
                    }
                    else
                    {
                        skipCorrectness = "O";
                    }

                    var skipSide = skipInformation.Contains("bottom") ? "bottom" : "right";
                    var skip = $"{skipSide}, {skipCorrectness}";
                    skips.Add(skip);
                }

                entry.AlternativeArraySkipCountingCorretness.Add($"{codedObject} [{codedID}] {string.Join("; ", skips)}");
            }

            #endregion // Arrays

            #region Number Lines

            var alternativeNumberLines = alternativeRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var alternativeUsedNumberLines = alternativeNumberLines.Where(r => r.IsUsed).ToList();

            entry.AlternativeNumberLineCreatedCount = alternativeUsedNumberLines.Count;

            if (entry.AlternativeNumberLineCreatedCount == 0)
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

            var alternativeStampIDsCreated = new List<string>();
            var alternativeStampIDsDeleted = new List<string>();
            foreach (var usedRepresentation in alternativeStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (PSID)"));
                var parentStampInfoParts = parentStampAdditionalInfo.Split(" - ");
                if (parentStampInfoParts.Length == 2)
                {
                    var parentStampIDsComposite = parentStampInfoParts[1];
                    var parentStampIDs = parentStampIDsComposite.Split(" ; ").ToList();
                    alternativeStampIDsCreated.AddRange(parentStampIDs);
                    alternativeStampIDsDeleted.AddRange(parentStampIDs.Where(id => !stampIDsOnPage.Contains(id)));
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.AlternativeStampImagesCreatedCount += stampImageCount;
                }

                var companionStampedObjectAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (COID)"));
                var companionStampedObjectInfoParts = companionStampedObjectAdditionalInfo.Split(" - ");
                if (companionStampedObjectInfoParts.Length == 2)
                {
                    var companionStampedObjectIDsComposite = parentStampInfoParts[1];
                    var companionStampedObjectIDs = companionStampedObjectIDsComposite.Split(" ; ").ToList();
                    entry.AlternativeStampImagesCreatedCount += companionStampedObjectIDs.Count;
                }
            }

            entry.StampCreatedCount += alternativeStampIDsCreated.Count;
            entry.StampDeletedCount += alternativeStampIDsDeleted.Count;

            entry.AlternativeStampImagesSwitched = alternativeStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            #region Representation Correctness Counts

            entry.AlternativeArrayCorrectCount = alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.Correct);
            entry.AlternativeArrayPartiallyCorrectCount = alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.PartiallyCorrect);

            entry.AlternativeNumberLineCorrectCount = alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Correct);
            entry.AlternativeNumberLinePartiallyCorrectCount =
                alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.AlternativeNumberLinePartiallyCorrectSwappedCount =
                alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            entry.AlternativeStampCorrectCount = alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.Correct);
            entry.AlternativeStampPartiallyCorrectCount =
                alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.AlternativeStampPartiallyCorrectSwappedCount =
                alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            #endregion // Representation Correctness Counts

            entry.AlternativeRepresentationsAndCorrectness =
                alternativeRepresentations.Select(
                                                  r =>
                                                          $"{r.CodedObject} [{r.CodedID}] {(r.CodedObject == Codings.OBJECT_STAMP ? r.RepresentationInformation : string.Empty)}{(r.CodedObject == Codings.OBJECT_NUMBER_LINE ? r.RepresentationInformation : string.Empty)} {Codings.CorrectnessToCodedCorrectness(r.Correctness)}")
                                          .ToList();

            entry.IsAlternativeMR = alternativeRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Alternative Side

            #region Unmatched

            var unmatchedRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_NONE).ToList();

            #region Arrays

            var unmatchedArrays = unmatchedRepresentations.Where(r => r.CodedObject == Codings.OBJECT_ARRAY).ToList();
            var unmatchedUsedArrays = unmatchedArrays.Where(r => r.IsUsed).ToList();

            entry.UnmatchedArrayCreatedCount = unmatchedUsedArrays.Count;

            entry.UnmatchedArrayCutCount = unmatchedUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Deleted by Cut")));

            entry.UnmatchedArraySnapCount = unmatchedUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Created by Snap")));

            entry.UnmatchedArrayDivideCount = unmatchedUsedArrays.Select(r => r.RepresentationInformation.Count(c => c == ',') ).Sum();

            var unmatchedArraysWithSkips = unmatchedUsedArrays.Where(r => r.AdditionalInformation.Any(a => a.Contains("skip"))).ToList();

            entry.UnmatchedArraySkipCount = unmatchedArraysWithSkips.Count;

            foreach (var usedRepresentation in unmatchedArraysWithSkips)
            {
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = usedRepresentation.CodedID;
                var skipInformations = usedRepresentation.AdditionalInformation.Where(a => a.Contains("skip")).ToList();

                var skips = new List<string>();
                foreach (var skipInformation in skipInformations)
                {
                    var skipCorrectness = string.Empty;
                    if (skipInformation.Contains("correct"))
                    {
                        skipCorrectness = "C";
                    }
                    else if (skipInformation.Contains("wrong dimension"))
                    {
                        skipCorrectness = "WD";
                    }
                    else
                    {
                        skipCorrectness = "O";
                    }

                    var skipSide = skipInformation.Contains("bottom") ? "bottom" : "right";
                    var skip = $"{skipSide}, {skipCorrectness}";
                    skips.Add(skip);
                }

                entry.UnmatchedArraySkipCountingCorretness.Add($"{codedObject} [{codedID}] {string.Join("; ", skips)}");
            }

            #endregion // Arrays

            #region Number Lines

            var unmatchedNumberLines = unmatchedRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var unmatchedUsedNumberLines = unmatchedNumberLines.Where(r => r.IsUsed).ToList();

            entry.UnmatchedNumberLineCreatedCount = unmatchedUsedNumberLines.Count;

            if (entry.UnmatchedNumberLineCreatedCount == 0)
            {
                entry.UnmatchedNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = unmatchedUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.UnmatchedNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            #endregion // Number Lines

            #region Stamps

            var unmatchedStampImages = unmatchedRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            var unmatchedStampIDsCreated = new List<string>();
            var unmatchedStampIDsDeleted = new List<string>();
            foreach (var usedRepresentation in unmatchedStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (PSID)"));
                var parentStampInfoParts = parentStampAdditionalInfo.Split(" - ");
                if (parentStampInfoParts.Length == 2)
                {
                    var parentStampIDsComposite = parentStampInfoParts[1];
                    var parentStampIDs = parentStampIDsComposite.Split(" ; ").ToList();
                    unmatchedStampIDsCreated.AddRange(parentStampIDs);
                    unmatchedStampIDsDeleted.AddRange(parentStampIDs.Where(id => !stampIDsOnPage.Contains(id)));
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.UnmatchedStampImagesCreatedCount += stampImageCount;
                }

                var companionStampedObjectAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (COID)"));
                var companionStampedObjectInfoParts = companionStampedObjectAdditionalInfo.Split(" - ");
                if (companionStampedObjectInfoParts.Length == 2)
                {
                    var companionStampedObjectIDsComposite = parentStampInfoParts[1];
                    var companionStampedObjectIDs = companionStampedObjectIDsComposite.Split(" ; ").ToList();
                    entry.UnmatchedStampImagesCreatedCount += companionStampedObjectIDs.Count;
                }
            }

            entry.StampCreatedCount += unmatchedStampIDsCreated.Count;
            entry.StampDeletedCount += unmatchedStampIDsDeleted.Count;

            #endregion // Stamps

            #region Representation Correctness Counts

            entry.UnmatchedArrayPartiallyCorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.PartiallyCorrect);
            entry.UnmatchedArrayIncorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.Incorrect);

            entry.UnmatchedNumberLinePartiallyCorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect);
            entry.UnmatchedNumberLineIncorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Incorrect);

            entry.UnmatchedStampPartiallyCorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect);
            entry.UnmatchedStampIncorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.Incorrect);

            #endregion // Representation Correctness Counts

            entry.UnmatchedRepresentationsAndCorrectness =
                unmatchedRepresentations.Select(
                                                r =>
                                                        $"{r.CodedObject} [{r.CodedID}] {(r.CodedObject == Codings.OBJECT_STAMP ? r.RepresentationInformation : string.Empty)}{(r.CodedObject == Codings.OBJECT_NUMBER_LINE ? r.RepresentationInformation : string.Empty)} {Codings.CorrectnessToCodedCorrectness(r.Correctness)}")
                                        .ToList();

            entry.IsUnmatchedMR = unmatchedRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Unmatched

            #region Whole Page Analysis

            var totalNumberLineUsedCount = entry.LeftNumberLineCreatedCount + entry.RightNumberLineCreatedCount + entry.AlternativeNumberLineCreatedCount + entry.UnmatchedNumberLineCreatedCount;
            if (totalNumberLineUsedCount == 0)
            {
                entry.NLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = entry.LeftNLJE == AnalysisEntry.YES || entry.RightNLJE == AnalysisEntry.YES || entry.AlternativeNLJE == AnalysisEntry.YES || entry.UnmatchedNLJE == AnalysisEntry.YES;
                entry.NLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.IsMR2STEP = AnalysisEntry.NA;
            if (representationsUsedTag != null &&
                representationsUsedTag.RepresentationsUsedType == RepresentationsUsedTypes.RepresentationsUsed &&
                entry.ProblemType != AnalysisEntry.PROBLEM_TYPE_1_PART)
            {
                var isMR2STEP = representationsUsedTag.AnalysisCodes.Contains(Codings.REPRESENTATIONS_MR2STEP);
                entry.IsMR2STEP = isMR2STEP ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            var intermediaryAnswerCorrectnessTag = page.Tags.OfType<IntermediaryAnswerCorrectnessTag>().FirstOrDefault();
            if (entry.ProblemType == AnalysisEntry.PROBLEM_TYPE_1_PART)
            {
                entry.IntermediaryAnswerCorrectness = AnalysisEntry.NA;
            }
            else if (intermediaryAnswerCorrectnessTag == null)
            {
                entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
            }
            else
            {
                switch (intermediaryAnswerCorrectnessTag.IntermediaryAnswerCorrectness)
                {
                    case Correctness.Correct:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_CORRECT;
                        break;
                    case Correctness.Incorrect:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_INCORRECT;
                        break;
                    case Correctness.PartiallyCorrect:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_PARTIAL;
                        break;
                    case Correctness.Illegible:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_ILLEGIBLE;
                        break;
                    case Correctness.Unanswered:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNANSWERED;
                        break;
                    default:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
                        break;
                }
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
                    case Correctness.Correct:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_CORRECT;
                        break;
                    case Correctness.Incorrect:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_INCORRECT;
                        break;
                    case Correctness.PartiallyCorrect:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_PARTIAL;
                        break;
                    case Correctness.Illegible:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_ILLEGIBLE;
                        break;
                    case Correctness.Unanswered:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNANSWERED;
                        break;
                    default:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
                        break;
                }
            }

            var answerRepresentationSequenceTag = page.Tags.OfType<AnswerRepresentationSequenceTag>().FirstOrDefault();
            if (answerRepresentationSequenceTag == null)
            {
                entry.ABR_RAA.Add(AnalysisEntry.NA);
                entry.AnswersChangedAfterRepresentation.Add(AnalysisEntry.NO);
            }
            else
            {
                if (answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_FINAL_ANS_COR_BEFORE_REP) ||
                    answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_FINAL_ANS_INC_BEFORE_REP))
                {
                    entry.ABR_RAA.Add(AnalysisEntry.FABR);
                }

                if (answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_INTERMEDIARY_ANS_COR_BEFORE_REP) ||
                    answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_INTERMEDIARY_ANS_INC_BEFORE_REP))
                {
                    entry.ABR_RAA.Add(AnalysisEntry.IABR);
                }

                if (answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_REP_AFTER_FINAL_ANSWER))
                {
                    entry.ABR_RAA.Add(AnalysisEntry.RAFA);
                }

                if (answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_REP_AFTER_INTERMEDIARY_ANSWER))
                {
                    entry.ABR_RAA.Add(AnalysisEntry.RAIA);
                }

                if (!entry.ABR_RAA.Any())
                {
                    entry.ABR_RAA.Add(AnalysisEntry.NA);
                }

                foreach (var analysisCode in answerRepresentationSequenceTag.AnalysisCodes)
                {
                    if (analysisCode == Codings.ANALYSIS_FINAL_ANS_COR_BEFORE_REP ||
                        analysisCode == Codings.ANALYSIS_FINAL_ANS_INC_BEFORE_REP ||
                        analysisCode == Codings.ANALYSIS_INTERMEDIARY_ANS_COR_BEFORE_REP ||
                        analysisCode == Codings.ANALYSIS_INTERMEDIARY_ANS_INC_BEFORE_REP ||
                        analysisCode == Codings.ANALYSIS_REP_AFTER_FINAL_ANSWER ||
                        analysisCode == Codings.ANALYSIS_REP_AFTER_INTERMEDIARY_ANSWER)
                    {
                        continue;
                    }

                    entry.AnswersChangedAfterRepresentation.Add(analysisCode);
                }

                if (!entry.AnswersChangedAfterRepresentation.Any())
                {
                    entry.AnswersChangedAfterRepresentation.Add(AnalysisEntry.NO);
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

            return entry;
        }
    }
}
