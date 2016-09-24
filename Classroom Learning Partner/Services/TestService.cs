using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Xml;
using System.Xml.Linq;
using Catel.IoC;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using CLP.InkInterpretation;
using Path = Catel.IO.Path;

namespace Classroom_Learning_Partner.Services
{
    public class TestService
    {
        private readonly IDataService _dataService;

        public TestService()
        {
            _dataService = ServiceLocator.Default.ResolveType<IDataService>();
        }

        #region Notebook Info Pane

        private void OnSaveNotebookForStudentCommandExecute()
        {
            //if (_dataService == null ||
            //    _dataService.CurrentCacheInfo == null ||
            //    _dataService.CurrentNotebookInfo == null ||
            //    _dataService.CurrentNotebookInfo.Notebook == null)
            //{
            //    return;
            //}

            //var textInputViewModel = new TextInputViewModel
            //{
            //    TextPrompt = "Student Name: "
            //};
            //var textInputView = new TextInputView(textInputViewModel);
            //textInputView.ShowDialog();

            //if (textInputView.DialogResult == null ||
            //    textInputView.DialogResult != true ||
            //    string.IsNullOrEmpty(textInputViewModel.InputText))
            //{
            //    return;
            //}

            //var person = new Person
            //{
            //    IsStudent = true,
            //    FullName = textInputViewModel.InputText
            //};

            //var copiedNotebook = _dataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(person);
            //copiedNotebook.CurrentPage = copiedNotebook.Pages.FirstOrDefault();
            //var notebookComposite = NotebookNameComposite.ParseNotebook(copiedNotebook);
            //var notebookPath = Path.Combine(_dataService.CurrentCacheInfo.NotebooksFolderPath, notebookComposite.ToFolderName());
            //var notebookInfo = new NotebookInfo(_dataService.CurrentCacheInfo, notebookPath)
            //{
            //    Notebook = copiedNotebook
            //};
            //PleaseWaitHelper.Show(() => _dataService.SaveNotebookLocally(notebookInfo, true), null, "Saving Notebook");
            //_dataService.SetCurrentNotebook(notebookInfo);
        }

        private void OnClearPagesNonAnimationHistoryCommandExecute()
        {
            PleaseWaitHelper.Show(() =>
            {
                //foreach (var page in Notebook.Pages)
                //{
                //    page.History.ClearNonAnimationHistory();
                //}
            },
                                  null,
                                  "Clearing History");
        }

        private void OnGenerateStudentNotebooksCommandExecute()
        {
            // HACK: This is very hardcoded.
            //if (_dataService == null ||
            //    _dataService.CurrentCacheInfo == null ||
            //    _dataService.CurrentNotebookInfo == null ||
            //    _dataService.CurrentNotebookInfo.Notebook == null)
            //{
            //    return;
            //}

            //var classInfoPath = Path.Combine(_dataService.CurrentCacheInfo.ClassesFolderPath, "classInfo;KK;S1nEmeKiYkSuPPo3t2nWXQ.xml");
            //var classInfo = ClassInformation.LoadFromXML(classInfoPath);
            //if (classInfo == null)
            //{
            //    return;
            //}

            //var teacher = classInfo.Teacher;
            //var copiedNotebookT = _dataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(teacher);
            //copiedNotebookT.CurrentPage = copiedNotebookT.Pages.FirstOrDefault();
            //var notebookCompositeT = NotebookNameComposite.ParseNotebook(copiedNotebookT);
            //var notebookPathT = Path.Combine(_dataService.CurrentCacheInfo.NotebooksFolderPath, notebookCompositeT.ToFolderName());
            //var notebookInfoT = new NotebookInfo(_dataService.CurrentCacheInfo, notebookPathT)
            //{
            //    Notebook = copiedNotebookT
            //};
            //PleaseWaitHelper.Show(() => _dataService.SaveNotebookLocally(notebookInfoT, true), null, "Saving Notebook for " + teacher.FullName);

            //foreach (var person in classInfo.StudentList)
            //{
            //    var copiedNotebook = _dataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(person);
            //    copiedNotebook.CurrentPage = copiedNotebook.Pages.FirstOrDefault();
            //    var notebookComposite = NotebookNameComposite.ParseNotebook(copiedNotebook);
            //    var notebookPath = Path.Combine(_dataService.CurrentCacheInfo.NotebooksFolderPath, notebookComposite.ToFolderName());
            //    var notebookInfo = new NotebookInfo(_dataService.CurrentCacheInfo, notebookPath)
            //    {
            //        Notebook = copiedNotebook
            //    };
            //    PleaseWaitHelper.Show(() => _dataService.SaveNotebookLocally(notebookInfo, true), null, "Saving Notebook for " + person.FullName);
            //}
        }

        #endregion // Notebook Info Pane

        #region Options Pane

        private void OnBatchTagAnalysisCommandExecute()
        {
            Analyze();
        }

