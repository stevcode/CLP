using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Data;
using CLP.Entities;
using Catel.Runtime.Serialization;
using Path = Catel.IO.Path;

namespace ConsoleScripts
{
    class Program
    {
        static void Main(string[] args)
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

        static void Convert()
        {
            var convertFromFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Convert");
            var convertToFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Converted");
            if(!Directory.Exists(convertToFolderPath))
            {
                Directory.CreateDirectory(convertToFolderPath);
            }

            var notebookFolderPaths = Directory.EnumerateDirectories(convertFromFolderPath);
            foreach(var notebookFolderPath in notebookFolderPaths)
            {
                var filePath = Path.Combine(notebookFolderPath, "notebook.xml");
                var notebook = ModelBase.Load<Notebook>(filePath, SerializationMode.Xml);
                var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
                var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                foreach(var pageFilePath in pageFilePaths)
                {
                    var page = ModelBase.Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                    page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
                    page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);

                    Console.WriteLine("Loaded {3}'s page {0}, differentiation {1}, version {2}", page.PageNumber, page.DifferentiationLevel, page.VersionIndex, page.Owner.FullName);
                    //Do stuff to each page here.

                    //for (int i = 0; i < page.History.UndoItems.Count; i++)
                    //{
                    //    var historyItem = page.History.UndoItems[i];
                    //    if (historyItem is PageObjectMoveBatchHistoryItem)
                    //    {
                    //        page.History.UndoItems[i] = new ObjectsMovedBatchHistoryItem(historyItem as PageObjectMoveBatchHistoryItem);
                    //        continue;
                    //    }

                    //    if (historyItem is PageObjectsMoveBatchHistoryItem)
                    //    {
                    //        page.History.UndoItems[i] = new ObjectsMovedBatchHistoryItem(historyItem as PageObjectsMoveBatchHistoryItem);
                    //        continue;
                    //    }
                    //}

                    //for (int i = 0; i < page.History.RedoItems.Count; i++)
                    //{
                    //    var historyItem = page.History.RedoItems[i];
                    //    if (historyItem is PageObjectMoveBatchHistoryItem)
                    //    {
                    //        page.History.RedoItems[i] = new ObjectsMovedBatchHistoryItem(historyItem as PageObjectMoveBatchHistoryItem);
                    //        continue;
                    //    }

                    //    if (historyItem is PageObjectsMoveBatchHistoryItem)
                    //    {
                    //        page.History.RedoItems[i] = new ObjectsMovedBatchHistoryItem(historyItem as PageObjectsMoveBatchHistoryItem);
                    //        continue;
                    //    }
                    //}

                    //page.History.RefreshHistoryIndexes();

                    HackJumpSizeHistoryItems(page);

                    //Finished doing stuff to page, it'll save below.
                    page.ToXML(pageFilePath, true);
                }
            }
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

                if (nextRedoItem is StrokesChangedHistoryItem)
                {
                    var strokesChanged = nextRedoItem as StrokesChangedHistoryItem;
                    foreach (var removedStroke in strokesChanged.StrokeIDsRemoved.Select(page.History.GetStrokeByID))
                    {
                        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                        {
                            var pageObjectBounds = new Rect(numberLine.XPosition, numberLine.YPosition, numberLine.Width, numberLine.Height);
                            if (removedStroke.HitTest(pageObjectBounds, 5))
                            {
                                continue;
                            }

                            var actuallyRemovedStrokes = new StrokeCollection
                                                         {
                                                             removedStroke
                                                         };
                            var tickR = numberLine.FindClosestTick(actuallyRemovedStrokes, true);
                            var tickL = numberLine.FindClosestTick(actuallyRemovedStrokes, false);
                            if (tickR != null &&
                                tickL != null &&
                                tickR != tickL)
                            {
                                var deletedStartTickValue = tickL.TickValue;
                                var deletedJumpSize = tickR.TickValue - tickL.TickValue;

                                var jumpsToRemove = numberLine.JumpSizes.Where(jump => jump.JumpSize == deletedJumpSize && jump.StartingTickIndex == deletedStartTickValue).ToList();
                                if (!jumpsToRemove.Any())
                                {
                                    Console.WriteLine("JumpsToRemove is empty on page {0}!", page.PageNumber);
                                    continue;
                                }
                                var historyItem = new NumberLineJumpSizesChangedHistoryItem(numberLine.ParentPage,
                                                                                                numberLine.ParentPage.Owner,
                                                                                                numberLine.ID,
                                                                                                new List<NumberLineJumpSize>(),
                                                                                                jumpsToRemove);
                                page.History.UndoItems.Insert(0, historyItem);
                            }
                        }
                    }

                    foreach (var addedStroke in strokesChanged.StrokeIDsAdded.Select(page.GetStrokeByID))
                    {
                        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                        {
                            var pageObjectBounds = new Rect(numberLine.XPosition, numberLine.YPosition, numberLine.Width, numberLine.Height);
                            if (addedStroke.HitTest(pageObjectBounds, 5))
                            {
                                continue;
                            }

                            var actuallyAddedStrokes = new StrokeCollection
                                                         {
                                                             addedStroke
                                                         };
                            var tickR = numberLine.FindClosestTick(actuallyAddedStrokes, true);
                            var tickL = numberLine.FindClosestTick(actuallyAddedStrokes, false);
                            if (tickR != null &&
                                tickL != null &&
                                tickR != tickL)
                            {
                                var addedStartTickValue = tickL.TickValue;
                                var addedJumpSize = tickR.TickValue - tickL.TickValue;

                                var jumpsToAdd = numberLine.JumpSizes.Where(jump => jump.JumpSize == addedJumpSize && jump.StartingTickIndex == addedStartTickValue).ToList();
                                if (!jumpsToAdd.Any())
                                {
                                    Console.WriteLine("JumpsToAdd is empty on page {0}!", page.PageNumber);
                                    continue;
                                }
                                var historyItem = new NumberLineJumpSizesChangedHistoryItem(numberLine.ParentPage,
                                                                                                numberLine.ParentPage.Owner,
                                                                                                numberLine.ID,
                                                                                                jumpsToAdd,
                                                                                                new List<NumberLineJumpSize>());
                                page.History.UndoItems.Insert(0, historyItem);
                            }
                        }
                    }
                }

                
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
        static Boolean processCommand(string command)
        {
            if(command.Equals("replace"))
            {
                replace();
            }
            else if(command.Equals("interpret student"))
            {
                batchInterpretStudent();
            }
            else if(command.Equals("interpret teacher"))
            {
                batchInterpretTeacher();
            }
            else if(command.Equals("strip"))
            {
                stripHistory();
            }
            else if(command.Equals("combine"))
            {
                NotebookMerge.Combine();
            }
            else if(command.Equals("xml"))
            {
                XMLImporter.Import();
            }
            else if(command.Equals("exit"))
            {
                return false;
            }
            else
            {
                Console.WriteLine("Command not recognized");
            }
            return true;
        }

        static void replace()
        {
            Console.WriteLine("Starting");

            NotebookMerge.Replace();

            Console.WriteLine("Ended");
        }

        static void batchInterpretStudent()
        {
            BatchInterpreter.InterpretStudentNotebooks();
        }

        static void batchInterpretTeacher()
        {
            BatchInterpreter.InterpretTeacherNotebooks();
        }

        static void stripHistory()
        {
            StripHistory.StripAll();
        }
    }
}