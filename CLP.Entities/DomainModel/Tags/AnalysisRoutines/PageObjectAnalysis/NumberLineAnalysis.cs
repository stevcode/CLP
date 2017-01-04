using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Ink;

namespace CLP.Entities.Demo
{
    public static class NumberLineAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region)
        {
            // First, clear out any old DivisionTemplateTags generated via Analysis.
            //foreach (var tag in
            //    page.Tags.ToList()
            //        .Where(
            //               tag =>
            //               tag is NumberLineCompletenessTag || tag is NumberLineRepresentationCorrectnessTag))
            //{
            //    page.RemoveTag(tag);
            //}

            var numberLineDefinitionsTags = page.Tags.OfType<MultiplicationRelationDefinitionTag>().ToList();
            var numberLines = page.PageObjects.OfType<NumberLine>().ToList();
            if (!numberLineDefinitionsTags.Any() ||
                !numberLines.Any())
            {
                return;
            }

            foreach (var numberLineDefinitionTag in numberLineDefinitionsTags)
            {
                foreach (var numberLine in numberLines)
                {
                    AnalyzeRepresentationCorrectness(page, numberLineDefinitionTag, numberLine);
                }
            }
        }

        public static List<string> GetListOfNumberLineIDsInHistory(CLPPage page)
        {
            var completeOrderedHistory = page.History.UndoItems.Reverse().Concat(page.History.RedoItems).ToList();

            var numberLineIDsInHistory = new List<string>();
            foreach (var pageObjectsAddedHistoryItem in completeOrderedHistory.OfType<PageObjectsAddedHistoryItem>())
            {
                numberLineIDsInHistory.AddRange(from pageObjectID in pageObjectsAddedHistoryItem.PageObjectIDs
                                                      let numberLine =
                                                          page.GetPageObjectByID(pageObjectID) as NumberLine ?? page.History.GetPageObjectByID(pageObjectID) as NumberLine
                                                      where numberLine != null
                                                      select pageObjectID);
            }

            return numberLineIDsInHistory;
        }

        public static void AnalyzeRepresentationCorrectness(CLPPage page, MultiplicationRelationDefinitionTag multiplicationRelationDefinition, NumberLine numberLine)
        {
            //var numberLineIDsInHistory = GetListOfNumberLineIDsInHistory(page);
            //var incomplete = false;


            //Completeness
            var arcs = new List<dynamic>();
            foreach (var stroke in numberLine.AcceptedStrokes)
            {
                var tickRight = numberLine.FindClosestTickToArcStroke(stroke, true);
                var tickLeft = numberLine.FindClosestTickToArcStroke(stroke, false);
                arcs.Add(new
                         {
                             Start = tickLeft.TickValue,
                             End = tickRight.TickValue
                         });
            }
            var sortedArcs = arcs.Distinct().OrderBy(x => x.Start).ToList();
            var gaps = 0;
            var overlaps = 0;

            for (var i = 0; i < sortedArcs.Count - 1; i++)
            {
                if (sortedArcs[i].End < sortedArcs[i + 1].Start)
                {
                    gaps++;
                }
                else if(sortedArcs[i].End > sortedArcs[i+1].Start)
                {
                    overlaps++;
                }
            }



            //if (!numberLine.JumpSizes.Any())
            //{
            //    var tag = new NumberLineCompletenessTag(page,
            //                                            Origin.StudentPageObjectGenerated,
            //                                            numberLine.ID,
            //                                            0,
            //                                            numberLine.NumberLineSize,
            //                                            numberLineIDsInHistory.IndexOf(numberLine.ID),
            //                                            false,
            //                                            true,
            //                                            0,
            //                                            0);
            //    page.AddTag(tag);
            //    incomplete = true;
            //}
            //else if(gaps != 0 || overlaps != 0)
            //{
            //    var tag = new NumberLineCompletenessTag(page,
            //                                            Origin.StudentPageObjectGenerated,
            //                                            numberLine.ID,
            //                                            0,
            //                                            numberLine.NumberLineSize,
            //                                            numberLineIDsInHistory.IndexOf(numberLine.ID),
            //                                            false,
            //                                            false,
            //                                            gaps,
            //                                            overlaps);
            //    page.AddTag(tag);
            //    incomplete = true;
            //}
            //else
            //{
            //    var tag = new NumberLineCompletenessTag(page,
            //                            Origin.StudentPageObjectGenerated,
            //                            numberLine.ID,
            //                            0,
            //                            numberLine.NumberLineSize,
            //                            numberLineIDsInHistory.IndexOf(numberLine.ID),
            //                            true,
            //                            false,
            //                            0,
            //                            0);
            //    page.AddTag(tag);
            //}

            //Correctness
            //var incorrectReasons = new List<NumberLineRepresentationIncorrectReasons>();

            //if (incomplete)
            //{
            //    incorrectReasons.Add(NumberLineRepresentationIncorrectReasons.Incomplete);
            //}

            //if ((multiplicationRelationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups)
            //    && (numberLine.JumpSizes.Count != multiplicationRelationDefinition.Factors[0].RelationPartAnswerValue || numberLine.JumpSizes.Count != multiplicationRelationDefinition.Factors[1].RelationPartAnswerValue))
            //{
            //    incorrectReasons.Add(NumberLineRepresentationIncorrectReasons.WrongNumberofJumps);
            //}
            //else if ((multiplicationRelationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups)
            //    && (numberLine.JumpSizes.Count == multiplicationRelationDefinition.Factors[1].RelationPartAnswerValue))
            //{
            //    incorrectReasons.Add(NumberLineRepresentationIncorrectReasons.ReversedGrouping);
            //}
            //else if ((multiplicationRelationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups)
            //    && (numberLine.JumpSizes.Count != multiplicationRelationDefinition.Factors[0].RelationPartAnswerValue))
            //{
            //    incorrectReasons.Add(NumberLineRepresentationIncorrectReasons.WrongNumberofJumps);
            //}

            //var isWrongJumpSize = false;
            //if ((multiplicationRelationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups))
            //{
            //    foreach (var jump in numberLine.JumpSizes)
            //    {
            //        if (numberLine.JumpSizes.All(x => x.JumpSize == multiplicationRelationDefinition.Factors[0].RelationPartAnswerValue) ||
            //            numberLine.JumpSizes.All(x => x.JumpSize == multiplicationRelationDefinition.Factors[1].RelationPartAnswerValue))
            //        {
            //            isWrongJumpSize = false;
            //        }
            //        else
            //        {
            //            isWrongJumpSize = true;
            //        }
            //    }
            //}

            //if ((multiplicationRelationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups))
            //{
            //    foreach (var jump in numberLine.JumpSizes)
            //    {
            //        if (jump.JumpSize != multiplicationRelationDefinition.Factors[1].RelationPartAnswerValue)
            //        {
            //            isWrongJumpSize = true;
            //        }
            //    }
            //}

            //if (isWrongJumpSize)
            //{
            //    incorrectReasons.Add(NumberLineRepresentationIncorrectReasons.WrongJumpSizes);
            //}

            //var lastMarkedTick = numberLine.Ticks.LastOrDefault(x => x.IsMarked);
            //if (lastMarkedTick != null &&
            //    lastMarkedTick.TickValue != multiplicationRelationDefinition.Product)
            //{
            //    incorrectReasons.Add(NumberLineRepresentationIncorrectReasons.WrongLastMarkedTick);
            //}

            //if (incorrectReasons.Any())
            //{
            //    var incorrectTag = new NumberLineRepresentationCorrectnessTag(page,
            //                                Origin.StudentPageObjectGenerated,
            //                                numberLine.ID,
            //                                0,
            //                                numberLine.NumberLineSize,
            //                                numberLineIDsInHistory.IndexOf(numberLine.ID),
            //                                Correctness.Incorrect,
            //                                incorrectReasons);
            //    page.AddTag(incorrectTag);
            //}
            //else
            //{
            //    var incorrectTag = new NumberLineRepresentationCorrectnessTag(page,
            //                Origin.StudentPageObjectGenerated,
            //                numberLine.ID,
            //                0,
            //                numberLine.NumberLineSize,
            //                numberLineIDsInHistory.IndexOf(numberLine.ID),
            //                Correctness.Correct,
            //                incorrectReasons);
            //    page.AddTag(incorrectTag);
            //}
        }
    }
}