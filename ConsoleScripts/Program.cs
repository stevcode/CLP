using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Collections;
using Catel.Data;
using CLP.Entities;
using Path = Catel.IO.Path;

namespace ConsoleScripts
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //while(true)
            //{
            //    Console.Write("> ");
            //    string command = Console.ReadLine();
            //    if(!processCommand(command))
            //    {
            //        break;
            //    }
            //}

            //while(true)
            //{
            //    Console.Write("> ");
            //    var command = Console.ReadLine();
            //    if(command == null)
            //    {
            //        continue;
            //    }
            //    if(command == "exit")
            //    {
            //        return;
            //    }
            //    var compactID = new Guid(command).ToCompactID();
            //    Console.WriteLine("CompactID: " + compactID);
            //}

            Convert();
            Console.WriteLine("*****Finished*****");
            Console.ReadLine();
        }

        private static void Convert()
        {
            var convertFromFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Convert");
            //var convertToFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Converted");
            //if (!Directory.Exists(convertToFolderPath))
            //{
            //    Directory.CreateDirectory(convertToFolderPath);
            //}

            var notebookFolderPaths = Directory.EnumerateDirectories(convertFromFolderPath);
            foreach (var notebookFolderPath in notebookFolderPaths)
            {
                //var filePath = Path.Combine(notebookFolderPath, "notebook.xml");
                //var notebook = ModelBase.Load<Notebook>(filePath, SerializationMode.Xml);
                var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
                var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                foreach (var pageFilePath in pageFilePaths)
                {
                    var page = ModelBase.Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                    page.AfterDeserialization();

                    Console.WriteLine("Loaded {3}'s page {0}, differentiation {1}, version {2}", page.PageNumber, page.DifferentiationLevel, page.VersionIndex, page.Owner.FullName);
                    //Do stuff to each page here. 

                    _isConvertingEmilyCache = false;
                    ConvertDivisionTemplatesToUseNewRemainderTiles(page);
                    TheSlowRewind(page);

                    //Finished doing stuff to page, it'll save below.
                    page.ToXML(pageFilePath, true);
                }
            }
        }

        private static bool _isConvertingEmilyCache;

        public static void ConvertDivisionTemplatesToUseNewRemainderTiles(CLPPage page)
        {
            foreach (var divisionTemplate in page.PageObjects.OfType<FuzzyFactorCard>().Where(d => d.RemainderTiles != null))
            {
                divisionTemplate.IsRemainderTilesVisible = true;
            }

            foreach (var divisionTemplate in page.History.TrashedPageObjects.OfType<FuzzyFactorCard>().Where(d => d.RemainderTiles != null))
            {
                divisionTemplate.IsRemainderTilesVisible = true;
            }
        }

        public static void FixOldDivisionTemplateSizing(FuzzyFactorCard divisionTemplate)
        {
            if (!_isConvertingEmilyCache)
            {
                return;
            }

            var gridSize = divisionTemplate.ArrayHeight / divisionTemplate.Rows;

            divisionTemplate.SizeArrayToGridLevel(gridSize, false);

            var position = 0.0;
            foreach (var division in divisionTemplate.VerticalDivisions)
            {
                division.Position = position;
                division.Length = divisionTemplate.GridSquareSize * division.Value;
                position = division.Position + division.Length;
            }
        }

        public static void TheSlowRewind(CLPPage page)
        {
            //Rewind entire page
            page.History.IsAnimating = true;

            page.History.RefreshHistoryIndexes();

            while (page.History.UndoItems.Any())
            {
                var historyItemToUndo = page.History.UndoItems.FirstOrDefault();
                if (historyItemToUndo == null)
                {
                    break;
                }

                #region WorksAsIs

                if (historyItemToUndo is AnimationIndicator ||
                    historyItemToUndo is CLPArrayRotateHistoryItem ||
                    historyItemToUndo is CLPArrayGridToggleHistoryItem ||
                    historyItemToUndo is CLPArrayDivisionValueChangedHistoryItem ||
                    historyItemToUndo is FFCArrayRemovedHistoryItem ||
                    historyItemToUndo is FFCArraySnappedInHistoryItem ||
                    historyItemToUndo is RemainderTilesVisibilityToggledHistoryItem ||
                    historyItemToUndo is PartsValueChangedHistoryItem ||
                    historyItemToUndo is CLPArraySnapHistoryItem)
                {
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //WorksAsIs

                #region CLPArrayDivisionsChanged fix

                if (historyItemToUndo is CLPArrayDivisionsChangedHistoryItem)
                {
                    var divisionsChanged = historyItemToUndo as CLPArrayDivisionsChangedHistoryItem;
                    if (!divisionsChanged.AddedDivisions.Any() &&
                        !divisionsChanged.RemovedDivisions.Any())
                    {
                        page.History.UndoItems.RemoveFirst();
                        continue;
                    }

                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //CLPArrayDivisionsChanged fix

                #region PageObjectResize fix for old Division Templates

                if (historyItemToUndo is PageObjectResizeBatchHistoryItem)
                {
                    var pageObjectResized = historyItemToUndo as PageObjectResizeBatchHistoryItem;
                    var divisionTemplate = page.GetVerifiedPageObjectOnPageByID(pageObjectResized.PageObjectID) as FuzzyFactorCard;
                    if (divisionTemplate != null)
                    {
                        FixOldDivisionTemplateSizing(divisionTemplate);
                        var fixStretchedDimensions = (from point in pageObjectResized.StretchedDimensions
                                                      let height = point.Y
                                                      let gridSize = (height - (2 * divisionTemplate.LabelLength)) / divisionTemplate.Rows
                                                      let newWidth = (gridSize * divisionTemplate.Columns) + divisionTemplate.LabelLength + divisionTemplate.LargeLabelLength
                                                      select new Point(newWidth, point.Y)).ToList();

                        pageObjectResized.StretchedDimensions = fixStretchedDimensions;
                    }

                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectResize fix for old Division Templates

                #region PageObjectsAdded to ObjectsOnPageChanged

                if (historyItemToUndo is PageObjectsAddedHistoryItem)
                {
                    var pageObjectsAdded = historyItemToUndo as PageObjectsAddedHistoryItem;
                    page.History.UndoItems.RemoveFirst();
                    if (!pageObjectsAdded.PageObjectIDs.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryItem(pageObjectsAdded);

                    foreach (var id in objectsChanged.PageObjectIDsAdded)
                    {
                        var divisionTemplate = page.GetVerifiedPageObjectOnPageByID(id) as FuzzyFactorCard;
                        if (divisionTemplate != null)
                        {
                            FixOldDivisionTemplateSizing(divisionTemplate);
                        }
                    }

                    page.History.UndoItems.Insert(0, objectsChanged);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectsAdded to ObjectsOnPageChanged

                #region PageObjectsRemoved to ObjectsOnPageChanged

                if (historyItemToUndo is PageObjectsRemovedHistoryItem)
                {
                    var pageObjectsRemoved = historyItemToUndo as PageObjectsRemovedHistoryItem;
                    page.History.UndoItems.RemoveFirst();
                    if (!pageObjectsRemoved.PageObjectIDs.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryItem(pageObjectsRemoved);

                    foreach (var id in objectsChanged.PageObjectIDsRemoved)
                    {
                        var divisionTemplate = page.GetVerifiedPageObjectInTrashByID(id) as FuzzyFactorCard;
                        if (divisionTemplate != null)
                        {
                            FixOldDivisionTemplateSizing(divisionTemplate);
                        }
                    }

                    page.History.UndoItems.Insert(0, objectsChanged);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectsRemoved to ObjectsOnPageChanged

                #region PageObjectMove to ObjectsMovedChanged

                if (historyItemToUndo is PageObjectMoveBatchHistoryItem)
                {
                    var pageObjectMove = historyItemToUndo as PageObjectMoveBatchHistoryItem;
                    page.History.UndoItems.RemoveFirst();
                    var pageObject = page.GetPageObjectByIDOnPageOrInHistory(pageObjectMove.PageObjectID);
                    if (!pageObjectMove.TravelledPositions.Any() ||
                        string.IsNullOrEmpty(pageObjectMove.PageObjectID) ||
                        pageObject == null)
                    {
                        continue;
                    }

                    var objectsMoved = new ObjectsMovedBatchHistoryItem(pageObjectMove);
                    if (objectsMoved.TravelledPositions.Count == 2 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().X - objectsMoved.TravelledPositions.Last().X) < 0.00001 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().Y - objectsMoved.TravelledPositions.Last().Y) < 0.00001)
                    {
                        continue;
                    }

                    page.History.UndoItems.Insert(0, objectsMoved);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectMove to ObjectsOnPageChanged

                #region PageObjectsMove to ObjectsMovedChanged

                if (historyItemToUndo is PageObjectsMoveBatchHistoryItem)
                {
                    var pageObjectsMove = historyItemToUndo as PageObjectsMoveBatchHistoryItem;
                    page.History.UndoItems.RemoveFirst();
                    var pageObjects = pageObjectsMove.PageObjectIDs.Select(id => pageObjectsMove.ParentPage.GetVerifiedPageObjectOnPageByID(id)).ToList();
                    pageObjects = pageObjects.Where(p => p != null).ToList();
                    if (!pageObjectsMove.TravelledPositions.Any() ||
                        !pageObjectsMove.PageObjectIDs.Any() ||
                        !pageObjects.Any())
                    {
                        continue;
                    }

                    var objectsMoved = new ObjectsMovedBatchHistoryItem(pageObjectsMove);
                    if (objectsMoved.TravelledPositions.Count == 2 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().X - objectsMoved.TravelledPositions.Last().X) < 0.00001 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().Y - objectsMoved.TravelledPositions.Last().Y) < 0.00001)
                    {
                        continue;
                    }
                    page.History.UndoItems.Insert(0, objectsMoved);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectMove to ObjectssOnPageChanged

                #region PageObjectCut fix

                if (historyItemToUndo is PageObjectCutHistoryItem)
                {
                    //BUG: Fix to allow strokes that don't cut any pageObjects.
                    var pageObjectCut = historyItemToUndo as PageObjectCutHistoryItem;
                    if (!string.IsNullOrEmpty(pageObjectCut.CutPageObjectID))
                    {
                        page.History.ConversionUndo();
                        continue;
                    }
                    var cuttingStroke = pageObjectCut.ParentPage.GetVerifiedStrokeInHistoryByID(pageObjectCut.CuttingStrokeID);
                    if (!pageObjectCut.CutPageObjectIDs.Any() ||
                        cuttingStroke == null)
                    {
                        page.History.UndoItems.RemoveFirst();
                        continue;
                    }
                    if (pageObjectCut.CutPageObjectIDs.Count == 1)
                    {
                        pageObjectCut.CutPageObjectID = pageObjectCut.CutPageObjectIDs.First();
                        pageObjectCut.CutPageObjectIDs.Clear();
                        page.History.ConversionUndo();
                        continue;
                    }

                    var newHistoryItems = new List<PageObjectCutHistoryItem>();
                    foreach (var cutPageObjectID in pageObjectCut.CutPageObjectIDs)
                    {
                        var cutPageObject = pageObjectCut.ParentPage.GetVerifiedPageObjectInTrashByID(cutPageObjectID) as ICuttable;
                        if (pageObjectCut.HalvedPageObjectIDs.Count < 2)
                        {
                            continue;
                        }
                        var halvedPageObjectIDs = new List<string>
                                                  {
                                                      pageObjectCut.HalvedPageObjectIDs[0],
                                                      pageObjectCut.HalvedPageObjectIDs[1]
                                                  };
                        pageObjectCut.HalvedPageObjectIDs.RemoveRange(0, 2);
                        if (cutPageObject == null)
                        {
                            continue;
                        }
                        var newCutHistoryItem = new PageObjectCutHistoryItem(pageObjectCut.ParentPage,
                                                                             pageObjectCut.ParentPage.Owner,
                                                                             cuttingStroke,
                                                                             cutPageObject,
                                                                             halvedPageObjectIDs);
                        newHistoryItems.Add(newCutHistoryItem);
                    }

                    page.History.UndoItems.RemoveFirst();

                    foreach (var pageObjectCutHistoryItem in newHistoryItems)
                    {
                        page.History.UndoItems.Insert(0, pageObjectCutHistoryItem);
                    }

                    continue;
                }

                #endregion //PageObjectCut fix

                #region EndPointChangedHistoryItem Adjustments

                if (historyItemToUndo is NumberLineEndPointsChangedHistoryItem)
                {
                    var endPointsChangedHistoryItem = historyItemToUndo as NumberLineEndPointsChangedHistoryItem;
                    var resizeBatchHistoryItem = page.History.RedoItems.FirstOrDefault() as PageObjectResizeBatchHistoryItem;

                    if (resizeBatchHistoryItem != null)
                    {
                        var numberLine = page.GetVerifiedPageObjectOnPageByID(endPointsChangedHistoryItem.NumberLineID) as NumberLine;
                        if (numberLine == null)
                        {
                            page.History.UndoItems.RemoveFirst();
                            continue;
                        }

                        var potentialNumberLineMatch = page.GetVerifiedPageObjectOnPageByID(resizeBatchHistoryItem.PageObjectID) as NumberLine;
                        if (potentialNumberLineMatch == null)
                        {
                            page.History.ConversionUndo();
                            continue;
                        }

                        if (numberLine.ID == potentialNumberLineMatch.ID)
                        {
                            var previousWidth = resizeBatchHistoryItem.StretchedDimensions.First().X;
                            var currentEndPoint = numberLine.NumberLineSize;
                            var previousEndPoint = endPointsChangedHistoryItem.PreviousEndValue;

                            var previousNumberLineWidth = previousWidth - (numberLine.ArrowLength * 2);
                            var previousTickLength = previousNumberLineWidth / previousEndPoint;

                            var preStretchedWidth = previousWidth + (previousTickLength * (currentEndPoint - previousEndPoint));
                            if (Math.Abs(numberLine.Width - preStretchedWidth) < numberLine.TickLength / 2)
                            {
                                preStretchedWidth = numberLine.Width;
                            }
                            endPointsChangedHistoryItem.PreStretchedWidth = preStretchedWidth;
                        }
                    }

                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //EndPointChangedHistoryItem Adjustments

                #region StrokesChanged to ObjectsOnPageChanged

                if (historyItemToUndo is StrokesChangedHistoryItem)
                {
                    var strokesChanged = historyItemToUndo as StrokesChangedHistoryItem;
                    page.History.UndoItems.RemoveFirst();
                    if (!strokesChanged.StrokeIDsAdded.Any() &&
                        !strokesChanged.StrokeIDsRemoved.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryItem(strokesChanged);
                    if (objectsChanged.IsUsingPageObjects)
                    {
                        page.History.UndoItems.Insert(0, objectsChanged);
                        page.History.ConversionUndo(); //?
                    }

                    #region JumpSizeConversion

                    var removedJumpStrokeIDs = new List<string>();
                    var jumpsRemoved = new List<NumberLineJumpSize>();
                    foreach (var strokeID in objectsChanged.StrokeIDsRemoved)
                    {
                        var stroke = page.GetVerifiedStrokeInHistoryByID(strokeID);
                        if (stroke == null)
                        {
                            removedJumpStrokeIDs.Add(strokeID);
                            continue;
                        }

                        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                        {
                            var tickR = numberLine.FindClosestTickToArcStroke(stroke, true);
                            var tickL = numberLine.FindClosestTickToArcStroke(stroke, false);
                            if (tickR == null ||
                                tickL == null ||
                                tickR == tickL)
                            {
                                continue;
                            }

                            removedJumpStrokeIDs.Add(stroke.GetStrokeID());

                            var oldHeight = numberLine.Height;
                            var oldYPosition = numberLine.YPosition;
                            if (numberLine.JumpSizes.Count == 0)
                            {
                                var tallestPoint = stroke.GetBounds().Top;
                                tallestPoint = tallestPoint - 40;

                                if (tallestPoint < 0)
                                {
                                    tallestPoint = 0;
                                }

                                if (tallestPoint > numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight)
                                {
                                    tallestPoint = numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight;
                                }

                                oldHeight += (numberLine.YPosition - tallestPoint);
                                oldYPosition = tallestPoint;
                            }
                            var jumpsChangedHistoryItem = new NumberLineJumpSizesChangedHistoryItem(page,
                                                                                                    page.Owner,
                                                                                                    numberLine.ID,
                                                                                                    new List<Stroke>(),
                                                                                                    new List<Stroke>
                                                                                                    {
                                                                                                        stroke
                                                                                                    },
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    oldHeight,
                                                                                                    oldYPosition,
                                                                                                    numberLine.Height,
                                                                                                    numberLine.YPosition,
                                                                                                    true);

                            page.History.UndoItems.Insert(0, jumpsChangedHistoryItem);
                            page.History.ConversionUndo();
                            break;
                        }
                    }

                    var addedJumpStrokeIDs = new List<string>();
                    foreach (var strokeID in objectsChanged.StrokeIDsAdded)
                    {
                        var stroke = page.GetVerifiedStrokeOnPageByID(strokeID);
                        if (stroke == null)
                        {
                            addedJumpStrokeIDs.Add(strokeID);
                            continue;
                        }

                        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                        {
                            var tickR = numberLine.FindClosestTickToArcStroke(stroke, true);
                            var tickL = numberLine.FindClosestTickToArcStroke(stroke, false);
                            if (tickR == null ||
                                tickL == null ||
                                tickR == tickL)
                            {
                                continue;
                            }

                            addedJumpStrokeIDs.Add(stroke.GetStrokeID());

                            var oldHeight = numberLine.JumpSizes.Count == 1 ? numberLine.NumberLineHeight : numberLine.Height;
                            var oldYPosition = numberLine.JumpSizes.Count == 1 ? numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight : numberLine.YPosition;
                            var jumpsChangedHistoryItem = new NumberLineJumpSizesChangedHistoryItem(page,
                                                                                                    page.Owner,
                                                                                                    numberLine.ID,
                                                                                                    new List<Stroke>
                                                                                                    {
                                                                                                        stroke
                                                                                                    },
                                                                                                    new List<Stroke>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    oldHeight,
                                                                                                    oldYPosition,
                                                                                                    numberLine.Height,
                                                                                                    numberLine.YPosition,
                                                                                                    true);

                            page.History.UndoItems.Insert(0, jumpsChangedHistoryItem);
                            page.History.ConversionUndo();
                            break;
                        }
                    }

                    objectsChanged.StrokeIDsRemoved.RemoveAll(removedJumpStrokeIDs.Contains);
                    objectsChanged.StrokeIDsAdded.RemoveAll(addedJumpStrokeIDs.Contains);

                    #endregion //JumpSizeConversion

                    if (objectsChanged.IsUsingStrokes)
                    {
                        page.History.UndoItems.Insert(0, objectsChanged);
                        page.History.ConversionUndo();
                    }

                    continue;
                }

                #endregion //StrokesChanged to ObjectsOnPageChanged
            }

            page.History.RefreshHistoryIndexes();
            while (page.History.RedoItems.Any())
            {
                page.History.Redo();
            }

            page.History.IsAnimating = false;
        }

        //Clear Authored Histories
        //var undoItemsToRemove = page.History.UndoItems.Where(historyItem => historyItem.OwnerID == Person.Author.ID).ToList();
        //            foreach(var historyItem in undoItemsToRemove)
        //            {
        //                page.History.UndoItems.Remove(historyItem);
        //            }

        //            var redoItemsToRemove = page.History.RedoItems.Where(historyItem => historyItem.OwnerID == Person.Author.ID).ToList();
        //            foreach(var historyItem in redoItemsToRemove)
        //            {
        //                page.History.RedoItems.Remove(historyItem);
        //            }

        //            page.History.OptimizeTrashedItems();

        // Process a console command
        // returns true iff the console should accept another command after this one

        private static Boolean processCommand(string command)
        {
            if (command.Equals("replace"))
            {
                replace();
            }
            else if (command.Equals("interpret student"))
            {
                batchInterpretStudent();
            }
            else if (command.Equals("interpret teacher"))
            {
                batchInterpretTeacher();
            }
            else if (command.Equals("strip"))
            {
                stripHistory();
            }
            else if (command.Equals("combine"))
            {
                NotebookMerge.Combine();
            }
            else if (command.Equals("xml"))
            {
                XMLImporter.Import();
            }
            else if (command.Equals("exit"))
            {
                return false;
            }
            else
            {
                Console.WriteLine("Command not recognized");
            }
            return true;
        }

        private static void replace()
        {
            Console.WriteLine("Starting");

            NotebookMerge.Replace();

            Console.WriteLine("Ended");
        }

        private static void batchInterpretStudent() { BatchInterpreter.InterpretStudentNotebooks(); }

        private static void batchInterpretTeacher() { BatchInterpreter.InterpretTeacherNotebooks(); }

        private static void stripHistory() { StripHistory.StripAll(); }
    }
}