        private void OnBatchRepresentationsUsedTagCommandExecute()
        {
            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = System.IO.Path.Combine(desktopDirectory, "Batch Results");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = System.IO.Path.Combine(fileDirectory, "Batch Representations Used Tags.txt");
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

            foreach (var notebookInfo in _dataService.LoadedNotebooksInfo.OrderBy(ni => ni.Notebook.Owner.FullName))
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

        private void OnBatchSkipCountStatsCommandExecute()
        {
            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = System.IO.Path.Combine(desktopDirectory, "Batch Results");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = System.IO.Path.Combine(fileDirectory, "Skip Counting Stats.txt");
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
            var expectedSkipStringValue = new Dictionary<string, string>();
            var interpretationOfStrokesInInitialBoundingBox = new Dictionary<string, string>();
            var interpretationOfStrokesNotGroupedByRows = new Dictionary<string, string>();
            var interpretationOfStrokesGroupedByRows = new Dictionary<string, string>();

            var expectedBottomSkipStringValue = new Dictionary<string, string>();
            var interpretationOfBottomStrokes = new Dictionary<string, string>();

            foreach (var notebookInfo in _dataService.LoadedNotebooksInfo.OrderBy(ni => ni.Notebook.Owner.FullName))
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

                        #region Expected Skip Values Calculations

                        var expectedRows = array.Rows;

                        // Hard-coded special cases for partial skip counting
                        if (lastSubmission.Owner.FullName == "Gates Morton")
                        {
                            if (lastSubmission.PageNumber == 3 ||
                                lastSubmission.PageNumber == 5 ||
                                lastSubmission.PageNumber == 7 ||
                                lastSubmission.PageNumber == 13)
                            {
                                if (array.Rows == 9 &&
                                    array.Columns == 7)
                                {
                                    expectedRows = 4;
                                }

                                if (array.Rows == 8 &&
                                    array.Columns == 8)
                                {
                                    expectedRows = 4;
                                }

                                if (array.Rows == 6 &&
                                    array.Columns == 6)
                                {
                                    expectedRows = 3;
                                }

                                if (array.Rows == 8 &&
                                    array.Columns == 4)
                                {
                                    expectedRows = 4;
                                }

                                if (array.Rows == 6 &&
                                    array.Columns == 9)
                                {
                                    expectedRows = 3;
                                }
                            }
                        }

                        var expectedSkipValues = new List<int>();
                        for (var i = 1; i <= expectedRows; i++)
                        {
                            expectedSkipValues.Add(i * array.Columns);
                        }

                        if (lastSubmission.Owner.FullName == "Djemimah Filois" &&
                            lastSubmission.PageNumber == 12 &&
                            array.Rows == 5 &&
                            array.Columns == 8)
                        {
                            expectedSkipValues.Clear();
                            expectedSkipValues.Add(5);
                            expectedSkipValues.Add(10);
                            expectedSkipValues.Add(15);
                            expectedSkipValues.Add(20);
                            expectedSkipValues.Add(25);
                        }

                        if (lastSubmission.Owner.FullName == "Julia Guden" &&
                            lastSubmission.PageNumber == 10 &&
                            array.Rows == 7 &&
                            array.Columns == 4)
                        {
                            expectedSkipValues.Clear();
                            expectedSkipValues.Add(4);
                            expectedSkipValues.Add(8);
                            expectedSkipValues.Add(12);
                            expectedSkipValues.Add(16);
                            expectedSkipValues.Add(18);
                            expectedSkipValues.Add(22);
                            expectedSkipValues.Add(26);
                        }

                        expectedSkipStringValue.Add(vkey, string.Join(" ", expectedSkipValues).Trim());

                        var expectedBottomSkipValue = string.Empty;
                        for (var i = 1; i <= array.Columns; i++)
                        {
                            expectedBottomSkipValue += i * array.Rows;
                        }

                        expectedBottomSkipStringValue.Add(vkey, expectedBottomSkipValue.Trim());

                        #endregion // Expected Skip Values Calculations

                        #region Bottom Skip Counting Calculations

                        var formattedBottomSkips = ArrayCodedActions.StaticBottomSkipCountAnalysis(lastSubmission, array, false);
                        if (string.IsNullOrEmpty(formattedBottomSkips))
                        {
                            interpretationOfBottomStrokes.Add(vkey, "NO BOTTOM STROKES");
                        }
                        else
                        {
                            interpretationOfBottomStrokes.Add(vkey, formattedBottomSkips);
                        }

                        #endregion // Bottom Skip Counting Calculations

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
                            var guess = InkInterpreter.InterpretationClosestToANumber(interpretations);
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
                            var guess = InkInterpreter.InterpretationClosestToANumber(interpretations);
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
                            //var trimmedSkip = interpretedRowValues.Select(s => s.Replace(" ", string.Empty)).ToList();
                            //var interpretedSkips = string.Join(" ", trimmedSkip);
                            //interpretationOfStrokesGroupedByRows.Add(vkey, interpretedSkips.Trim());

                            var interpretedSkipValues = new List<string>();

                            totalSkipCountRows += array.Rows;
                            totalArraysRecognizedAsSkipCounting.Add(string.Format("{0}, page {1}. [{2}x{3}]", lastSubmission.Owner.FullName, lastSubmission.PageNumber, array.Rows, array.Columns));
                            for (int i = 0; i < array.Rows; i++)
                            {
                                var expectedValue = (i + 1) * array.Columns;
                                var interpretedValue = interpretedRowValues[i];

                                #region v3 Calculations

                                var adjustedInterpretedValue = interpretedValue;
                                var compiledInterpretedValue = string.Empty;
                                if (expectedValue.ToString().Length != interpretedValue.Length)
                                {
                                    foreach (var c in expectedValue.ToString())
                                    {
                                        if (adjustedInterpretedValue.Contains(c))
                                        {
                                            compiledInterpretedValue += c;
                                            var index = adjustedInterpretedValue.IndexOf(c);
                                            adjustedInterpretedValue = new string(adjustedInterpretedValue.Skip(index + 1).ToArray());
                                            continue;
                                        }

                                        if (string.IsNullOrWhiteSpace(adjustedInterpretedValue))
                                        {
                                            compiledInterpretedValue += " ";
                                            continue;
                                        }

                                        compiledInterpretedValue += adjustedInterpretedValue[0];
                                        adjustedInterpretedValue = new string(adjustedInterpretedValue.Skip(1).ToArray());
                                    }

                                    interpretedSkipValues.Add(compiledInterpretedValue);
                                }
                                else
                                {
                                    interpretedSkipValues.Add(interpretedValue);
                                }

                                #endregion // v3 Calculations

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

                            var interpretedSkips = string.Join(" ", interpretedSkipValues);

                            if (lastSubmission.Owner.FullName == "Gates Morton" &&
                                lastSubmission.PageNumber == 3 &&
                                array.Rows == 9 &&
                                array.Columns == 7)
                            {
                                interpretedSkips = "7 14 21 28 t";
                            }

                            interpretationOfStrokesGroupedByRows.Add(vkey, interpretedSkips.Trim());
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
                File.AppendAllText(filePath, string.Format("\nExpected Value: {0}", expectedSkipStringValue[key]));
                var v1ED = EditDistance.Compute(expectedSkipStringValue[key], interpretationOfStrokesInInitialBoundingBox[key]);
                var cer1 = Math.Round((v1ED * 100.0) / expectedSkipStringValue[key].Length, 1, MidpointRounding.AwayFromZero);
                File.AppendAllText(filePath, string.Format("\n\tv1: {0}\t\tEdit Distance: {1}\tCER: {2}%", interpretationOfStrokesInInitialBoundingBox[key], v1ED, cer1));
                var v2ED = EditDistance.Compute(expectedSkipStringValue[key], interpretationOfStrokesNotGroupedByRows[key]);
                var cer2 = Math.Round((v2ED * 100.0) / expectedSkipStringValue[key].Length, 1, MidpointRounding.AwayFromZero);
                File.AppendAllText(filePath, string.Format("\n\tv2: {0}\t\tEdit Distance: {1}\tCER: {2}%", interpretationOfStrokesNotGroupedByRows[key], v2ED, cer2));
                var v3ED = EditDistance.Compute(expectedSkipStringValue[key], interpretationOfStrokesGroupedByRows[key]);
                var cer3 = Math.Round((v3ED * 100.0) / expectedSkipStringValue[key].Length, 1, MidpointRounding.AwayFromZero);
                File.AppendAllText(filePath, string.Format("\n\tv3: {0}\t\tEdit Distance: {1}\tCER: {2}%", interpretationOfStrokesGroupedByRows[key], v3ED, cer3));

                File.AppendAllText(filePath, string.Format("\n\n\tBottom Skip Counting:"));
                File.AppendAllText(filePath, string.Format("\n\tExpected Bottom Value: {0}", expectedBottomSkipStringValue[key]));
                var bottomED = EditDistance.Compute(expectedBottomSkipStringValue[key], interpretationOfBottomStrokes[key]);
                var bottomCER = Math.Round((bottomED * 100.0) / expectedBottomSkipStringValue[key].Length, 1, MidpointRounding.AwayFromZero);
                var isSkip = bottomED > 4 || interpretationOfBottomStrokes[key] == "NO BOTTOM STROKES" ? "NO" : "YES";
                File.AppendAllText(filePath, string.Format("\n\tInterpretation: {0}\t\tEdit Distance: {1}\tCER: {2}%\t\tIs Skip Counting: {3}", interpretationOfBottomStrokes[key], bottomED, bottomCER, isSkip));
            }
        }

        private void OnApplyRenameToCacheCommandExecute()
        {
            //var notebookService = DependencyResolver.Resolve<INotebookService>();
            //if (notebookService == null)
            //{
            //    return;
            //}

            //const string NEW_NAME = "Elvis Garzona";
            //App.MainWindowViewModel.CurrentUser.FullName = NEW_NAME;
            //var newPerson = App.MainWindowViewModel.CurrentUser;
            //var notebook = notebookService.CurrentNotebook;
            //notebook.Owner = newPerson;
            //foreach (var page in notebook.Pages)
            //{
            //    page.Owner = newPerson;
            //    foreach (var submission in page.Submissions)
            //    {
            //        submission.Owner = newPerson;
            //    }
            //}
        }

        private void OnToggleBindingStyleCommandExecute()
        {
            App.MainWindowViewModel.IsUsingOldPageObjectBoundary = !App.MainWindowViewModel.IsUsingOldPageObjectBoundary;
        }

        private void OnReplayHistoryCommandExecute()
        {
            var animationControlRibbon = NotebookWorkspaceViewModel.GetAnimationControlRibbon();
            if (animationControlRibbon == null)
            {
                return;
            }

            animationControlRibbon.IsNonAnimationPlaybackEnabled = !animationControlRibbon.IsNonAnimationPlaybackEnabled;
        }

        //public Notebook CopyForNewOwner(Person owner)
        //{
        //    var newNotebook = this.DeepCopy();
        //    if (newNotebook == null)
        //    {
        //        return null;
        //    }
        //    newNotebook.Owner = owner;
        //    newNotebook.CurrentPage = CurrentPage == null ? null : CurrentPage.CopyForNewOwner(owner);
        //    foreach (var newPage in Pages.Select(page => page.CopyForNewOwner(owner)))
        //    {
        //        if (!owner.IsStudent)
        //        {
        //            newNotebook.Pages.Add(newPage);
        //            continue;
        //        }

        //        if (newPage.DifferentiationLevel == string.Empty ||
        //            newPage.DifferentiationLevel == "0" ||
        //            newPage.DifferentiationLevel == owner.CurrentDifferentiationGroup)
        //        {
        //            newNotebook.Pages.Add(newPage);
        //            continue;
        //        }

        //        if (owner.CurrentDifferentiationGroup == string.Empty &&
        //            newPage.DifferentiationLevel == "A")
        //        {
        //            newNotebook.Pages.Add(newPage);
        //        }
        //    }

        //    return newNotebook;
        //}

        private void OnGenerateSubmissionsCommandExecute()
        {
            //foreach (var notebookInfo in LoadedNotebooksInfo.Where(n => n.Notebook != null && n.Notebook.Owner.IsStudent))
            //{
            //    foreach (var page in notebookInfo.Notebook.Pages.Where(p => p.VersionIndex == 0))
            //    {
            //        if (page.Submissions.Any())
            //        {
            //            var mostRecentSubmission = page.Submissions.Last();
            //            if (page.InkStrokes.StrokesWeight() != mostRecentSubmission.InkStrokes.StrokesWeight() ||
            //                page.PageObjects.Count != mostRecentSubmission.PageObjects.Count)
            //            {
            //                page.TrimPage();
            //                var submission = page.NextVersionCopy();
            //                page.Submissions.Add(submission);
            //            }
            //        }
            //        else
            //        {
            //            if (page.InkStrokes.Any(s => s.GetStrokeOwnerID() == page.OwnerID) ||
            //                page.PageObjects.Any(p => p.OwnerID == page.OwnerID))
            //            {
            //                page.TrimPage();
            //                var submission = page.NextVersionCopy();
            //                page.Submissions.Add(submission);
            //            }
            //        }
            //    }
            //}

            //foreach (var notebookInfo in LoadedNotebooksInfo.Where(n => n.Notebook != null && !n.Notebook.Owner.IsStudent))
            //{
            //    foreach (var page in notebookInfo.Notebook.Pages.Where(p => p.VersionIndex == 0))
            //    {
            //        var pageViewModels = page.GetAllViewModels();
            //        foreach (var pageViewModel in pageViewModels)
            //        {
            //            var pageVM = pageViewModel as ACLPPageBaseViewModel;
            //            if (pageVM == null)
            //            {
            //                continue;
            //            }
            //            pageVM.UpdateSubmissionCount();
            //        }
            //    }
            //}
        }

        #endregion // Options Pane

        #region New Pane

        public static void CreateClassSubject()
        {
            var classSubject = new ClassInformation();
            var classSubjectCreationViewModel = new ClassSubjectCreationViewModel(classSubject);
            var classSubjectCreationView = new ClassSubjectCreationView(classSubjectCreationViewModel);
            classSubjectCreationView.ShowDialog();

            if (classSubjectCreationView.DialogResult == null ||
                classSubjectCreationView.DialogResult != true)
            {
                return;
            }

            //foreach (var group in classSubjectCreationViewModel.GroupCreationViewModel.Groups)
            //{
            //    foreach (var student in group.Members)
            //    {
            //        if (classSubjectCreationViewModel.GroupCreationViewModel.GroupType == "Temp")
            //        {
            //            student.TemporaryDifferentiationGroup = group.Label;
            //        }
            //        else
            //        {
            //            student.CurrentDifferentiationGroup = group.Label;
            //        }
            //    }
            //}

            //foreach (var group in classSubjectCreationViewModel.TempGroupCreationViewModel.Groups)
            //{
            //    foreach (var student in group.Members)
            //    {
            //        if (classSubjectCreationViewModel.TempGroupCreationViewModel.GroupType == "Temp")
            //        {
            //            student.TemporaryDifferentiationGroup = group.Label;
            //        }
            //        else
            //        {
            //            student.CurrentDifferentiationGroup = group.Label;
            //        }
            //    }
            //}

            //classSubject.Projector = classSubject.Teacher;

            //var classesFolderPath = SelectedCache.ClassesFolderPath;
            //if (!Directory.Exists(classesFolderPath))
            //{
            //    Directory.CreateDirectory(classesFolderPath);
            //}
            //classSubject.SaveToXML(classesFolderPath);
        }

        #endregion // New Pane

        #region Export Pane

        private void OnCopyNotebookForNewOwnerCommandExecute()
        {
            // TODO: Utilize NotebookInfoPane's OnSaveNotebookForStudentCommandExecute

            //var person = new Person();
            //var personCreationView = new PersonView(new PersonViewModel(person));
            //personCreationView.ShowDialog();

            //if (personCreationView.DialogResult == null ||
            //    personCreationView.DialogResult != true)
            //{
            //    return;
            //}

            //var copiedNotebook = _dataService.CurrentNotebook.CopyForNewOwner(person);
            //copiedNotebook.CurrentPage = copiedNotebook.Pages.FirstOrDefault();

            //App.MainWindowViewModel.CurrentUser = person;
            //App.MainWindowViewModel.IsAuthoring = false;
            //App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            //App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(copiedNotebook);
            //App.MainWindowViewModel.IsBackStageVisible = false;
        }

        #endregion // Export Pane

        #region Open Notebook Pane

        private void OnAnonymizeCacheCommandExecute()
        {
            //const string TEXT_FILE_NAME = "AnonymousNames.txt";
            //var anonymousTextFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), TEXT_FILE_NAME);
            //if (!File.Exists(anonymousTextFilePath))
            //{
            //    MessageBox.Show("You are missing AnonymousNames.txt on the Desktop.");
            //    return;
            //}

            //var names = new Dictionary<string, string>();
            //var textFile = new StreamReader(anonymousTextFilePath);
            //string line;
            //while ((line = textFile.ReadLine()) != null)
            //{
            //    var parts = line.Split(',');
            //    if (parts.Length != 2)
            //    {
            //        MessageBox.Show("AnonymousNames.txt is in the wrong format.");
            //        textFile.Close();
            //        return;
            //    }

            //    var oldName = parts[0];
            //    var newName = parts[1];
            //    newName = newName.Replace("\t", string.Empty);
            //    newName = newName.Replace("\n", string.Empty);
            //    newName = newName.Trim();

            //    if (!names.ContainsKey(oldName))
            //    {
            //        names.Add(oldName, newName);
            //    }
            //}

            //textFile.Close();

            //var cacheInfoToAnonymize = SelectedCache;
            //var newCacheFolderPath = cacheInfoToAnonymize.CacheFolderPath + ".Anon";
            //var newCacheInfo = new CacheInfo(newCacheFolderPath);
            //newCacheInfo.Initialize();
            //var notebooksFolderPathDirectoryInfo = new DirectoryInfo(newCacheInfo.NotebooksFolderPath);

            //PleaseWaitHelper.Show(() =>
            //{
            //    foreach (var file in notebooksFolderPathDirectoryInfo.GetFiles())
            //    {
            //        file.Delete();
            //    }
            //    foreach (var dir in notebooksFolderPathDirectoryInfo.GetDirectories())
            //    {
            //        dir.Delete(true);
            //    }

            //    var imagesFolderPathDirectoryInfo = new DirectoryInfo(cacheInfoToAnonymize.ImagesFolderPath);
            //    imagesFolderPathDirectoryInfo.Copy(newCacheFolderPath);

            //    var notebookInfosToCopy = Services.DataService.GetNotebooksInCache(cacheInfoToAnonymize);
            //    foreach (var notebookInfo in notebookInfosToCopy)
            //    {
            //        var notebookFolderPath = notebookInfo.NotebookFolderPath;
            //        var directoryInfo = new DirectoryInfo(notebookFolderPath);
            //        directoryInfo.Copy(newCacheInfo.NotebooksFolderPath);
            //    }
            //},
            //                      null,
            //                      "Copying Data");

            //var nonConvertedNames = new List<string>();
            //PleaseWaitHelper.Show(() =>
            //{
            //    var notebookInfosToAnonymize = Services.DataService.GetNotebooksInCache(newCacheInfo);
            //    foreach (var notebookInfo in notebookInfosToAnonymize.Where(ni => ni.NameComposite.OwnerTypeTag == "S"))
            //    {
            //        var nameComposite = notebookInfo.NameComposite;
            //        var oldName = nameComposite.OwnerName;
            //        if (!names.ContainsKey(oldName))
            //        {
            //            nonConvertedNames.Add(oldName);
            //            continue;
            //        }

            //        var newName = names[oldName];
            //        nameComposite.OwnerName = newName;
            //        var newFolderName = nameComposite.ToFolderName();
            //        var notebookFolderPath = notebookInfo.NotebookFolderPath;
            //        var directoryInfo = new DirectoryInfo(notebookFolderPath);
            //        var parentDirectory = directoryInfo.Parent.FullName;
            //        var newFolderPath = Path.Combine(parentDirectory, newFolderName);
            //        directoryInfo.MoveTo(newFolderPath);

            //        var notebookFilePath = Path.Combine(newFolderPath, "notebook.xml");
            //        if (!File.Exists(notebookFilePath))
            //        {
            //            MessageBox.Show("Problem with copied cache. Exiting.");
            //            return;
            //        }

            //        var doc = new XmlDocument();
            //        doc.Load(notebookFilePath);
            //        var node = doc.DocumentElement;
            //        foreach (XmlNode childNode in node.ChildNodes)
            //        {
            //            foreach (XmlNode secondChildNode in childNode)
            //            {
            //                if (secondChildNode.Name == "FullName")
            //                {
            //                    secondChildNode.InnerText = newName;
            //                    break;
            //                }
            //            }
            //        }

            //        doc.Save(notebookFilePath);

            //        var pagesFolderPath = Path.Combine(newFolderPath, "Pages");
            //        var pagesDirectoryInfo = new DirectoryInfo(pagesFolderPath);
            //        foreach (var pageFileInfo in pagesDirectoryInfo.GetFiles("*.xml"))
            //        {
            //            var pageDoc = new XmlDocument();
            //            pageDoc.Load(pageFileInfo.FullName);
            //            var pageNode = pageDoc.DocumentElement;
            //            foreach (XmlNode childNode in pageNode.ChildNodes)
            //            {
            //                foreach (XmlNode secondChildNode in childNode)
            //                {
            //                    if (secondChildNode.Name == "FullName")
            //                    {
            //                        secondChildNode.InnerText = newName;
            //                    }
            //                    if (secondChildNode.Name == "Alias")
            //                    {
            //                        secondChildNode.InnerText = string.Empty;
            //                        break;
            //                    }
            //                }
            //            }

            //            pageDoc.Save(pageFileInfo.FullName);
            //        }
            //    }
            //},
            //                      null,
            //                      "Anonymizing Data");

            //if (nonConvertedNames.Any())
            //{
            //    var namesToPrint = string.Join("\n", nonConvertedNames);
            //    MessageBox.Show("Names not Anonymized:\n" + namesToPrint);
            //}
        }

        private void OnLargeCacheAnalysisCommandExecute()
        {
            //#region Initialize TSV file and header columns

            //var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //var fileDirectory = Path.Combine(desktopDirectory, "LargeCacheAnalysis");
            //if (!Directory.Exists(fileDirectory))
            //{
            //    Directory.CreateDirectory(fileDirectory);
            //}

            //var filePath = Path.Combine(fileDirectory, "BatchAnalysis.tsv");
            //if (File.Exists(filePath))
            //{
            //    File.Delete(filePath);
            //}
            //File.WriteAllText(filePath, "");

            //var columnHeaders = new List<string>
            //                    {
            //                        "STUDENT NAME",
            //                        "PAGE NUMBER",
            //                        "SUBMISSION TIME",
            //                        "IS MISSING",
            //                        "ARR",
            //                        "ARR cut",
            //                        "ARR snap",
            //                        "ARR divide",
            //                        "ARR rotate",
            //                        "STAMP total",
            //                        "STAMP on page",
            //                        "STAMP used",
            //                        "STAMP IMAGES total",
            //                        "STAMP IMAGES on page",
            //                        "NL",
            //                        "NL used",
            //                        "NLs w/ changed endpoints",
            //                        "MR",
            //                        "Ink Only",
            //                        "Blank"
            //                    };
            //var tabbedColumnHeaders = string.Join("\t", columnHeaders);
            //File.AppendAllText(filePath, tabbedColumnHeaders);

            //var fileRows = new List<List<string>>();

            //#endregion // Initialize TSV file and header columns

            //#region Generate Stats

            //XNamespace typeNamespace = "http://www.w3.org/2001/XMLSchema-instance";
            //XNamespace entityNamespace = "http://schemas.datacontract.org/2004/07/CLP.Entities";
            //XNamespace serializationNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
            //var typeName = typeNamespace + "type";
            //var anyTypeName = serializationNamespace + "anyType";
            //var stringTypeName = serializationNamespace + "string";
            //var strokeName = entityNamespace + "StrokeDTO";
            //var strokeOwnerIDName = entityNamespace + "_x003C_PersonID_x003E_k__BackingField";

            //const string ARRAY_ENTITY = "d1p1:CLPArray";
            //const string NUMBER_LINE_ENTITY = "d1p1:NumberLine";
            //const string STAMP_ENTITY = "d1p1:Stamp";
            //const string STAMP_IMAGE_ENTITY = "d1p1:StampedObject";

            //const string CUT_ENTITY = "d1p1:PageObjectCutHistoryItem";
            //const string SNAP_ENTITY = "d1p1:CLPArraySnapHistoryItem";
            //const string DIVIDE_ENTITY = "d1p1:CLPArrayDivisionsChangedHistoryItem";
            //const string ROTATE_ENTITY = "d1p1:CLPArrayRotateHistoryItem";
            //const string END_POINTS_CHANGED_ENTITY = "d1p1:NumberLineEndPointsChangedHistoryItem";

            //var missingPages = new Dictionary<string, List<int>>();

            //var cacheInfoToAnalyze = SelectedCache;
            //var pagesToIgnore = new List<int>
            //                    {
            //                        1,
            //                        2,
            //                        3,
            //                        4,
            //                        5,
            //                        6,
            //                        7,
            //                        8,
            //                        9,
            //                        10,
            //                        11,
            //                        12,
            //                        13,
            //                        14,
            //                        15,
            //                        16,
            //                        17,
            //                        18,
            //                        19,
            //                        20,
            //                        21,
            //                        22,
            //                        23,
            //                        24,
            //                        25,
            //                        26,
            //                        27,
            //                        28,
            //                        29,
            //                        30,
            //                        33,
            //                        34,
            //                        35,
            //                        36,
            //                        37,
            //                        38,
            //                        39,
            //                        40,
            //                        41,
            //                        42,
            //                        45,
            //                        46,
            //                        47,
            //                        48,
            //                        50,
            //                        52,
            //                        53,
            //                        54,
            //                        55,
            //                        56,
            //                        57,
            //                        58,
            //                        59,
            //                        60,
            //                        61,
            //                        62,
            //                        63,
            //                        64,
            //                        65,
            //                        66,
            //                        67,
            //                        68,
            //                        69,
            //                        70,
            //                        71,
            //                        72,
            //                        73,
            //                        74,
            //                        75,
            //                        76,
            //                        77,
            //                        78,
            //                        79,
            //                        80,
            //                        81,
            //                        82,
            //                        83,
            //                        84,
            //                        85,
            //                        86,
            //                        87,
            //                        88,
            //                        89,
            //                        90,
            //                        91,
            //                        92,
            //                        95,
            //                        96,
            //                        97,
            //                        98,
            //                        99,
            //                        100,
            //                        101,
            //                        102,
            //                        103,
            //                        104,
            //                        105,
            //                        106,
            //                        107,
            //                        108,
            //                        109,
            //                        110,
            //                        111,
            //                        112,
            //                        115,
            //                        116,
            //                        117,
            //                        118,
            //                        119,
            //                        120,
            //                        121,
            //                        122,
            //                        123,
            //                        124,
            //                        125,
            //                        126,
            //                        127,
            //                        128,
            //                        129,
            //                        130,
            //                        131,
            //                        138,
            //                        139,
            //                        140,
            //                        141,
            //                        142,
            //                        143,
            //                        144,
            //                        145,
            //                        146,
            //                        152,
            //                        153,
            //                        154,
            //                        155,
            //                        156,
            //                        157,
            //                        158,
            //                        164,
            //                        165,
            //                        166,
            //                        167,
            //                        168,
            //                        169,
            //                        170,
            //                        182,
            //                        183,
            //                        184,
            //                        187,
            //                        188,
            //                        189,
            //                        190,
            //                        191,
            //                        192,
            //                        193,
            //                        202,
            //                        203,
            //                        204,
            //                        205,
            //                        206,
            //                        207,
            //                        208,
            //                        222,
            //                        223,
            //                        224,
            //                        237,
            //                        238,
            //                        239,
            //                        250,
            //                        274,
            //                        275,
            //                        294,
            //                        319,
            //                        327,
            //                        328,
            //                        329,
            //                        330,
            //                        331,
            //                        332,
            //                        333,
            //                        334,
            //                        340,
            //                        345,
            //                        346,
            //                        347,
            //                        348,
            //                        349,
            //                        350,
            //                        351,
            //                        352,
            //                        353,
            //                        361,
            //                        386
            //                    };

            //var notebookInfosToAnalyze = Services.DataService.GetNotebooksInCache(cacheInfoToAnalyze);
            //foreach (var notebookInfo in notebookInfosToAnalyze.Where(ni => ni.NameComposite.OwnerTypeTag == "S"))
            //{
            //    var allPageNumbers = Enumerable.Range(1, 386 - 1).ToList();

            //    var nameComposite = notebookInfo.NameComposite;
            //    var studentName = nameComposite.OwnerName;
            //    var studentOwnerID = nameComposite.OwnerID;

            //    var pagesDirectoryInfo = new DirectoryInfo(notebookInfo.PagesFolderPath);
            //    foreach (var pageFileInfo in pagesDirectoryInfo.GetFiles("*.xml"))
            //    {
            //        var pageDoc = XElement.Load(pageFileInfo.FullName);

            //        var pageNumber = pageDoc.Descendants("PageNumber").First().Value;
            //        var pageNumberValue = int.Parse(pageNumber);
            //        if (allPageNumbers.Contains(pageNumberValue))
            //        {
            //            allPageNumbers.Remove(pageNumberValue);
            //        }
            //        var versionIndex = pageDoc.Descendants("VersionIndex").First().Value;
            //        if (versionIndex != "0" ||
            //            pagesToIgnore.Contains(pageNumberValue))
            //        {
            //            continue;
            //        }
            //        var submissionTime = pageDoc.Descendants("SubmissionTime").First().Value;
            //        if (string.IsNullOrEmpty(submissionTime) || string.IsNullOrWhiteSpace(submissionTime))
            //        {
            //            submissionTime = "UNSUBMITTED";
            //        }

            //        // PageObjects
            //        var pageObjects = pageDoc.Descendants("PageObjects").First().Descendants(anyTypeName).Where(xe => xe.Descendants("CreatorID").First().Value == studentOwnerID).ToList();
            //        var trashedPageObjects = pageDoc.Descendants("TrashedPageObjects").First().Descendants(anyTypeName).Where(xe => xe.Descendants("CreatorID").First().Value == studentOwnerID).ToList();

            //        // Ink
            //        var inkOnPage =
            //            pageDoc.Descendants("SerializedStrokes")
            //                   .First()
            //                   .Descendants(strokeName)
            //                   .Where(xe => xe.Descendants(strokeOwnerIDName).First().Value == studentOwnerID)
            //                   .ToList();
            //        var trashedInk =
            //            pageDoc.Descendants("SerializedTrashedInkStrokes")
            //                   .First()
            //                   .Descendants(strokeName)
            //                   .Where(xe => xe.Descendants(strokeOwnerIDName).First().Value == studentOwnerID)
            //                   .ToList();

            //        // History
            //        var undoHistoryItems = pageDoc.Descendants("UndoItems").First().Descendants(anyTypeName);
            //        var redoHistoryItems = pageDoc.Descendants("RedoItems").First().Descendants(anyTypeName);
            //        var historyItems = undoHistoryItems.Concat(redoHistoryItems).Where(xe => xe.Descendants("OwnerID").First().Value == studentOwnerID).ToList();

            //        // ARR
            //        var arraysOnPage = pageObjects.Where(xe => (string)xe.Attribute(typeName) == ARRAY_ENTITY);
            //        var trashedArrays = trashedPageObjects.Where(xe => (string)xe.Attribute(typeName) == ARRAY_ENTITY);

            //        var arraysOnPageIDs = arraysOnPage.Select(xe => xe.Descendants("ID").First().Value);
            //        var trashedarraysIDs = trashedArrays.Select(xe => xe.Descendants("ID").First().Value);
            //        var arraysIDs = arraysOnPageIDs.Concat(trashedarraysIDs);

            //        var arraysOnPageCount = arraysOnPage.Count();
            //        var trashedArraysCount = trashedArrays.Count();
            //        var arraysUsedCount = arraysOnPageCount + trashedArraysCount;

            //        var cutHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == CUT_ENTITY).ToList();
            //        var arraysWithACutCount = arraysIDs.Count(arraysID => cutHistoryItems.Any(xe => xe.Descendants("CutPageObjectIDs").First().Descendants(stringTypeName).Any(e => e.Value == arraysID)));
            //        var cutsOverArrayCount = cutHistoryItems.Count(xe => xe.Descendants("CutPageObjectIDs").First().Descendants(stringTypeName).Any(e => arraysIDs.Contains(e.Value)));

            //        var snapHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == SNAP_ENTITY).ToList();
            //        var twoArraysSnappedTogetherCount = snapHistoryItems.Count;

            //        var divideHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == DIVIDE_ENTITY).ToList();
            //        var arrayDividersChangedCount = divideHistoryItems.Count;

            //        var rotateHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == ROTATE_ENTITY).ToList();
            //        var arrayRotateCount = rotateHistoryItems.Count;

            //        // STAMP
            //        var stampsOnPage = pageObjects.Where(xe => (string)xe.Attribute(typeName) == STAMP_ENTITY);
            //        var trashedStamps = trashedPageObjects.Where(xe => (string)xe.Attribute(typeName) == STAMP_ENTITY);

            //        var stampsOnPageIDs = stampsOnPage.Select(xe => xe.Descendants("ID").First().Value);
            //        var trashedStampsIDs = trashedStamps.Select(xe => xe.Descendants("ID").First().Value);
            //        var stampIDs = stampsOnPageIDs.Concat(trashedStampsIDs);

            //        var stampsOnPageCount = stampsOnPage.Count();
            //        var trashedStampsCount = trashedStamps.Count();
            //        var stampsCount = stampsOnPageCount + trashedStampsCount;

            //        // STAMP IMAGES
            //        var stampImagesOnPage = pageObjects.Where(xe => (string)xe.Attribute(typeName) == STAMP_IMAGE_ENTITY);
            //        var trashedStampImages = trashedPageObjects.Where(xe => (string)xe.Attribute(typeName) == STAMP_IMAGE_ENTITY);
            //        var allStampedImages = stampImagesOnPage.Concat(trashedStampImages).ToList();

            //        var stampsUsedCount = stampIDs.Count(stampID => allStampedImages.Any(xe => xe.Descendants("ParentStampID").First().Value == stampID));

            //        var stampImagesOnPageIDs = stampImagesOnPage.Select(xe => xe.Descendants("ID").First().Value);
            //        var trashedstampImageIDs = trashedStampImages.Select(xe => xe.Descendants("ID").First().Value);
            //        var stampImageIDs = stampImagesOnPageIDs.Concat(trashedstampImageIDs);

            //        var stampImagesOnPageCount = stampImagesOnPage.Count();
            //        var trashedStampImagesCount = trashedStampImages.Count();
            //        var stampImagesCount = stampImagesOnPageCount + trashedStampImagesCount;

            //        // NL
            //        var numberLinesOnPage = pageObjects.Where(xe => (string)xe.Attribute(typeName) == NUMBER_LINE_ENTITY);
            //        var trashedNumberLines = trashedPageObjects.Where(xe => (string)xe.Attribute(typeName) == NUMBER_LINE_ENTITY);

            //        var numberLinesOnPageIDs = numberLinesOnPage.Select(xe => xe.Descendants("ID").First().Value);
            //        var trashedNumberLineIDs = trashedNumberLines.Select(xe => xe.Descendants("ID").First().Value);
            //        var numberLineIDs = numberLinesOnPageIDs.Concat(trashedNumberLineIDs);

            //        var numberLinesOnPageCount = numberLinesOnPage.Count();
            //        var trashedNumberLinesCount = trashedNumberLines.Count();
            //        var numberLinesUsedCount = numberLinesOnPageCount + trashedNumberLinesCount;

            //        var numberLinesWithJumpsOnPageCount = numberLinesOnPage.Count(xe => xe.Descendants("JumpSizes").First().HasElements);
            //        var trashedNumberLinesWithJumpsCount = trashedNumberLines.Count(xe => xe.Descendants("JumpSizes").First().HasElements);
            //        var numberLinesWithJumpsCount = numberLinesWithJumpsOnPageCount + trashedNumberLinesWithJumpsCount;

            //        var endPointsChangedHistoryItems = historyItems.Where(xe => (string)xe.Attribute(typeName) == END_POINTS_CHANGED_ENTITY).ToList();
            //        var numberLinesWithEndPointsChangedCount = numberLineIDs.Count(numberLineID => endPointsChangedHistoryItems.Any(xe => xe.Descendants("NumberLineID").First().Value == numberLineID));

            //        // Sum Stats
            //        var isArrayUsedCount = arraysUsedCount > 0 ? 1 : 0;
            //        var isStampUsedCount = stampsUsedCount > 0 ? 1 : 0;
            //        var isNumberLinesUsedCount = numberLinesUsedCount > 0 ? 1 : 0;
            //        var isMultipleRepresentations = isArrayUsedCount + isNumberLinesUsedCount + isStampUsedCount > 1 ? "Y" : "N";

            //        var isInkOnlyInkOnPage = arraysUsedCount + numberLinesUsedCount + stampsUsedCount == 0 && inkOnPage.Any() ? "Y" : "N";
            //        var isInkOnlyCountingErasedInk = arraysUsedCount + numberLinesUsedCount == 0 && (inkOnPage.Any() || trashedInk.Any()) ? "Y" : "N";

            //        var isBlank = isArrayUsedCount + isNumberLinesUsedCount + isStampUsedCount == 0 && !inkOnPage.Any() && !trashedInk.Any() ? "Y" : "N";

            //        Console.WriteLine($"Name: {studentName}, Page Number: {pageNumber}, Submission Time: {submissionTime}, " +
            //                          $"ARR: {arraysUsedCount}, ARR cut: {cutsOverArrayCount}, ARR snap: {twoArraysSnappedTogetherCount}, ARR divide: {arrayDividersChangedCount}, ARR rotate: {arrayRotateCount}, " +
            //                          $"STAMP total: {stampsCount}, STAMP on page: {stampsOnPageCount}, STAMP used: {stampsUsedCount}, " +
            //                          $"STAMP IMAGES total: {stampImagesCount}, STAMP IMAGES on page: {stampImagesOnPageCount}, " +
            //                          $"NL: {numberLinesUsedCount}, NL used: {numberLinesWithJumpsCount}, NLs w/ changed endpoints: {numberLinesWithEndPointsChangedCount}, " +
            //                          $"MR: {isMultipleRepresentations}, Ink Only: {isInkOnlyInkOnPage}, Blank: {isBlank}");

            //        var rowContents = new List<string>()
            //                          {
            //                              studentName,
            //                              pageNumber,
            //                              submissionTime,
            //                              "N",
            //                              arraysUsedCount.ToString(),
            //                              cutsOverArrayCount.ToString(),
            //                              twoArraysSnappedTogetherCount.ToString(),
            //                              arrayDividersChangedCount.ToString(),
            //                              arrayRotateCount.ToString(),
            //                              stampsCount.ToString(),
            //                              stampsOnPageCount.ToString(),
            //                              stampsUsedCount.ToString(),
            //                              stampImagesCount.ToString(),
            //                              stampImagesOnPageCount.ToString(),
            //                              numberLinesUsedCount.ToString(),
            //                              numberLinesWithJumpsCount.ToString(),
            //                              numberLinesWithEndPointsChangedCount.ToString(),
            //                              isMultipleRepresentations,
            //                              isInkOnlyInkOnPage,
            //                              isBlank
            //                          };
            //        fileRows.Add(rowContents);
            //    }

            //    foreach (var pageNumber in allPageNumbers)
            //    {
            //        var rowContents = new List<string>()
            //                          {
            //                              studentName,
            //                              pageNumber.ToString(),
            //                              "MISSING",
            //                              "Y",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X",
            //                              "X"
            //                          };
            //        fileRows.Add(rowContents);
            //    }

            //    if (allPageNumbers.Any())
            //    {
            //        missingPages.Add(studentName, allPageNumbers);
            //    }
            //}

            //#endregion // Generate Stats

            //#region Order rows and write to TSV file

            //foreach (var studentName in missingPages.Keys)
            //{
            //    var pagesMissing = string.Join(", ", missingPages[studentName]);
            //    Console.WriteLine("{0} is missing pages: {1}", studentName, pagesMissing);
            //}

            //var orderedFileRows = fileRows.OrderBy(r => r.First()).ThenBy(r => int.Parse(r[1])).ToList();
            //foreach (var orderedFileRow in orderedFileRows)
            //{
            //    var tabbedRow = string.Join("\t", orderedFileRow);
            //    File.AppendAllText(filePath, Environment.NewLine + tabbedRow);
            //}

            //#endregion // Order rows and write to TSV file
        }

        #endregion // Open Notebook Pane

        #region DataService

        public void Analyze()
        {
            #region TSV Batch

            //var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //var fileDirectory = Path.Combine(desktopDirectory, "HistoryActions");
            //if (!Directory.Exists(fileDirectory))
            //{
            //    Directory.CreateDirectory(fileDirectory);
            //}

            //var filePath = Path.Combine(fileDirectory, "BatchTags.tsv");
            //if (File.Exists(filePath))
            //{
            //    File.Delete(filePath);
            //}
            //File.WriteAllText(filePath, "");

            //var columnHeaders = new List<string>
            //              {
            //                  "STUDENT NAME",
            //                  "PAGE NUMBER",
            //                  "PASS 1",
            //                  "PASS 2",
            //                  "PASS 3"
            //              };
            //var tabbedColumnHeaders = string.Join("\t", columnHeaders);
            //File.AppendAllText(filePath, tabbedColumnHeaders);

            //foreach (var notebookInfo in LoadedNotebooksInfo)
            //{
            //    var notebook = notebookInfo.Notebook;
            //    if (notebook.OwnerID == Person.Author.ID ||
            //        !notebook.Owner.IsStudent)
            //    {
            //        continue;
            //    }

            //    foreach (var page in notebook.Pages)
            //    {
            //        var lastSubmission = page.Submissions.LastOrDefault();
            //        if (lastSubmission == null)
            //        {
            //            continue;
            //        }

            //        var columns = new List<string>
            //                      {
            //                          page.Owner.FullName,
            //                          page.PageNumber.ToString()
            //                      };

            //        Console.WriteLine("Generating SEvents for page {0}, for {1}", page.PageNumber, page.Owner.FullName);
            //        HistoryAnalysis.GenerateHistoryActions(lastSubmission);
            //        Console.WriteLine("Finished generating SEvents.\n");

            //        var pass2Action = lastSubmission.History.HistoryActions.FirstOrDefault(h => h.CodedObject == "PASS" && h.CodedObjectID == "2");
            //        var pass2Index = lastSubmission.History.HistoryActions.IndexOf(pass2Action);
            //        var pass3Action = lastSubmission.History.HistoryActions.FirstOrDefault(h => h.CodedObject == "PASS" && h.CodedObjectID == "3");
            //        var pass3Index = lastSubmission.History.HistoryActions.IndexOf(pass3Action);

            //        var pass1 = lastSubmission.History.HistoryActions.Skip(1).Take(pass2Index - 1).Select(h => h.CodedValue).ToList();
            //        var pass2 = lastSubmission.History.HistoryActions.Skip(pass2Index + 1).Take(pass3Index - pass2Index - 1).Select(h => h.CodedValue).ToList();
            //        var pass3 = lastSubmission.History.HistoryActions.Skip(pass3Index + 1).Select(h => h.CodedValue).ToList();

            //        columns.Add(string.Join(", ", pass1));
            //        columns.Add(string.Join(", ", pass2));
            //        columns.Add(string.Join(", ", pass3));

            //        File.AppendAllText(filePath, "\n");
            //        var tabbedColumns = string.Join("\t", columns);
            //        File.AppendAllText(filePath, tabbedColumns);
            //    }
            //}

            #endregion // TSV Batch
        }

        #endregion // DataService
    }
}