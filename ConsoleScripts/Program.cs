using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Ink;
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
        }

        private static void Convert()
        {
            var convertFromFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Convert");
            var convertToFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Converted");
            if (!Directory.Exists(convertToFolderPath))
            {
                Directory.CreateDirectory(convertToFolderPath);
            }

            var notebookFolderPaths = Directory.EnumerateDirectories(convertFromFolderPath);
            foreach (var notebookFolderPath in notebookFolderPaths)
            {
                var filePath = Path.Combine(notebookFolderPath, "notebook.xml");
                var notebook = ModelBase.Load<Notebook>(filePath, SerializationMode.Xml);
                var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
                var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                foreach (var pageFilePath in pageFilePaths)
                {
                    var page = ModelBase.Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                    page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
                    page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);

                    Console.WriteLine("Loaded {3}'s page {0}, differentiation {1}, version {2}", page.PageNumber, page.DifferentiationLevel, page.VersionIndex, page.Owner.FullName);
                    //Do stuff to each page here.                   

                    //ReplaceHistoryItems(page);
                    //HackJumpSizeHistoryItems(page);

                    for (int i = 0; i < page.History.UndoItems.Count; i++)
                    {
                        var historyItem = page.History.UndoItems[i];

                        if (historyItem is NumberLineEndPointsChangedHistoryItem)
                        {
                            if (i + 1 < page.History.UndoItems.Count)
                            {
                                var previousHistoryItem = page.History.UndoItems[i + 1] as PageObjectResizeBatchHistoryItem;
                                if (previousHistoryItem != null)
                                {
                                    if (previousHistoryItem.StretchedDimensions.Count == 2 &&
                                        previousHistoryItem.StretchedDimensions.First() != previousHistoryItem.StretchedDimensions.Last())
                                    {
                                        Console.WriteLine("Found resize BEFORE endpoints changed that actually resizes");
                                    }
                                }
                            }

                            continue;
                        }
                    }

                    page.History.RefreshHistoryIndexes();
                    //Finished doing stuff to page, it'll save below.
                    page.ToXML(pageFilePath, true);
                }
            }
        }

        public static void ReplaceHistoryItems(CLPPage page)
        {
            for (int i = 0; i < page.History.UndoItems.Count; i++)
            {
                var historyItem = page.History.UndoItems[i];

                if (historyItem is StrokesChangedHistoryItem)
                {
                    page.History.UndoItems[i] = new ObjectsOnPageChangedHistoryItem(historyItem as StrokesChangedHistoryItem);
                    continue;
                }

                if (historyItem is PageObjectsAddedHistoryItem)
                {
                    page.History.UndoItems[i] = new ObjectsOnPageChangedHistoryItem(historyItem as PageObjectsAddedHistoryItem);
                    continue;
                }

                if (historyItem is PageObjectsRemovedHistoryItem)
                {
                    page.History.UndoItems[i] = new ObjectsOnPageChangedHistoryItem(historyItem as PageObjectsRemovedHistoryItem);
                    continue;
                }

                if (historyItem is PageObjectMoveBatchHistoryItem)
                {
                    page.History.UndoItems[i] = new ObjectsMovedBatchHistoryItem(historyItem as PageObjectMoveBatchHistoryItem);
                    continue;
                }

                if (historyItem is PageObjectsMoveBatchHistoryItem)
                {
                    page.History.UndoItems[i] = new ObjectsMovedBatchHistoryItem(historyItem as PageObjectsMoveBatchHistoryItem);
                    continue;
                }
            }

            for (int i = 0; i < page.History.RedoItems.Count; i++)
            {
                var historyItem = page.History.RedoItems[i];

                if (historyItem is StrokesChangedHistoryItem)
                {
                    page.History.RedoItems[i] = new ObjectsOnPageChangedHistoryItem(historyItem as StrokesChangedHistoryItem);
                    continue;
                }

                if (historyItem is PageObjectsAddedHistoryItem)
                {
                    page.History.RedoItems[i] = new ObjectsOnPageChangedHistoryItem(historyItem as PageObjectsAddedHistoryItem);
                    continue;
                }

                if (historyItem is PageObjectsRemovedHistoryItem)
                {
                    page.History.RedoItems[i] = new ObjectsOnPageChangedHistoryItem(historyItem as PageObjectsRemovedHistoryItem);
                    continue;
                }

                if (historyItem is PageObjectMoveBatchHistoryItem)
                {
                    page.History.RedoItems[i] = new ObjectsMovedBatchHistoryItem(historyItem as PageObjectMoveBatchHistoryItem);
                    continue;
                }

                if (historyItem is PageObjectsMoveBatchHistoryItem)
                {
                    page.History.RedoItems[i] = new ObjectsMovedBatchHistoryItem(historyItem as PageObjectsMoveBatchHistoryItem);
                    continue;
                }
            }
        }

        public static void AdjustNumberLineEndPointChangedHistoryItem(CLPPage page)
        {
            //Rewind entire page
            page.History.IsAnimating = true;

            while (page.History.UndoItems.Any())
            {
                var historyItemToUndo = page.History.UndoItems.FirstOrDefault();
                if (historyItemToUndo == null)
                {
                    break;
                }

                #region EndPointChangedHistoryItem Adjustments

                var endPointsChangedHistoryItem = historyItemToUndo as NumberLineEndPointsChangedHistoryItem;
                var previousUndoHistoryItem = page.History.RedoItems.FirstOrDefault();
                if (endPointsChangedHistoryItem != null &&
                    previousUndoHistoryItem != null)
                {
                    var resizeBatchHistoryItem = previousUndoHistoryItem as PageObjectResizeBatchHistoryItem;
                    if (resizeBatchHistoryItem != null)
                    {
                        var numberLine = page.GetVerifiedPageObjectOnPageByID(endPointsChangedHistoryItem.NumberLineID) as NumberLine;
                        if (numberLine == null)
                        {
                            Console.WriteLine("ERROR: Number Line not on page in NumberLineEndPointsChangedHistoryItem on History Index {0}.",
                                                  endPointsChangedHistoryItem.HistoryIndex);
                            continue;
                        }

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

                #endregion //EndPointChangedHistoryItem Adjustments

                #region JumpSizeHistoryItem Conversion

                var strokesChangedHistoryItem = historyItemToUndo as ObjectsOnPageChangedHistoryItem;
                if (strokesChangedHistoryItem != null)
                {
                    if (strokesChangedHistoryItem.IsUsingStrokes &&
                        !strokesChangedHistoryItem.IsUsingPageObjects)
                    {
                        var removedJumpStrokeIDs = new List<string>();
                        foreach (var stroke in strokesChangedHistoryItem.StrokeIDsRemoved.Select(page.GetVerifiedStrokeInHistoryByID))
                        {
                            if (stroke == null)
                            {
                                Console.WriteLine("ERROR: Null stroke in StrokeIDsRemoved in ObjectsOnPageChangedHistoryItem on History Index {0}.",
                                                  strokesChangedHistoryItem.HistoryIndex);
                                continue;
                            }

                            foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                            {
                                var tickR = numberLine.FindClosestTick(stroke, true);
                                var tickL = numberLine.FindClosestTick(stroke, false);
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
                                                                                                        oldHeight,
                                                                                                        oldYPosition);

                                page.History.UndoItems.Insert(0, jumpsChangedHistoryItem);
                                break;
                            }
                        }

                        var addedJumpStrokeIDs = new List<string>();
                        foreach (var stroke in strokesChangedHistoryItem.StrokeIDsAdded.Select(page.GetVerifiedStrokeOnPageByID))
                        {
                            if (stroke == null)
                            {
                                Console.WriteLine("ERROR: Null stroke in StrokeIDsAdded in ObjectsOnPageChangedHistoryItem on History Index {0}.",
                                                  strokesChangedHistoryItem.HistoryIndex);
                                continue;
                            }

                            foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                            {
                                var tickR = numberLine.FindClosestTick(stroke, true);
                                var tickL = numberLine.FindClosestTick(stroke, false);
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
                                                                                                        oldHeight,
                                                                                                        oldYPosition);

                                page.History.UndoItems.Insert(0, jumpsChangedHistoryItem);
                                break;
                            }
                        }

                        strokesChangedHistoryItem.StrokeIDsRemoved.RemoveAll(s => removedJumpStrokeIDs.Contains(s));
                        strokesChangedHistoryItem.StrokeIDsAdded.RemoveAll(s => addedJumpStrokeIDs.Contains(s));

                        if (!strokesChangedHistoryItem.IsUsingStrokes)
                        {
                            page.History.UndoItems.Remove(strokesChangedHistoryItem);
                        }
                    }
                }

                #endregion //JumpSizeHistoryItem Conversion

                page.History.Undo();
            }

            while (page.History.RedoItems.Any())
            {
                page.History.Redo();
            }

            page.History.IsAnimating = false;
        }

        public static void HackJumpSizeHistoryItems(CLPPage page)
        {
            //Rewind entire page
            page.History.IsAnimating = true;

            while (page.History.UndoItems.Any())
            {
                page.History.Undo();
            }

            while (page.History.RedoItems.Any() &&
                   page.History.IsAnimating)
            {
                var nextRedoItem = page.History.RedoItems.FirstOrDefault();
                if (nextRedoItem == null)
                {
                    return;
                }

                page.History.Redo();

                //if (nextRedoItem is StrokesChangedHistoryItem)
                //{
                //    var strokesChanged = nextRedoItem as StrokesChangedHistoryItem;
                //    foreach (var removedStroke in strokesChanged.StrokeIDsRemoved.Select(page.History.GetStrokeByID))
                //    {
                //        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                //        {
                //            var pageObjectBounds = new Rect(numberLine.XPosition, numberLine.YPosition, numberLine.Width, numberLine.Height);
                //            if (removedStroke.HitTest(pageObjectBounds, 5))
                //            {
                //                continue;
                //            }

                //            var actuallyRemovedStrokes = new StrokeCollection
                //                                         {
                //                                             removedStroke
                //                                         };
                //            var tickR = numberLine.FindClosestTick(actuallyRemovedStrokes, true);
                //            var tickL = numberLine.FindClosestTick(actuallyRemovedStrokes, false);
                //            if (tickR != null &&
                //                tickL != null &&
                //                tickR != tickL)
                //            {
                //                var deletedStartTickValue = tickL.TickValue;
                //                var deletedJumpSize = tickR.TickValue - tickL.TickValue;

                //                var jumpsToRemove = numberLine.JumpSizes.Where(jump => jump.JumpSize == deletedJumpSize && jump.StartingTickIndex == deletedStartTickValue).ToList();
                //                if (!jumpsToRemove.Any())
                //                {
                //                    Console.WriteLine("JumpsToRemove is empty on page {0}!", page.PageNumber);
                //                    continue;
                //                }
                //                var historyItem = new NumberLineJumpSizesChangedHistoryItem(numberLine.ParentPage,
                //                                                                                numberLine.ParentPage.Owner,
                //                                                                                numberLine.ID,
                //                                                                                new List<NumberLineJumpSize>(),
                //                                                                                jumpsToRemove);
                //                page.History.UndoItems.Insert(0, historyItem);
                //            }
                //        }
                //    }

                //    foreach (var addedStroke in strokesChanged.StrokeIDsAdded.Select(page.GetStrokeByID))
                //    {
                //        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                //        {
                //            var pageObjectBounds = new Rect(numberLine.XPosition, numberLine.YPosition, numberLine.Width, numberLine.Height);
                //            if (addedStroke.HitTest(pageObjectBounds, 5))
                //            {
                //                continue;
                //            }

                //            var actuallyAddedStrokes = new StrokeCollection
                //                                         {
                //                                             addedStroke
                //                                         };
                //            var tickR = numberLine.FindClosestTick(actuallyAddedStrokes, true);
                //            var tickL = numberLine.FindClosestTick(actuallyAddedStrokes, false);
                //            if (tickR != null &&
                //                tickL != null &&
                //                tickR != tickL)
                //            {
                //                var addedStartTickValue = tickL.TickValue;
                //                var addedJumpSize = tickR.TickValue - tickL.TickValue;

                //                var jumpsToAdd = numberLine.JumpSizes.Where(jump => jump.JumpSize == addedJumpSize && jump.StartingTickIndex == addedStartTickValue).ToList();
                //                if (!jumpsToAdd.Any())
                //                {
                //                    Console.WriteLine("JumpsToAdd is empty on page {0}!", page.PageNumber);
                //                    continue;
                //                }
                //                var historyItem = new NumberLineJumpSizesChangedHistoryItem(numberLine.ParentPage,
                //                                                                                numberLine.ParentPage.Owner,
                //                                                                                numberLine.ID,
                //                                                                                jumpsToAdd,
                //                                                                                new List<NumberLineJumpSize>());
                //                page.History.UndoItems.Insert(0, historyItem);
                //            }
                //        }
                //    }
                //}
            }
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