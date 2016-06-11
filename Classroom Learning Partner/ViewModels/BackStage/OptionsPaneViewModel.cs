using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.IoC;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using CLP.Entities;
using CLP.InkInterpretation;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OptionsPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OptionsPaneViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            BatchTagAnalysisCommand = new Command(OnBatchTagAnalysisCommandExecute);
            BatchRepresentationsUsedTagCommand = new Command(OnBatchRepresentationsUsedTagCommandExecute);
            BatchSkipCountStatsCommand = new Command(OnBatchSkipCountStatsCommandExecute);
            GenerateRandomMainColorCommand = new Command(OnGenerateRandomMainColorCommandExecute);
            ApplyRenameToCacheCommand = new Command(OnApplyRenameToCacheCommandExecute);
            ToggleBindingStyleCommand = new Command(OnToggleBindingStyleCommandExecute);
            ReplayHistoryCommand = new Command(OnReplayHistoryCommandExecute);
            GenerateSubmissionsCommand = new Command(OnGenerateSubmissionsCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Options"; }
        }

        #endregion //Bindings

        #region Commands

        public Command BatchTagAnalysisCommand { get; private set; }

        private void OnBatchTagAnalysisCommandExecute()
        {
            var dataService = DependencyResolver.Resolve<IDataService>();
            if (dataService == null)
            {
                return;
            }

            dataService.Analyze();
        }

        /// <summary>SUMMARY</summary>
        public Command BatchRepresentationsUsedTagCommand { get; private set; }

        private void OnBatchRepresentationsUsedTagCommandExecute()
        {
            var dataService = DependencyResolver.Resolve<IDataService>();
            if (dataService == null)
            {
                return;
            }

            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "Batch Results");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, "Batch Representations Used Tags.txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");
            File.AppendAllText(filePath, "*****Representations Used Tags*****" + "\n\n");

            var totalARRdeleted = 0;
            var totalARRfinal = 0;
            var totalSKIP = 0;
            var totalNLdeleted = 0;
            var totalNLfinal = 0;
            var totalStampedImagesDeleted = 0;
            var totalStampedImagesFinal = 0;
            var pagesWithARR = 0;
            var pagesWithARRDeleted = 0;
            var pagesWithARRFinal = 0;
            var pagesWithSKIP = 0;
            var pagesWithNL = 0;
            var pagesWithNLDeleted = 0;
            var pagesWithNLFinal = 0;
            var pagesWithStampedImages = 0;
            var pagesWithStampedImagesDeleted = 0;
            var pagesWithStampedImagesFinal = 0;
            var totalPages = 0;
            var totalPagesWithReps = 0;
            var totalPagesWithRepsDeleted = 0;
            var totalPagesWithRepsFinal = 0;

            foreach (var notebookInfo in dataService.LoadedNotebooksInfo.OrderBy(ni => ni.Notebook.Owner.FullName))
            {
                var notebook = notebookInfo.Notebook;
                if (notebook.OwnerID == Person.Author.ID ||
                    !notebook.Owner.IsStudent)
                {
                    continue;
                }

                foreach (var page in notebook.Pages)
                {
                    var lastSubmission = page.Submissions.LastOrDefault();
                    if (lastSubmission == null)
                    {
                        continue;
                    }

                    totalPages++;

                    Console.WriteLine("Generating Representations Used Tag for page {0}, for {1}", page.PageNumber, page.Owner.FullName);
                    HistoryAnalysis.GenerateHistoryActions(lastSubmission);
                    Console.WriteLine("Finished generating Representations Used Tag.\n");

                    var tag = lastSubmission.Tags.FirstOrDefault(t => t is RepresentationsUsedTag) as RepresentationsUsedTag;
                    if (tag == null)
                    {
                        continue;
                    }

                    if (tag.AllRepresentations.Any())
                    {
                        totalPagesWithReps++;
                    }
                    if (tag.DeletedCodedRepresentations.Any())
                    {
                        totalPagesWithRepsDeleted++;
                    }
                    if (tag.FinalCodedRepresentations.Any())
                    {
                        totalPagesWithRepsFinal++;
                    }

                    totalARRdeleted += tag.DeletedCodedRepresentations.Count(c => c.Contains("ARR"));
                    totalARRfinal += tag.FinalCodedRepresentations.Count(c => c.Contains("ARR"));
                    totalSKIP += tag.FinalCodedRepresentations.Count(c => c.Contains("skip"));
                    totalNLdeleted += tag.DeletedCodedRepresentations.Count(c => c.Contains("NL"));
                    totalNLfinal += tag.FinalCodedRepresentations.Count(c => c.Contains("NL"));
                    totalStampedImagesDeleted += tag.DeletedCodedRepresentations.Count(c => c.Contains("STAMP"));
                    totalStampedImagesFinal += tag.FinalCodedRepresentations.Count(c => c.Contains("STAMP"));

                    if (tag.AllRepresentations.Any(c => c.Contains("ARR")))
                    {
                        pagesWithARR++;
                    }
                    if (tag.DeletedCodedRepresentations.Any(c => c.Contains("ARR")))
                    {
                        pagesWithARRDeleted++;
                    }
                    if (tag.FinalCodedRepresentations.Any(c => c.Contains("ARR")))
                    {
                        pagesWithARRFinal++;
                    }
                    if (tag.FinalCodedRepresentations.Any(c => c.Contains("skip")))
                    {
                        pagesWithSKIP++;
                    }
                    if (tag.AllRepresentations.Any(c => c.Contains("NL")))
                    {
                        pagesWithNL++;
                    }
                    if (tag.DeletedCodedRepresentations.Any(c => c.Contains("NL")))
                    {
                        pagesWithNLDeleted++;
                    }
                    if (tag.FinalCodedRepresentations.Any(c => c.Contains("NL")))
                    {
                        pagesWithNLFinal++;
                    }
                    if (tag.AllRepresentations.Any(c => c.Contains("STAMP")))
                    {
                        pagesWithStampedImages++;
                    }
                    if (tag.DeletedCodedRepresentations.Any(c => c.Contains("STAMP")))
                    {
                        pagesWithStampedImagesDeleted++;
                    }
                    if (tag.FinalCodedRepresentations.Any(c => c.Contains("STAMP")))
                    {
                        pagesWithStampedImagesFinal++;
                    }

                    var tagLine = string.Format("{0}, p {1}:\n{2}", page.Owner.FullName, page.PageNumber, tag.FormattedValue);

                    File.AppendAllText(filePath, tagLine + "\n\n");
                }

                File.AppendAllText(filePath, "*****\n");
            }

            // return total instances, deleted instances, and final instances counts instead of just total
            // same 3 states for final (only report FINAL for stamps)
            File.AppendAllText(filePath, "\n*****Page Stats*****\n\n");

            File.AppendAllText(filePath, string.Format("Total Pages: {0}\n", totalPages));
            File.AppendAllText(filePath, string.Format("Total Pages with Representations Used: {0}\n", totalPagesWithReps));
            File.AppendAllText(filePath, string.Format("Total Pages with (deleted) Representations Used: {0}\n", totalPagesWithRepsDeleted));
            File.AppendAllText(filePath, string.Format("Total Pages with (final) Representations Used: {0}\n", totalPagesWithRepsFinal));
            File.AppendAllText(filePath, string.Format("Total Pages with Ink Only: {0}\n", totalPages - totalPagesWithReps));
            File.AppendAllText(filePath, string.Format("Total ARR instances: {0}\n", totalARRdeleted + totalARRfinal));
            File.AppendAllText(filePath, string.Format("Total ARR instances (deleted): {0}\n", totalARRdeleted));
            File.AppendAllText(filePath, string.Format("Total ARR instances (final): {0}\n", totalARRfinal));
            File.AppendAllText(filePath, string.Format("Total ARR skip instances (in Final Representations): {0}\n", totalSKIP));
            File.AppendAllText(filePath, string.Format("Total NL instances: {0}\n", totalNLdeleted + totalNLfinal));
            File.AppendAllText(filePath, string.Format("Total NL instances (deleted): {0}\n", totalNLdeleted));
            File.AppendAllText(filePath, string.Format("Total NL instances (final): {0}\n", totalNLfinal));
            File.AppendAllText(filePath, string.Format("Total STAMP instances (probably inaccurate): {0}\n", totalStampedImagesDeleted + totalStampedImagesFinal));
            File.AppendAllText(filePath, string.Format("Total STAMP instances (deleted, probably inaccurate): {0}\n", totalStampedImagesDeleted));
            File.AppendAllText(filePath, string.Format("Total STAMP instances (final): {0}\n", totalStampedImagesFinal));
            File.AppendAllText(filePath, string.Format("Number of Pages with ARR instances: {0}\n", pagesWithARR));
            File.AppendAllText(filePath, string.Format("Number of Pages with ARR instances (deleted): {0}\n", pagesWithARRDeleted));
            File.AppendAllText(filePath, string.Format("Number of Pages with ARR instances (final): {0}\n", pagesWithARRFinal));
            File.AppendAllText(filePath, string.Format("Number of Pages with ARR skip instances (in Final Representations): {0}\n", pagesWithSKIP));
            File.AppendAllText(filePath, string.Format("Number of Pages with NL instances: {0}\n", pagesWithNL));
            File.AppendAllText(filePath, string.Format("Number of Pages with NL instances (deleted): {0}\n", pagesWithNLDeleted));
            File.AppendAllText(filePath, string.Format("Number of Pages with NL instances (final): {0}\n", pagesWithNLFinal));
            File.AppendAllText(filePath, string.Format("Number of Pages with STAMP instances (probably inaccurate: {0}\n", pagesWithStampedImages));
            File.AppendAllText(filePath, string.Format("Number of Pages with STAMP instances (deleted, probably inaccurate): {0}\n", pagesWithStampedImagesDeleted));
            File.AppendAllText(filePath, string.Format("Number of Pages with STAMP instances (final): {0}\n", pagesWithStampedImagesFinal));
        }

        public Command BatchSkipCountStatsCommand { get; private set; }

        private void OnBatchSkipCountStatsCommandExecute()
        {
            var dataService = DependencyResolver.Resolve<IDataService>();
            if (dataService == null)
            {
                return;
            }

            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "Batch Results");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, "Skip Counting Stats.txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");

            var totalPages = new List<string>();
            var totalArraysTested = 0;
            var totalArraysRecognizedAsSkipCounting = new List<string>();

            var totalRowsTested = 0;
            var totalSkipCountRows = 0;
            var totalSkipCountRowsWithInterpretations = 0;
            var totalSkipCountRowsMatchedCorrectly = 0;

            var correctedMatchedUncorrectedCount = 0;
            var correctedNotMatchedUncorrectedCount = 0;
            var uncorrectedMatchedExpectedCount = 0;
            var correctedMatchedExpectedCount = 0;
            var correctedNotMatchedExpectedCount = 0;
            var uncorrectedNotMatchedExpectedCount = 0;
            var correctedImprovedExpectedCount = 0;

            var correctedImprovedSkipCount = 0;
            var correctedMatchedUncorrectedCountAfter = 0;
            var correctedNotMatchedUncorrectedCountAfter = 0;
            var uncorrectedMatchedExpectedCountAfter = 0;
            var correctedMatchedExpectedCountAfter = 0;
            var correctedNotMatchedExpectedCountAfter = 0;
            var uncorrectedNotMatchedExpectedCountAfter = 0;
            var correctedImprovedExpectedCountAfter = 0;
            var correctedImprovedExpectedCountAfterRows = new List<string>();

            var correctedNotMatchedExpectedCountAfterExamples = new List<string>();

            var keyList = new List<string>();
            var interpretationOfStrokesInInitialBoundingBox = new Dictionary<string, string>();
            var interpretationOfStrokesNotGroupedByRows = new Dictionary<string,string>();
            var interpretationOfStrokesGroupedByRows = new Dictionary<string, string>();

            foreach (var notebookInfo in dataService.LoadedNotebooksInfo.OrderBy(ni => ni.Notebook.Owner.FullName))
            {
                var notebook = notebookInfo.Notebook;
                if (notebook.OwnerID == Person.Author.ID ||
                    !notebook.Owner.IsStudent)
                {
                    continue;
                }

                foreach (var page in notebook.Pages)
                {
                    var lastSubmission = page.Submissions.LastOrDefault();
                    if (lastSubmission == null)
                    {
                        continue;
                    }

                    totalPages.Add(string.Format("{0}, page {1}", lastSubmission.Owner.FullName, lastSubmission.PageNumber));

                    Console.WriteLine("Generating Skip Counting Stats for page {0}, for {1}", lastSubmission.PageNumber, lastSubmission.Owner.FullName);
                    var arraysOnPage = lastSubmission.PageObjects.OfType<CLPArray>().ToList();

                    //Iterates over arrays on page
                    foreach (var array in arraysOnPage)
                    {
                        totalArraysTested++;
                        totalRowsTested += array.Rows;

                        var historyIndex = 0;
                        var lastHistoryItem = lastSubmission.History.CompleteOrderedHistoryItems.LastOrDefault();
                        if (lastHistoryItem != null)
                        {
                            historyIndex = lastHistoryItem.HistoryIndex;
                        }

                        var strokes = lastSubmission.InkStrokes.ToList();

                        var vkey = string.Format("{0}, page {1}. [{2}x{3}]", lastSubmission.Owner.FullName, lastSubmission.PageNumber, array.Rows, array.Columns);
                        var vkeyIncrement = 0;
                        while (keyList.Contains(vkey))
                        {
                            vkeyIncrement++;
                            vkey = string.Format("{0}, page {1}. [{2}x{3}, {4}]", lastSubmission.Owner.FullName, lastSubmission.PageNumber, array.Rows, array.Columns, vkeyIncrement);
                        }

                        keyList.Add(vkey);

                        #region v1 Calculations

                        const double RIGHT_OF_VISUAL_RIGHT_THRESHOLD = 80.0;
                        const double LEFT_OF_VISUAL_RIGHT_THRESHOLD = 41.5;

                        var arrayPosition = array.GetPositionAtHistoryIndex(historyIndex);
                        var arrayDimensions = array.GetDimensionsAtHistoryIndex(historyIndex);
                        var arrayColumnsAndRows = array.GetColumnsAndRowsAtHistoryIndex(historyIndex);
                        var arrayVisualRight = arrayPosition.X + arrayDimensions.X - array.LabelLength;
                        var arrayVisualTop = arrayPosition.Y + array.LabelLength;
                        var halfGridSquareSize = array.GridSquareSize * 0.5;
                        var acceptedBoundary = new Rect(arrayVisualRight - LEFT_OF_VISUAL_RIGHT_THRESHOLD,
                                                        arrayVisualTop - halfGridSquareSize,
                                                        LEFT_OF_VISUAL_RIGHT_THRESHOLD + RIGHT_OF_VISUAL_RIGHT_THRESHOLD,
                                                        array.GridSquareSize * (arrayColumnsAndRows.Y + 1));

                        var strokesInsideBoundary = new List<Stroke>();

                        foreach (var stroke in strokes)
                        {
                            var strokeBounds = stroke.GetBounds();

                            // Rule 3: Rejected for being outside the accepted skip counting bounds
                            var intersect = Rect.Intersect(strokeBounds, acceptedBoundary);
                            if (intersect.IsEmpty)
                            {
                                continue;
                            }

                            var intersectPercentage = intersect.Area() / strokeBounds.Area();
                            if (intersectPercentage <= 0.50)
                            {
                                continue;
                            }

                            if (intersectPercentage <= 0.90)
                            {
                                var weightedCenterX = stroke.WeightedCenter().X;
                                if (weightedCenterX < arrayVisualRight - LEFT_OF_VISUAL_RIGHT_THRESHOLD ||
                                    weightedCenterX > arrayVisualRight + RIGHT_OF_VISUAL_RIGHT_THRESHOLD)
                                {
                                    continue;
                                }
                            }

                            strokesInsideBoundary.Add(stroke);
                        }

                        if (!strokesInsideBoundary.Any())
                        {
                            interpretationOfStrokesInInitialBoundingBox.Add(vkey, "NO STROKES IN BOUNDARY");
                        }
                        else
                        {
                            var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(strokesInsideBoundary));
                            var guess = InkInterpreter.InterpretationClosestToANumber(interpretations); //or use [0]

                            interpretationOfStrokesInInitialBoundingBox.Add(vkey, guess);
                        }

                        #endregion // v1 Calculations

                        var strokeGroupPerRow = ArrayCodedActions.GroupPossibleSkipCountStrokes(lastSubmission, array, strokes, historyIndex);

                        #region v2 Calculations

                        var skipStrokes = strokeGroupPerRow.Where(kv => kv.Key != 0 && kv.Key != -1).SelectMany(kv => kv.Value).Distinct().ToList();
                        if (!skipStrokes.Any())
                        {
                            interpretationOfStrokesNotGroupedByRows.Add(vkey, "NO STROKES RECOGNIZED AS SKIP STROKES");
                        }
                        else
                        {
                            var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(skipStrokes));
                            var guess = InkInterpreter.InterpretationClosestToANumber(interpretations); //or use [0]

                            interpretationOfStrokesNotGroupedByRows.Add(vkey, guess);
                        }

                        #endregion // v2 Calculations

                        var interpretedRowValuesUncorrected = ArrayCodedActions.InterpretSkipCountGroups(lastSubmission, array, strokeGroupPerRow, historyIndex, true);
                        var interpretedRowValues = ArrayCodedActions.InterpretSkipCountGroups(lastSubmission, array, strokeGroupPerRow, historyIndex);
                        if (interpretedRowValues.Any() &&
                            interpretedRowValuesUncorrected.Any())
                        {
                            for (int i = 0; i < array.Rows; i++)
                            {
                                var expectedValue = (i + 1) * array.Columns;
                                var interpretedValueUncorrected = interpretedRowValuesUncorrected[i];
                                var interpretedValue = interpretedRowValues[i];

                                if (string.IsNullOrEmpty(interpretedValueUncorrected) ||
                                    string.IsNullOrEmpty(interpretedValue))
                                {
                                    continue;
                                }

                                if (interpretedValue == interpretedValueUncorrected)
                                {
                                    correctedMatchedUncorrectedCount++;
                                }
                                else
                                {
                                    correctedNotMatchedUncorrectedCount++;
                                }

                                if (expectedValue.ToString() == interpretedValueUncorrected)
                                {
                                    uncorrectedMatchedExpectedCount++;
                                }
                                else
                                {
                                    uncorrectedNotMatchedExpectedCount++;
                                    if (expectedValue.ToString() == interpretedValue)
                                    {
                                        correctedImprovedExpectedCount++;
                                    }
                                }

                                if (expectedValue.ToString() == interpretedValue)
                                {
                                    correctedMatchedExpectedCount++;
                                }
                                else
                                {
                                    correctedNotMatchedExpectedCount++;
                                }
                            }
                        }

                        //var isSkipCountingWhenUncorrected = ArrayCodedActions.IsSkipCounting(interpretedRowValuesUncorrected);
                        var isSkipCounting = ArrayCodedActions.IsSkipCounting(interpretedRowValues);
                        if (isSkipCounting)
                        {
                            interpretationOfStrokesGroupedByRows.Add(vkey, string.Join(", ", interpretedRowValues));

                            totalSkipCountRows += array.Rows;
                            totalArraysRecognizedAsSkipCounting.Add(string.Format("{0}, page {1}. [{2}x{3}]", lastSubmission.Owner.FullName, lastSubmission.PageNumber, array.Rows, array.Columns));
                            for (int i = 0; i < array.Rows; i++)
                            {
                                var expectedValue = (i + 1) * array.Columns;
                                var interpretedValue = interpretedRowValues[i];
                                var interpretedValueUncorrected = interpretedRowValuesUncorrected[i];

                                if (expectedValue.ToString() == interpretedValue)
                                {
                                    totalSkipCountRowsMatchedCorrectly++;
                                }

                                if (!string.IsNullOrEmpty(interpretedValue))
                                {
                                    totalSkipCountRowsWithInterpretations++;
                                }

                                if (string.IsNullOrEmpty(interpretedValueUncorrected) ||
                                    string.IsNullOrEmpty(interpretedValue))
                                {
                                    continue;
                                }

                                if (interpretedValue == interpretedValueUncorrected)
                                {
                                    correctedMatchedUncorrectedCountAfter++;
                                }
                                else
                                {
                                    correctedNotMatchedUncorrectedCountAfter++;
                                }

                                if (expectedValue.ToString() == interpretedValueUncorrected)
                                {
                                    uncorrectedMatchedExpectedCountAfter++;
                                }
                                else
                                {
                                    uncorrectedNotMatchedExpectedCountAfter++;
                                    if (expectedValue.ToString() == interpretedValue)
                                    {
                                        correctedImprovedExpectedCountAfter++;
                                        correctedImprovedExpectedCountAfterRows.Add(
                                                                                    string.Format(
                                                                                                  "{0}, page {1}. [{2}x{3}]. Expected: {4}, Interpreted (Corrected): {5}, Interpreted (Uncorrected): {6}",
                                                                                                  lastSubmission.Owner.FullName,
                                                                                                  lastSubmission.PageNumber,
                                                                                                  array.Rows,
                                                                                                  array.Columns,
                                                                                                  expectedValue,
                                                                                                  interpretedValue,
                                                                                                  interpretedValueUncorrected));
                                    }
                                }

                                if (expectedValue.ToString() == interpretedValue)
                                {
                                    correctedMatchedExpectedCountAfter++;
                                }
                                else
                                {
                                    correctedNotMatchedExpectedCountAfter++;
                                    var expectedWrongDimensionValue = (i + 1) * array.Rows;
                                    if (expectedWrongDimensionValue.ToString() == interpretedValue)
                                    {
                                        correctedNotMatchedExpectedCountAfterExamples.Add(string.Format("*(Matches wrong dimension) {0}, page {1}. [{2}x{3}]. Expected: {4}, Interpreted: {5}",
                                                                                                        lastSubmission.Owner.FullName,
                                                                                                        lastSubmission.PageNumber,
                                                                                                        array.Rows,
                                                                                                        array.Columns,
                                                                                                        expectedValue,
                                                                                                        interpretedValue));
                                    }
                                    else
                                    {
                                        correctedNotMatchedExpectedCountAfterExamples.Add(string.Format("{0}, page {1}. [{2}x{3}]. Expected: {4}, Interpreted: {5}",
                                                                                                        lastSubmission.Owner.FullName,
                                                                                                        lastSubmission.PageNumber,
                                                                                                        array.Rows,
                                                                                                        array.Columns,
                                                                                                        expectedValue,
                                                                                                        interpretedValue));
                                    }
                                }
                            }
                        }
                        else
                        {
                            interpretationOfStrokesGroupedByRows.Add(vkey, "NOT RECOGNIZED AS SKIP COUNTING");
                        }

                        //if (isSkipCounting && 
                        //    !isSkipCountingWhenUncorrected)
                        //{
                        //    correctedImprovedSkipCount++;
                        //}
                    }
                    Console.WriteLine("Finished Skip Counting Stats for page {0}, for {1}", lastSubmission.PageNumber, lastSubmission.Owner.FullName);
                }
            }

            File.AppendAllText(filePath, "\n*****Skip Counting Stats*****\n\n");

            File.AppendAllText(filePath, string.Format("Total Pages: {0}\n", totalPages.Count));
            File.AppendAllText(filePath, string.Format("\t{0}\n\n", string.Join("\n\t", totalPages)));
            File.AppendAllText(filePath, string.Format("Total Arrays Tested: {0}\n", totalArraysTested));
            File.AppendAllText(filePath, string.Format("Total Arrays Recognized as Skip Counting: {0}\n", totalArraysRecognizedAsSkipCounting.Count));
            File.AppendAllText(filePath, string.Format("\t{0}\n\n", string.Join("\n\t", totalArraysRecognizedAsSkipCounting)));
            File.AppendAllText(filePath, string.Format("Total Skip Count Rows Tested: {0}\n", totalSkipCountRows));
            File.AppendAllText(filePath, string.Format("Total Skip Count Rows Tested (that have an interpreted value): {0}\n", totalSkipCountRowsWithInterpretations));
            File.AppendAllText(filePath, string.Format("Total Skip Count Rows Matched Correctly: {0}\n", totalSkipCountRowsMatchedCorrectly));

            File.AppendAllText(filePath, string.Format("\n***Stroke Grouping***\n\n"));
            File.AppendAllText(filePath, string.Format("Skip Counting Strokes Count: {0}\n", ArrayCodedActions.SkipStrokesCount));
            File.AppendAllText(filePath, string.Format("Rejected Strokes Count: {0}\n", ArrayCodedActions.RejectedStrokesCount));
            File.AppendAllText(filePath, string.Format("Overlapping Strokes Count: {0}\n", ArrayCodedActions.OverlappingStrokesCount));
            File.AppendAllText(filePath, string.Format("Initially Ungrouped Strokes Count: {0}\n", ArrayCodedActions.UngroupedStrokesCount));

            File.AppendAllText(filePath, string.Format("\nNumber of strokes rejected (or alternatively grouped) by a rule:\n"));
            File.AppendAllText(filePath, string.Format("Rule 1: Stroke is invisibly small: {0}\n", ArrayCodedActions.Rule1Count));
            File.AppendAllText(filePath, string.Format("Rule 2: Stroke is too tall (taller than 2 row heights): {0}\n", ArrayCodedActions.Rule2Count));
            File.AppendAllText(filePath, string.Format("Rule 3b: Stroke intersects less than 50% of initial boundary: {0}\n", ArrayCodedActions.Rule3bCount));
            File.AppendAllText(filePath,
                               string.Format("Rule 3c: Stroke intersects less than 90% of initial boundary and weighted center is not in the initial boundary: {0}\n", ArrayCodedActions.Rule3cCount));
            File.AppendAllText(filePath, string.Format("Rule 3a: Number of strokes that have NOT been rejected after Rule 3 applied: {0}\n", ArrayCodedActions.Rule3aCount));
            File.AppendAllText(filePath,
                               string.Format("Number of times all strokes have been rejected after Rule 3 (aka, no strokes in initial boundary): {0}\n",
                                             ArrayCodedActions.AllStrokesAreOutsideOfAcceptableBoundary));
            File.AppendAllText(filePath, string.Format("Rule 4: Stroke is too tall (deviates from average height): {0}\n", ArrayCodedActions.Rule4Count));
            File.AppendAllText(filePath, string.Format("Rule 5: Stroke ungrouped for being too short (should match Initially Ungrouped Strokes Count above): {0}\n", ArrayCodedActions.Rule5Count));
            File.AppendAllText(filePath, string.Format("Rule 6: Stroke is too far away from other strokes: {0}\n", ArrayCodedActions.Rule6Count));
            File.AppendAllText(filePath, string.Format("Rule 7c: Stroke is placed in initial overlap group: {0}\n", ArrayCodedActions.Rule7cCount));
            File.AppendAllText(filePath, string.Format("Rule 7d: Stroke does not overlap any row: {0}\n", ArrayCodedActions.Rule7dCount));
            File.AppendAllText(filePath, string.Format("Rule 8a: Overlap stroke intersects with an already grouped stroke: {0}\n", ArrayCodedActions.Rule8aCount));
            File.AppendAllText(filePath, string.Format("Rule 8b: Overlap stroke intersects 75% with a row: {0}\n", ArrayCodedActions.Rule8bCount));
            File.AppendAllText(filePath, string.Format("Rule 8c: Overlap stroke remains an overlap stroke: {0}\n", ArrayCodedActions.Rule8cCount));
            File.AppendAllText(filePath, string.Format("Rule 9: Ungrouped stroke matched with a grouped stroke: {0}\n", ArrayCodedActions.Rule9Count));
            File.AppendAllText(filePath,
                               string.Format("Rule 10: Number of times strokes are rejected for being on the inside of an array when the skip counting stroke are on the outside: {0}\n",
                                             ArrayCodedActions.Rule10Count));
            File.AppendAllText(filePath, string.Format("Rule 10: Number strokes rejected because of Rule 10: {0}\n", ArrayCodedActions.Rule10RejectedStrokesCount));

            File.AppendAllText(filePath, string.Format("\n***Ink Interpretation***\n\n"));

            File.AppendAllText(filePath, "\nBefore Skip Count Identification Test, all for non-empty strings\n\n");
            File.AppendAllText(filePath, string.Format("Number of Rows uncorrected interpretation matched corrected: {0}\n", correctedMatchedUncorrectedCount));
            File.AppendAllText(filePath, string.Format("Number of Rows uncorrected interpretation did not matched corrected: {0}\n", correctedNotMatchedUncorrectedCount));
            File.AppendAllText(filePath, string.Format("Number of Rows uncorrected matched expected value: {0}\n", uncorrectedMatchedExpectedCount));
            File.AppendAllText(filePath, string.Format("Number of Rows uncorrected did not match expected value: {0}\n", uncorrectedNotMatchedExpectedCount));
            File.AppendAllText(filePath, string.Format("Number of Rows corrected matched expected when uncorrected did not (aka an improvement): {0}\n", correctedImprovedExpectedCount));
            File.AppendAllText(filePath, string.Format("Number of Rows corrected matched expected value: {0}\n", correctedMatchedExpectedCount));
            File.AppendAllText(filePath, string.Format("Number of Rows corrected did not match expected value: {0}\n", correctedNotMatchedExpectedCount));

            File.AppendAllText(filePath, "\nAfter Skip Count Identification Test\n\n");
            File.AppendAllText(filePath, string.Format("Number of Times correct recognized as skip when uncorrected not recognized as skip: {0}\n", 0));
            File.AppendAllText(filePath, "Note on above stat, current value hardcoded after testing. Have to run IsSkipCounting() twice and that doubles the rejection rules counts.\n");

            File.AppendAllText(filePath, string.Format("Number of Rows uncorrected interpretation matched corrected: {0}\n", correctedMatchedUncorrectedCountAfter));
            File.AppendAllText(filePath, string.Format("Number of Rows uncorrected interpretation did not matched corrected: {0}\n", correctedNotMatchedUncorrectedCountAfter));
            File.AppendAllText(filePath, string.Format("Number of Rows uncorrected matched expected value: {0}\n", uncorrectedMatchedExpectedCountAfter));
            File.AppendAllText(filePath, string.Format("Number of Rows uncorrected did not match expected value: {0}\n", uncorrectedNotMatchedExpectedCountAfter));
            File.AppendAllText(filePath, string.Format("Number of Rows corrected matched expected when uncorrected did not (aka an improvement): {0}\n", correctedImprovedExpectedCountAfter));
            File.AppendAllText(filePath, string.Format("\t{0}\n\n", string.Join("\n\t", correctedImprovedExpectedCountAfterRows)));
            File.AppendAllText(filePath, string.Format("Number of Rows corrected matched expected value: {0}\n", correctedMatchedExpectedCountAfter));
            File.AppendAllText(filePath, string.Format("Number of Rows corrected did not match expected value: {0}\n", correctedNotMatchedExpectedCountAfter));
            File.AppendAllText(filePath, string.Format("\t{0}\n\n", string.Join("\n\t", correctedNotMatchedExpectedCountAfterExamples)));

            File.AppendAllText(filePath, string.Format("\n***Skip Counting Rejection Reasons***\n\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays Rejected as Skip Counting: {0}\n", totalArraysTested - totalArraysRecognizedAsSkipCounting.Count));

            File.AppendAllText(filePath, string.Format("Rejected Reasons:\n"));
            File.AppendAllText(filePath, string.Format("Rule 0: Passed null value (ERROR).\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule0));

            File.AppendAllText(filePath, string.Format("Rule 1: Only 1 row in the array.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule1));

            File.AppendAllText(filePath, string.Format("Rule 2: Fewer than 2 rows have an interpreted value.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule2));

            File.AppendAllText(filePath, string.Format("Rule 3: No rows have an interpreted value that is a number.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule3));

            File.AppendAllText(filePath, string.Format("Rule 4: Of the rows with interpreted values, the percentage of those interpreted values with numeric results is less than 34%.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule4));

            File.AppendAllText(filePath, string.Format("Rule 5: The first row does not have an interpreted value and only 50% or less of the rows have an interpreted value.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule5));

            File.AppendAllText(filePath, string.Format("Rule 6: The first 2 rows do not have interpreted values.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule6));

            File.AppendAllText(filePath, string.Format("Rule 7: There is more than 1 gap of 1 row between interpreted values.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule7));

            File.AppendAllText(filePath, string.Format("Rule 8: There is a gap of more than 1 row between interpreted values.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule8));

            File.AppendAllText(filePath, string.Format("Rule 9: More than 2 rows share the same interpreted value.\n"));
            File.AppendAllText(filePath, string.Format("Total Arrays: {0}\n\n", ArrayCodedActions.ArraysRule9));

            File.AppendAllText(filePath, string.Format("\n\nInterpretation improvements for every array."));
            File.AppendAllText(filePath, string.Format("\nv1: Interpretation of all strokes inside boundary box."));
            File.AppendAllText(filePath, string.Format("\nv2: Interpretation of all strokes recognized as likely skip counting strokes, but not grouped into rows. May include strokes later rejected by array structure criteria."));
            File.AppendAllText(filePath, string.Format("\nv3: Row by row interpretation of strokes recognized as skip counting strokes.\n\n"));

            File.AppendAllText(filePath, string.Format("\nNumber of Arrays with strokes inside initial boundary (v1): {0}", interpretationOfStrokesInInitialBoundingBox.Values.Count(s => s != "NO STROKES IN BOUNDARY")));
            File.AppendAllText(filePath, string.Format("\nNumber of Arrays with strokes considered possible skip counting, ungrouped by rows (v2): {0}", interpretationOfStrokesNotGroupedByRows.Values.Count(s => s != "NO STROKES RECOGNIZED AS SKIP STROKES")));

            foreach (var key in keyList)
            {
                File.AppendAllText(filePath, string.Format("\n\nArray: {0}", key));
                File.AppendAllText(filePath, string.Format("\n\tv1 interpretation: {0}", interpretationOfStrokesInInitialBoundingBox[key]));
                File.AppendAllText(filePath, string.Format("\n\tv2 interpretation: {0}", interpretationOfStrokesNotGroupedByRows[key]));
                File.AppendAllText(filePath, string.Format("\n\tv3 interpretation: {0}", interpretationOfStrokesGroupedByRows[key]));
            }
        }

        /// <summary>Sets the DynamicMainColor of the program to a random color.</summary>
        public Command GenerateRandomMainColorCommand { get; private set; }

        private void OnGenerateRandomMainColorCommandExecute()
        {
            var randomGen = new Random();
            var names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            var randomColorName = names[randomGen.Next(names.Length)];
            var color = Color.FromKnownColor(randomColorName);
            MainWindowViewModel.ChangeApplicationMainColor(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        /// <summary>Completely renames a Person throughout the entire cache.</summary>
        public Command ApplyRenameToCacheCommand { get; private set; }

        private void OnApplyRenameToCacheCommandExecute()
        {
            var notebookService = DependencyResolver.Resolve<INotebookService>();
            if (notebookService == null)
            {
                return;
            }

            const string NEW_NAME = "Elvis Garzona";
            App.MainWindowViewModel.CurrentUser.FullName = NEW_NAME;
            var newPerson = App.MainWindowViewModel.CurrentUser;
            var notebook = notebookService.CurrentNotebook;
            notebook.Owner = newPerson;
            foreach (var page in notebook.Pages)
            {
                page.Owner = newPerson;
                foreach (var submission in page.Submissions)
                {
                    submission.Owner = newPerson;
                }
            }
        }

        /// <summary>Toggles the style used by pageObjects for their boundary.</summary>
        public Command ToggleBindingStyleCommand { get; private set; }

        private void OnToggleBindingStyleCommandExecute()
        {
            App.MainWindowViewModel.IsUsingOldPageObjectBoundary = !App.MainWindowViewModel.IsUsingOldPageObjectBoundary;
        }

        /// <summary>Replays the interaction history of the page on the Grid Display.</summary>
        public Command ReplayHistoryCommand { get; private set; }

        private void OnReplayHistoryCommandExecute()
        {
            var animationControlRibbon = NotebookWorkspaceViewModel.GetAnimationControlRibbon();
            if (animationControlRibbon == null)
            {
                return;
            }

            animationControlRibbon.IsNonAnimationPlaybackEnabled = !animationControlRibbon.IsNonAnimationPlaybackEnabled;
        }

        /// <summary>Generates submissions for pages with no submissions.</summary>
        public Command GenerateSubmissionsCommand { get; private set; }

        private void OnGenerateSubmissionsCommandExecute()
        {
            var dataService = DependencyResolver.Resolve<IDataService>();
            if (dataService == null)
            {
                return;
            }

            PleaseWaitHelper.Show(() =>
                                  {
                                      DataService.GenerateSubmissionsFromModifiedStudentPages();
                                  },
                                  null,
                                  "Generating Submissions");
        }

        #endregion //Commands
    }
}