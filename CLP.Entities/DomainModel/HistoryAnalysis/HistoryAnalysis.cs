using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Ink;

namespace CLP.Entities
{
    public static class HistoryAnalysis
    {
        public static void GenerateHistoryActions(CLPPage page)
        {
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "1"
                                            });
            var initialHistoryActions = GenerateInitialHistoryActions(page);
            page.History.HistoryActions.AddRange(initialHistoryActions);

            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "HistoryActions");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, PageNameComposite.ParsePage(page).ToFileName() + ".txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");
            File.AppendAllText(filePath, "PASS [1]"+ "\n");
            foreach (var item in initialHistoryActions)
            {
                var semi = item == initialHistoryActions.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "2"
                                            });
            var refinedInkHistoryActions = RefineInkHistoryActions(page, initialHistoryActions);
            page.History.HistoryActions.AddRange(refinedInkHistoryActions);


        }

        #region First Pass: Initialization

        public static List<IHistoryAction> GenerateInitialHistoryActions(CLPPage page)
        {
            var historyItemBuffer = new List<IHistoryItem>();
            var initialHistoryActions = new List<IHistoryAction>();
            var historyItems = page.History.CompleteOrderedHistoryItems;

            for (var i = 0; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                historyItemBuffer.Add(currentHistoryItem);
                if (historyItemBuffer.Count == 1)
                {
                    var singleHistoryAction = VerifyAndGenerateSingleItemAction(page, historyItemBuffer.First());
                    if (singleHistoryAction != null)
                    {
                        initialHistoryActions.Add(singleHistoryAction);
                        historyItemBuffer.Clear();
                        continue;
                    }
                }

                var nextHistoryItem = i + 1 < historyItems.Count ? historyItems[i + 1] : null;
                var compoundHistoryAction = VerifyAndGenerateCompoundItemAction(page, historyItemBuffer, nextHistoryItem);
                if (compoundHistoryAction != null)
                {
                    initialHistoryActions.Add(compoundHistoryAction);
                    historyItemBuffer.Clear();
                }
            }

            return initialHistoryActions;
        }

        public static IHistoryAction VerifyAndGenerateSingleItemAction(CLPPage page, IHistoryItem historyItem)
        {
            if (historyItem == null)
            {
                return null;
            }

            IHistoryAction historyAction = null;
            TypeSwitch.On(historyItem)
                      .Case<ObjectsOnPageChangedHistoryItem>(h =>
                      {
                          historyAction = ObjectCodedActions.Add(page, h) ?? ObjectCodedActions.Delete(page, h);
                      })
                      .Case<CLPArrayRotateHistoryItem>(h => { historyAction = ArrayCodedActions.Rotate(page, h); })
                      .Case<PageObjectCutHistoryItem>(h => { historyAction = ArrayCodedActions.Cut(page, h); })
                      .Case<CLPArraySnapHistoryItem>(h => { historyAction = ArrayCodedActions.Snap(page, h); })
                      .Case<CLPArrayDivisionsChangedHistoryItem>(h => { historyAction = ArrayCodedActions.Divide(page, h); });

            return historyAction;
        }

        public static IHistoryAction VerifyAndGenerateCompoundItemAction(CLPPage page, List<IHistoryItem> historyItems, IHistoryItem nextHistoryItem)
        {
            if (!historyItems.Any())
            {
                return null;
            }

            if (historyItems.All(h => h is ObjectsOnPageChangedHistoryItem))
            {
                var objectsChangedHistoryItems = historyItems.Cast<ObjectsOnPageChangedHistoryItem>().ToList();
                // TODO: Edge case that recognizes multiple bins added at once.

                if (objectsChangedHistoryItems.All(h => h.IsUsingStrokes && !h.IsUsingPageObjects))
                {
                    var nextObjectsChangedHistoryItem = nextHistoryItem as ObjectsOnPageChangedHistoryItem;
                    if (nextObjectsChangedHistoryItem != null &&
                        nextObjectsChangedHistoryItem.IsUsingStrokes &&
                        !nextObjectsChangedHistoryItem.IsUsingPageObjects)
                    {
                        return null;
                    }

                    var historyAction = InkCodedActions.ChangeOrIgnore(page, objectsChangedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is ObjectsMovedBatchHistoryItem))
            {
                var objectsMovedHistoryItems = historyItems.Cast<ObjectsMovedBatchHistoryItem>().ToList();

                var firstIDSequence = objectsMovedHistoryItems.First().PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList();
                if (objectsMovedHistoryItems.All(h => firstIDSequence.SequenceEqual(h.PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList())))
                {
                    var nextMovedHistoryItem = nextHistoryItem as ObjectsMovedBatchHistoryItem;
                    if (nextMovedHistoryItem != null &&
                        firstIDSequence.SequenceEqual(nextMovedHistoryItem.PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList()))
                    {
                        return null;
                    }

                    var historyAction = ObjectCodedActions.Move(page, objectsMovedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is NumberLineEndPointsChangedHistoryItem))
            {
                var endPointsChangedHistoryItems = historyItems.Cast<NumberLineEndPointsChangedHistoryItem>().ToList();

                var firstNumberLineID = endPointsChangedHistoryItems.First().NumberLineID;
                if (endPointsChangedHistoryItems.All(h => h.NumberLineID == firstNumberLineID))
                {
                    var nextEndPointsChangedHistoryItem = nextHistoryItem as NumberLineEndPointsChangedHistoryItem;
                    if (nextEndPointsChangedHistoryItem != null &&
                        nextEndPointsChangedHistoryItem.NumberLineID == firstNumberLineID)
                    {
                        return null;
                    }

                    var historyAction = NumberLineCodedActions.EndPointsChange(page, endPointsChangedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is NumberLineJumpSizesChangedHistoryItem))
            {
                var jumpSizesChangedHistoryItems = historyItems.Cast<NumberLineJumpSizesChangedHistoryItem>().ToList();

                var firstNumberLineID = jumpSizesChangedHistoryItems.First().NumberLineID;
                if (jumpSizesChangedHistoryItems.All(h => h.NumberLineID == firstNumberLineID))
                {
                    var nextEndPointsChangedHistoryItem = nextHistoryItem as NumberLineJumpSizesChangedHistoryItem;
                    if (nextEndPointsChangedHistoryItem != null &&
                        nextEndPointsChangedHistoryItem.NumberLineID == firstNumberLineID)
                    {
                        return null;
                    }

                    var historyAction = NumberLineCodedActions.JumpSizesChange(page, jumpSizesChangedHistoryItems);
                    return historyAction;
                }
            }

            return null;
        }

        #endregion // First Pass: Initialization

        #region Second Pass: Ink Refinement

        public static List<IHistoryAction> RefineInkHistoryActions(CLPPage page, List<IHistoryAction> historyActions)
        {
            var refinedHistoryActions = new List<IHistoryAction>();

            foreach (var historyAction in historyActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK &&
                    historyAction.CodedObjectAction == Codings.ACTION_INK_CHANGE)
                {
                    var refinedInkActions = ProcessInkChangeHistoryAction(page, historyAction);
                    refinedHistoryActions.AddRange(refinedInkActions);
                }
                else
                {
                    refinedHistoryActions.Add(historyAction);
                }
            }

            return refinedHistoryActions;
        }

        public static List<IHistoryAction> ProcessInkChangeHistoryAction(CLPPage page, IHistoryAction historyAction)
        {
            var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
            var processedInkActions = new List<IHistoryAction>();
            var pageObjectsOnPage = ObjectCodedActions.GetPageObjectsOnPageAtHistoryIndex(page, historyItems.First().HistoryIndex);
            // TODO: validation

            var historyItemBuffer = new List<IHistoryItem>();
            IPageObject currentPageObjectReference = null;
            var currentLocationReference = Codings.ACTIONID_INK_LOCATION_NONE;
            var isInkAdd = true;
            for (var i = 0; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                historyItemBuffer.Add(currentHistoryItem);
                if (historyItemBuffer.Count == 1)
                {
                    var strokes = currentHistoryItem.StrokesAdded;
                    if (!strokes.Any())
                    {
                        strokes = currentHistoryItem.StrokesRemoved;
                        isInkAdd = false;
                    }

                    // TODO: If strokes.count != 1, deal with point erase
                    // TODO: Validation
                    var stroke = strokes.First();
                    var strokeBounds = InkCodedActions.GetStrokeBoundsAtHistoryIndex(stroke, currentHistoryItem.HistoryIndex);
                    currentPageObjectReference = pageObjectsOnPage.FirstOrDefault(p =>
                                                                                  {
                                                                                      var pageObjectBounds = ObjectCodedActions.GetPageObjectBoundsAtHistoryIndex(page,
                                                                                                                                                                  p,
                                                                                                                                                                  currentHistoryItem.HistoryIndex);
                                                                                      return APageObjectBase.IsBoundsOverlappingByPercentage(pageObjectBounds, strokeBounds, 0.80);
                                                                                  }); // TODO: make this pick the one it's most overlapping
                }

                var nextHistoryItem = i + 1 < historyItems.Count ? historyItems[i + 1] : null;
                if (nextHistoryItem != null)
                {
                    var nextStrokes = nextHistoryItem.StrokesAdded;

                    if (!nextStrokes.Any())
                    {
                        nextStrokes = nextHistoryItem.StrokesRemoved;
                    }
                    var isNextInkPartOfCurrent = isInkAdd == nextHistoryItem.StrokesAdded.Any() && isInkAdd == !nextHistoryItem.StrokesRemoved.Any();

                    var nextStroke = nextStrokes.First();
                    var nextStrokeBounds = InkCodedActions.GetStrokeBoundsAtHistoryIndex(nextStroke, nextHistoryItem.HistoryIndex);
                    var nextPageObjectReference = pageObjectsOnPage.FirstOrDefault(p =>
                                                                                   {
                                                                                       var pageObjectBounds = ObjectCodedActions.GetPageObjectBoundsAtHistoryIndex(page,
                                                                                                                                                                   p,
                                                                                                                                                                   nextHistoryItem.HistoryIndex);
                                                                                       return APageObjectBase.IsBoundsOverlappingByPercentage(pageObjectBounds, nextStrokeBounds, 0.80);
                                                                                   }); // TODO: make this pick the one it's most overlapping
                    var isNextPageObjectReferencePartOfCurrent = nextPageObjectReference.ID == currentPageObjectReference.ID;

                    //grouping is either over pageObject or not, if not, once NEXT is over a pageObject, take all previous and find the pageObject their weighted difference is closest to
                    //use that for location assignment. can even try clustering them all if not directly over.
                }

                var compoundHistoryAction = VerifyAndGenerateCompoundItemAction(page, historyItemBuffer, nextHistoryItem);
                if (compoundHistoryAction != null)
                {
                    processedInkActions.Add(compoundHistoryAction);
                    historyItemBuffer.Clear();
                }
            }


            return processedInkActions;
        } 

        #endregion // Second Pass: Ink Refinement

        // 3rd pass: ink interpretation: ink divider, skip counting, grouping by inking, ignoring?

            // 4th pass: simple pattern interpretations

            // 5th pass: complex pattern interpretations

            // 6th pass: Tag generation

        public static void AnalyzeHistoryActions(CLPPage page)
        {
            //var revisedHistoryActions = new List<IHistoryAction>();
            //var isHistoryActiosAltered = false;

            //var currentInkGroup = 0;
            //var inkGroupLetterIDs = new Dictionary<InkGroupKey, string>();
            //var currentObjectsOnPage = new List<IPageObject>();

            //var numberLineSizes = new Dictionary<int, int>();
            //var numberLineLetterIDs = new Dictionary<string, string>();
            //var arrayDimensions = new Dictionary<string, int>();
            //var arrayLetterIDs = new Dictionary<string, string>();

            //foreach (var historyAction in page.History.HistoryActions)
            //{
            //    var generalPageObjectAction = historyAction as GeneralPageObjectAction;
            //    if (generalPageObjectAction != null)
            //    {
            //        if (generalPageObjectAction.GeneralAction == GeneralPageObjectAction.GeneralActions.Add)
            //        {
            //            var obj = generalPageObjectAction.AddedPageObjects.First();
            //            currentObjectsOnPage.Add(obj);
            //            if (obj.GetType().Name == "CLPArray")
            //            {
            //                var arr = obj as CLPArray;
            //                var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
            //                if (arrayDimensions.ContainsKey(dimensions))
            //                {
            //                    arrayDimensions[dimensions] += 1;
            //                }
            //                else
            //                {
            //                    arrayDimensions[dimensions] = 1;
            //                }

            //                arrayLetterIDs[arr.ID] = "";
            //                if (arrayDimensions[dimensions] > 1)
            //                {
            //                    arrayLetterIDs[arr.ID] = ((char)(arrayDimensions[dimensions] + 95)).ToString();
            //                }
            //            }
            //            else if (obj.GetType().Name == "NumberLine")
            //            {
            //                var nl = obj as NumberLine;
            //                var size = nl.NumberLineSize;
            //                if (numberLineSizes.ContainsKey(size))
            //                {
            //                    numberLineSizes[size] += 1;
            //                }
            //                else
            //                {
            //                    numberLineSizes[size] = 1;
            //                }

            //                numberLineLetterIDs[nl.ID] = "";
            //                if (numberLineSizes[size] > 1)
            //                {
            //                    numberLineLetterIDs[nl.ID] = ((char)(numberLineSizes[size] + 95)).ToString();
            //                }
            //            }
            //        }
            //        else if (generalPageObjectAction.GeneralAction == GeneralPageObjectAction.GeneralActions.Delete)
            //        {
            //            var obj = generalPageObjectAction.RemovedPageObjects.First();
            //            currentObjectsOnPage.Remove(obj);
            //            if (obj.GetType().Name == "CLPArray")
            //            {
            //                var arr = obj as CLPArray;
            //                var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
            //                if (arrayDimensions.ContainsKey(dimensions))
            //                {
            //                    arrayDimensions[dimensions] -= 1;
            //                }

            //                arrayLetterIDs[arr.ID] = "";
            //                if (arrayDimensions[dimensions] > 1)
            //                {
            //                    arrayLetterIDs[arr.ID] = ((char)(arrayDimensions[dimensions] + 95)).ToString();
            //                }
            //            }
            //            else if (obj.GetType().Name == "NumberLine")
            //            {
            //                var nl = obj as NumberLine;
            //                var size = nl.NumberLineSize;
            //                if (numberLineSizes.ContainsKey(size))
            //                {
            //                    numberLineSizes[size] -= 1;
            //                }

            //                numberLineLetterIDs[nl.ID] = "";
            //                if (numberLineSizes[size] > 1)
            //                {
            //                    numberLineLetterIDs[nl.ID] = ((char)(numberLineSizes[size] + 95)).ToString();
            //                }
            //            }
            //        }
            //        else if (generalPageObjectAction.GeneralAction == GeneralPageObjectAction.GeneralActions.Move)
            //        {
            //            var obj = generalPageObjectAction.MovedPageObjects.First();
            //            if (obj.GetType().Name == "CLPArray")
            //            {
            //                Console.WriteLine("Array moved");
            //                var moveHistoryItem = generalPageObjectAction.HistoryItems.OfType<ObjectsMovedBatchHistoryItem>().FirstOrDefault();
            //                foreach (var p in moveHistoryItem.TravelledPositions)
            //                {
            //                    Console.WriteLine(string.Format("x:{0}, y:{1}", p.X, p.Y));
            //                }
            //                Console.WriteLine(string.Format("width:{0}, height:{0}", obj.Width, obj.Height));
            //                Console.WriteLine("\n");
            //            }
            //        }
            //        revisedHistoryActions.Add(generalPageObjectAction);
            //        continue;
            //    }

            //    var arrayHistoryAction = historyAction as ArrayHistoryAction;
            //    if (arrayHistoryAction != null)
            //    {
            //        if (arrayHistoryAction.ArrayAction == ArrayHistoryAction.ArrayActions.Cut)
            //        {
            //            var arrayCutHistoryItem = arrayHistoryAction.HistoryItems.First() as PageObjectCutHistoryItem;
            //            //cut array
            //            var cutArray = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.CutPageObjectID) as CLPArray;
            //            currentObjectsOnPage.Remove(cutArray);
            //            var cutDim = string.Format("{0}x{1}", cutArray.Rows, cutArray.Columns);
            //            arrayDimensions[cutDim] -= 1;
            //            arrayLetterIDs[cutArray.ID] = "no longer on page";

            //            var halfArray1 = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.HalvedPageObjectIDs[0]) as CLPArray;
            //            var halfArray2 = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.HalvedPageObjectIDs[1]) as CLPArray;
            //            currentObjectsOnPage.Add(halfArray1);
            //            currentObjectsOnPage.Add(halfArray2);

            //            //half array 1
            //            var halfDim1 = string.Format("{0}x{1}", halfArray1.Rows, halfArray1.Columns);
            //            if (halfArray1.Rows == cutArray.Rows && //some buggy cutting
            //                halfArray1.Columns == cutArray.Columns)
            //            {
            //                var rows1 = (halfArray2.Columns == cutArray.Columns) ? cutArray.Rows - halfArray2.Rows : cutArray.Rows;
            //                var columns1 = (halfArray2.Columns == cutArray.Columns) ? cutArray.Columns : cutArray.Columns = halfArray2.Columns;
            //                halfDim1 = string.Format("{0}x{1}", rows1, columns1);
            //            }
            //            if (arrayDimensions.ContainsKey(halfDim1))
            //            {
            //                arrayDimensions[halfDim1] += 1;
            //            }
            //            else
            //            {
            //                arrayDimensions[halfDim1] = 1;
            //            }
            //            arrayLetterIDs[halfArray1.ID] = "";
            //            if (arrayDimensions[halfDim1] > 1)
            //            {
            //                arrayLetterIDs[halfArray1.ID] = ((char)(arrayDimensions[halfDim1] + 95)).ToString();
            //            }

            //            //half array 2
            //            var halfDim2 = string.Format("{0}x{1}", halfArray2.Rows, halfArray2.Columns);
            //            if (arrayDimensions.ContainsKey(halfDim2))
            //            {
            //                arrayDimensions[halfDim2] += 1;
            //            }
            //            else
            //            {
            //                arrayDimensions[halfDim2] = 1;
            //            }
            //            arrayLetterIDs[halfArray2.ID] = "";
            //            if (arrayDimensions[halfDim2] > 1)
            //            {
            //                arrayLetterIDs[halfArray2.ID] = ((char)(arrayDimensions[halfDim2] + 95)).ToString();
            //            }
            //            Console.WriteLine("Cut");
            //        }
            //        else if (arrayHistoryAction.ArrayAction == ArrayHistoryAction.ArrayActions.Snap)
            //        {
            //            var arraySnapHistoryItem = arrayHistoryAction.HistoryItems.First() as CLPArraySnapHistoryItem;
            //            //snapped array
            //            var snappedArray = page.GetPageObjectByIDOnPageOrInHistory(arraySnapHistoryItem.SnappedArrayID) as CLPArray;
            //            currentObjectsOnPage.Remove(snappedArray);
            //            var dim = string.Format("{0}x{1}", snappedArray.Rows, snappedArray.Columns);
            //            if (arrayDimensions.ContainsKey(dim))
            //            {
            //                arrayDimensions[dim] -= 1;
            //            }
            //            arrayLetterIDs[snappedArray.ID] = "no longer on page";

            //            //persisting array, before snap
            //            var persistArray = page.GetPageObjectByIDOnPageOrInHistory(arraySnapHistoryItem.PersistingArrayID) as CLPArray;
            //            var direction = arraySnapHistoryItem.IsHorizontal;
            //            var removedRows = (direction) ? persistArray.Rows - snappedArray.Rows : persistArray.Rows;
            //            var removedColumns = (direction) ? snappedArray.Columns : persistArray.Columns - snappedArray.Columns;
            //            dim = string.Format("{0}x{1}", removedRows, removedColumns);
            //            arrayDimensions[dim] -= 1;

            //            //persisting(large) array
            //            dim = string.Format("{0}x{1}", persistArray.Rows, persistArray.Columns);
            //            if (arrayDimensions.ContainsKey(dim))
            //            {
            //                arrayDimensions[dim] += 1;
            //            }
            //            else
            //            {
            //                arrayDimensions[dim] = 1;
            //            }
            //            arrayLetterIDs[persistArray.ID] = "";
            //            if (arrayDimensions[dim] > 1)
            //            {
            //                arrayLetterIDs[persistArray.ID] = ((char)(arrayDimensions[dim] + 95)).ToString();
            //            }
            //        }
            //        else if (arrayHistoryAction.ArrayAction == ArrayHistoryAction.ArrayActions.Divide)
            //        {
            //            Console.WriteLine("Divide");
            //        }
            //        else if (arrayHistoryAction.ArrayAction == ArrayHistoryAction.ArrayActions.InkDivide)
            //        {
            //            Console.WriteLine("InkDivide");
            //        }
            //        else if (arrayHistoryAction.ArrayAction == ArrayHistoryAction.ArrayActions.Rotate)
            //        {
            //            Console.WriteLine("Rotate");
            //        }
            //        revisedHistoryActions.Add(arrayHistoryAction);
            //        continue;
            //    }

            //    var numberLineHistoryAction = historyAction as NumberLineHistoryAction;
            //    if (numberLineHistoryAction != null)
            //    {
            //        revisedHistoryActions.Add(numberLineHistoryAction);
            //        continue;
            //    }

            //    var inkAction = historyAction as InkAction;
            //    if (inkAction != null)
            //    {
            //        //for keeping track of object positions
            //        var currentHistoryActionID = historyAction.ID;
            //        var historyActionsAfterNow = new List<IHistoryAction>();
            //        var actionReached = false;
            //        foreach (var action in page.History.HistoryActions)
            //        {
            //            if (action.ID == currentHistoryActionID)
            //            {
            //                actionReached = true;
            //                continue;
            //            }

            //            if (actionReached)
            //            {
            //                historyActionsAfterNow.Add(action);
            //            }
            //        }

            //        var moveHistoryActionsAfterNow =
            //            historyActionsAfterNow.OfType<GeneralPageObjectAction>().Where(h => h.GeneralAction == GeneralPageObjectAction.GeneralActions.Move).ToList();
            //        var resizeHistoryActionsAfterNow =
            //            historyActionsAfterNow.OfType<GeneralPageObjectAction>().Where(h => h.GeneralAction == GeneralPageObjectAction.GeneralActions.Resize).ToList();
            //        var arraySnapHistoryActionsAfterNow =
            //            historyActionsAfterNow.OfType<ArrayHistoryAction>().Where(h => h.ArrayAction == ArrayHistoryAction.ArrayActions.Snap).ToList();

            //        var inkHistoryItemIDs = inkAction.HistoryItemIDs;

            //        //iterating through inkHistoryItems
            //        var currentActionType = InkAction.InkActions.Ignore;
            //        var currentHistoryItems = new List<IHistoryItem>();
            //        var currentLocation = InkAction.InkLocations.None;
            //        IPageObject currentNearestObject = null;
            //        var changedArrayDescription = "";

            //        var idLast = inkHistoryItemIDs.Last();
            //        foreach (var id in inkHistoryItemIDs)
            //        {
            //            var inkHistoryItem = page.History.UndoItems.FirstOrDefault(h => h.ID == id) as ObjectsOnPageChangedHistoryItem;

            //            var inkActionType = InkAction.InkActions.Ignore;
            //            var inkLocation = InkAction.InkLocations.None;
            //            IPageObject nearestObject = null;
            //            Stroke inkStroke = null;

            //            if (inkHistoryItem != null)
            //            {
            //                //type of action
            //                if (inkHistoryItem.StrokeIDsAdded.Any() &&
            //                    !inkHistoryItem.StrokeIDsRemoved.Any())
            //                {
            //                    inkStroke = page.GetStrokeByIDOnPageOrInHistory(inkHistoryItem.StrokeIDsAdded.First());
            //                    inkActionType = InkAction.InkActions.Add;
            //                }
            //                else if (!inkHistoryItem.StrokeIDsAdded.Any() &&
            //                         inkHistoryItem.StrokeIDsRemoved.Any())
            //                {
            //                    inkStroke = page.GetStrokeByIDOnPageOrInHistory(inkHistoryItem.StrokeIDsRemoved.First());
            //                    inkActionType = InkAction.InkActions.Erase;
            //                }
            //                else
            //                {
            //                    break; //throw error, neither inks add nor erase
            //                }

            //                //ignore small ink strokes
            //                if (inkStroke.GetBounds().Width < 1.0 ||
            //                    inkStroke.GetBounds().Height < 1.0)
            //                {
            //                    continue;
            //                }

            //                //location & nearest object
            //                if (!currentObjectsOnPage.Any())
            //                {
            //                    inkLocation = InkAction.InkLocations.Over;
            //                }
            //                else
            //                {
            //                    var inkX1 = inkStroke.GetBounds().X;
            //                    var inkX2 = inkStroke.GetBounds().X + inkStroke.GetBounds().Width;
            //                    var inkY1 = inkStroke.GetBounds().Y;
            //                    var inkY2 = inkStroke.GetBounds().Y + inkStroke.GetBounds().Height;
            //                    Console.WriteLine(string.Format("X1:{0} X2:{1} Y1:{2} Y2:{3}", inkX1, inkX2, inkY1, inkY2));

            //                    double minDistance = double.PositiveInfinity;
            //                    foreach (var pageObject in currentObjectsOnPage)
            //                    {
            //                        var x1 = pageObject.XPosition;
            //                        var y1 = pageObject.YPosition;

            //                        var oldPersistingArrayDimensions = "";

            //                        foreach (var pageObjectAction in moveHistoryActionsAfterNow)
            //                        {
            //                            var moveHistoryItem = pageObjectAction.HistoryItems.OfType<ObjectsMovedBatchHistoryItem>().FirstOrDefault();
            //                            if (moveHistoryItem != null)
            //                            {
            //                                if (moveHistoryItem.PageObjectIDs.First().Key == pageObject.ID)
            //                                {
            //                                    x1 = moveHistoryItem.TravelledPositions.First().X;
            //                                    y1 = moveHistoryItem.TravelledPositions.First().Y;
            //                                    break;
            //                                }
            //                            }
            //                        }

            //                        var x2 = x1 + pageObject.Width;
            //                        var y2 = y1 + pageObject.Height;
            //                        if (pageObject.GetType().Name == "CLPArray")
            //                        {
            //                            var arr = pageObject as CLPArray;
            //                            x1 += arr.LabelLength;
            //                            y1 += arr.LabelLength;
            //                            x2 -= arr.LabelLength;
            //                            y2 -= arr.LabelLength;
            //                        }
            //                        foreach (var pageObjectAction in arraySnapHistoryActionsAfterNow)
            //                        {
            //                            var snapHistoryItem = pageObjectAction.HistoryItems.OfType<CLPArraySnapHistoryItem>().FirstOrDefault();
            //                            if (snapHistoryItem != null)
            //                            {
            //                                var persistArray = page.GetPageObjectByIDOnPageOrInHistory(snapHistoryItem.PersistingArrayID) as CLPArray;
            //                                if (persistArray.ID == pageObject.ID)
            //                                {
            //                                    Console.WriteLine(persistArray.GridSquareSize);
            //                                    var snapArray = page.GetPageObjectByIDOnPageOrInHistory(snapHistoryItem.SnappedArrayID) as CLPArray;
            //                                    var rows = (persistArray.Columns == snapArray.Columns) ? persistArray.Rows - snapArray.Rows : persistArray.Rows;
            //                                    var cols = (persistArray.Columns == snapArray.Columns) ? persistArray.Columns : persistArray.Columns - snapArray.Columns;
            //                                    x2 = x1 + cols * persistArray.GridSquareSize;
            //                                    y2 = y1 + rows * persistArray.GridSquareSize;
            //                                    oldPersistingArrayDimensions = string.Format("{0}x{1}", rows, cols);
            //                                    break;
            //                                }
            //                                /*else if (snapArray.ID == pageObject.ID)
            //                                {
            //                                    x2 = x1 + snapArray.Columns * snapArray.GridSquareSize;
            //                                    y2 = y1 + snapArray.Rows * snapArray.GridSquareSize;
            //                                }*/
            //                            }
            //                        }

            //                        foreach (var pageObjectAction in resizeHistoryActionsAfterNow)
            //                        {
            //                            var resizeHistoryItem = pageObjectAction.HistoryItems.OfType<PageObjectResizeBatchHistoryItem>().FirstOrDefault();
            //                            if (resizeHistoryItem != null)
            //                            {
            //                                if (resizeHistoryItem.PageObjectID == pageObject.ID)
            //                                {
            //                                    x2 = x1 + resizeHistoryItem.OriginalWidth;
            //                                    y2 = y1 + resizeHistoryItem.OriginalHeight;
            //                                    if (pageObject.GetType().Name == "CLPArray")
            //                                    {
            //                                        var arr = pageObject as CLPArray;
            //                                        x2 -= 2 * arr.LabelLength;
            //                                        y2 -= 2 * arr.LabelLength;
            //                                    }
            //                                    break;
            //                                }
            //                            }
            //                        }

            //                        //over object
            //                        if ((inkX1 > x1 && inkX2 < x2) &&
            //                            (inkY1 > y1 && inkY2 < y2))
            //                        {
            //                            inkLocation = InkAction.InkLocations.Over;
            //                            nearestObject = pageObject;
            //                            changedArrayDescription = oldPersistingArrayDimensions;
            //                            minDistance = 0;

            //                            Console.WriteLine(inkActionType.ToString().ToLower());
            //                            Console.WriteLine(nearestObject.ID);
            //                            Console.WriteLine("over");
            //                            Console.WriteLine(string.Format("X1:{0} X2:{1} Y1:{2} Y2:{3}", x1, x2, y1, y2));
            //                            Console.WriteLine("\n");
            //                            break;
            //                        }

            //                        //relative position to object
            //                        var minDistanceToObject = double.PositiveInfinity;
            //                        var locationToObject = InkAction.InkLocations.None;

            //                        //right or left
            //                        if (inkX2 > x2) //to right
            //                        {
            //                            double d = Math.Abs(inkX1 - x2);
            //                            if (d < minDistanceToObject)
            //                            {
            //                                minDistanceToObject = d;
            //                                locationToObject = InkAction.InkLocations.Right;
            //                            }
            //                        }
            //                        else if (inkX1 < x1) //to left
            //                        {
            //                            double d = Math.Abs(inkX2 - x1);
            //                            if (d < minDistanceToObject)
            //                            {
            //                                minDistanceToObject = d;
            //                                locationToObject = InkAction.InkLocations.Left;
            //                            }
            //                        }

            //                        //top or bottom
            //                        if (inkY1 > y2)
            //                        {
            //                            double d = Math.Abs(inkY1 - y2);
            //                            if (d < minDistanceToObject)
            //                            {
            //                                minDistanceToObject = d;
            //                                locationToObject = InkAction.InkLocations.Below;
            //                            }
            //                        }
            //                        else if (inkY2 < y1)
            //                        {
            //                            double d = Math.Abs(inkY2 - y1);
            //                            if (d < minDistanceToObject)
            //                            {
            //                                minDistanceToObject = d;
            //                                locationToObject = InkAction.InkLocations.Above;
            //                            }
            //                        }

            //                        if (minDistanceToObject < minDistance)
            //                        {
            //                            minDistance = minDistanceToObject;
            //                            inkLocation = locationToObject;
            //                            nearestObject = pageObject;
            //                            changedArrayDescription = oldPersistingArrayDimensions;

            //                            Console.WriteLine(inkActionType.ToString().ToLower());
            //                            Console.WriteLine(nearestObject.ID);
            //                            Console.WriteLine(inkLocation.ToString().ToLower());
            //                            Console.WriteLine(string.Format("X1:{0} X2:{1} Y1:{2} Y2:{3}", x1, x2, y1, y2));
            //                            Console.WriteLine("\n");
            //                        }
            //                    }
            //                }

            //                //TESTING
            //                /*currentActionType = inkActionType;
            //                currentHistoryItems.Add(inkHistoryItem);
            //                currentLocation = inkLocation;
            //                currentNearestObject = nearestObject;
            //                //group letter
            //                var inkGroup = "";
            //                var nearestID = "";
            //                if (currentNearestObject != null)
            //                {
            //                    nearestID = currentNearestObject.ID;
            //                }
            //                if (currentActionType == InkAction.InkActions.Add) //add ink
            //                {
            //                    currentInkGroup += 1;
            //                    inkGroup = ((char)(currentInkGroup + 64)).ToString();
            //                    var key = new InkGroupKey(currentLocation, nearestID);
            //                    inkGroupLetterIDs[key] = inkGroup;
            //                }
            //                else if (currentActionType == InkAction.InkActions.Erase) //erase ink
            //                {
            //                    foreach (var key in inkGroupLetterIDs.Keys)
            //                    {
            //                        if (key.Location == currentLocation &&
            //                            key.NearestObject == nearestID)
            //                        {
            //                            inkGroup = inkGroupLetterIDs[key];
            //                            break;
            //                        }
            //                    }
            //                }
                                
            //                //nearest object
            //                var locationDescription = "";
            //                var currentObjectId = "";
            //                if (currentNearestObject == null)
            //                {
            //                    locationDescription = "page";
            //                }
            //                else
            //                {
            //                    var nearestObjectName = currentNearestObject.GetType().Name;
            //                    if (nearestObjectName == "CLPArray")
            //                    {
            //                        var objectID = currentNearestObject.ID;
            //                        var array = currentNearestObject as CLPArray;
            //                        locationDescription = objectID;
            //                        //locationDescription = string.Format("ARR [{0}x{1}{2}]", array.Rows, array.Columns, arrayLetterIDs[objectID]);
            //                    }
            //                    else if (nearestObjectName == "NumberLine")
            //                    {
            //                        var objectID = currentNearestObject.ID;
            //                        var nl = currentNearestObject as NumberLine;
            //                        locationDescription = string.Format("NL [{0}{1}]", nl.NumberLineSize, numberLineLetterIDs[objectID]);
            //                    }
            //                    else if (nearestObjectName == "Stamp")
            //                    {
            //                        locationDescription = "STAMP";
            //                    }
            //                    else if (nearestObjectName == "StampedObject")
            //                    {
            //                        locationDescription = "STAMP IMAGE";
            //                    }
            //                }
                                
            //                var inkActionSecondPass = new InkAction(page, currentHistoryItems, currentActionType, currentLocation, locationDescription, inkGroup);
            //                revisedHistoryActions.Add(inkActionSecondPass);*/

            //                //END TESTING

            //                //decide whether to group
            //                if (currentActionType == InkAction.InkActions.Ignore || //first item
            //                    currentLocation == InkAction.InkLocations.None)
            //                {
            //                    currentActionType = inkActionType;
            //                    currentHistoryItems.Add(inkHistoryItem);
            //                    currentLocation = inkLocation;
            //                    currentNearestObject = nearestObject;
            //                }
            //                else if (!(inkActionType == currentActionType && //different action type or location
            //                           inkLocation == currentLocation && nearestObject == currentNearestObject)) //last ink stroke in group
            //                {
            //                    //make action out of previous group

            //                    //group letter
            //                    var inkGroup = "";
            //                    var nearestID = "";
            //                    if (currentNearestObject != null)
            //                    {
            //                        nearestID = currentNearestObject.ID;
            //                    }
            //                    if (currentActionType == InkAction.InkActions.Add) //add ink
            //                    {
            //                        currentInkGroup += 1;
            //                        inkGroup = ((char)(currentInkGroup + 64)).ToString();
            //                        var key = new InkGroupKey(currentLocation, nearestID);
            //                        inkGroupLetterIDs[key] = inkGroup;
            //                    }
            //                    else if (currentActionType == InkAction.InkActions.Erase) //erase ink
            //                    {
            //                        foreach (var key in inkGroupLetterIDs.Keys)
            //                        {
            //                            if (key.Location == currentLocation &&
            //                                key.NearestObject == nearestID)
            //                            {
            //                                inkGroup = inkGroupLetterIDs[key];
            //                                break;
            //                            }
            //                        }
            //                    }

            //                    //nearest object
            //                    var locationDescription = "";
            //                    if (currentNearestObject == null)
            //                    {
            //                        locationDescription = "page";
            //                    }
            //                    else
            //                    {
            //                        var nearestObjectName = currentNearestObject.GetType().Name;
            //                        if (nearestObjectName == "CLPArray")
            //                        {
            //                            var objectID = currentNearestObject.ID;
            //                            var array = currentNearestObject as CLPArray;
            //                            locationDescription = string.Format("ARR [{0}x{1}{2}]", array.Rows, array.Columns, arrayLetterIDs[objectID]);
            //                            if (changedArrayDescription != "")
            //                            {
            //                                locationDescription = string.Format("ARR [{0}{1}]", changedArrayDescription, arrayLetterIDs[objectID]);
            //                            }
            //                        }
            //                        else if (nearestObjectName == "NumberLine")
            //                        {
            //                            var objectID = currentNearestObject.ID;
            //                            var nl = currentNearestObject as NumberLine;
            //                            locationDescription = string.Format("NL [{0}{1}]", nl.NumberLineSize, numberLineLetterIDs[objectID]);
            //                        }
            //                        else if (nearestObjectName == "Stamp")
            //                        {
            //                            locationDescription = "STAMP";
            //                        }
            //                        else if (nearestObjectName == "StampedObject")
            //                        {
            //                            locationDescription = "STAMP IMAGE";
            //                        }
            //                    }

            //                    var inkActionSecondPass = new InkAction(page, currentHistoryItems, currentActionType, currentLocation, locationDescription, inkGroup);
            //                    revisedHistoryActions.Add(inkActionSecondPass);

            //                    //restart grouping
            //                    currentActionType = inkActionType;
            //                    currentHistoryItems = new List<IHistoryItem>
            //                                          {
            //                                              inkHistoryItem
            //                                          };
            //                    currentLocation = inkLocation;
            //                    currentNearestObject = nearestObject;
            //                    changedArrayDescription = "";
            //                }
            //                else //same action type and location, not last in group
            //                {
            //                    currentHistoryItems.Add(inkHistoryItem);
            //                }

            //                if (id == idLast)
            //                {
            //                    //group letter
            //                    var inkGroup = "";
            //                    var nearestID = "";
            //                    if (currentNearestObject != null)
            //                    {
            //                        nearestID = currentNearestObject.ID;
            //                    }
            //                    if (currentActionType == InkAction.InkActions.Add) //add ink
            //                    {
            //                        currentInkGroup += 1;
            //                        inkGroup = ((char)(currentInkGroup + 64)).ToString();
            //                        var key = new InkGroupKey(currentLocation, nearestID);
            //                        inkGroupLetterIDs[key] = inkGroup;
            //                    }
            //                    else if (currentActionType == InkAction.InkActions.Erase) //erase ink
            //                    {
            //                        foreach (var key in inkGroupLetterIDs.Keys)
            //                        {
            //                            if (key.Location == currentLocation &&
            //                                key.NearestObject == nearestID)
            //                            {
            //                                inkGroup = inkGroupLetterIDs[key];
            //                                break;
            //                            }
            //                        }
            //                    }

            //                    //nearest object
            //                    var locationDescription = "";
            //                    if (currentNearestObject == null)
            //                    {
            //                        locationDescription = "page";
            //                    }
            //                    else
            //                    {
            //                        var nearestObjectName = currentNearestObject.GetType().Name;
            //                        if (nearestObjectName == "CLPArray")
            //                        {
            //                            var objectID = currentNearestObject.ID;
            //                            var array = currentNearestObject as CLPArray;
            //                            locationDescription = string.Format("ARR [{0}x{1}{2}]", array.Rows, array.Columns, arrayLetterIDs[objectID]);
            //                            if (changedArrayDescription != "")
            //                            {
            //                                locationDescription = string.Format("ARR [{0}{1}]", changedArrayDescription, arrayLetterIDs[objectID]);
            //                            }
            //                        }
            //                        else if (nearestObjectName == "NumberLine")
            //                        {
            //                            var objectID = currentNearestObject.ID;
            //                            var nl = currentNearestObject as NumberLine;
            //                            locationDescription = string.Format("NL [{0}{1}]", nl.NumberLineSize, numberLineLetterIDs[objectID]);
            //                        }
            //                        else if (nearestObjectName == "Stamp")
            //                        {
            //                            locationDescription = "STAMP";
            //                        }
            //                        else if (nearestObjectName == "StampedObject")
            //                        {
            //                            locationDescription = "STAMP IMAGE";
            //                        }
            //                    }

            //                    var inkActionSecondPass = new InkAction(page, currentHistoryItems, currentActionType, currentLocation, locationDescription, inkGroup);
            //                    revisedHistoryActions.Add(inkActionSecondPass);
            //                }
            //            }
            //        }
            //    }

            //    //TODO: Adisa: This is 2nd pass, so this will always be "INK changed", sub-divide into INK strokes over array, left of array, etc
            //    //Keep track of InkGroups you make, each one you make you'll want to increment currentInkGroup++;
            //    //Then convert the currentInkGroup from int to string (0 = A, 1 = B, 2 = C, etc), pass that value in to the constructor for the InkAction, then you'll have that for the Ink group ID.
            //}

            ////Replace historyActions with revisedHistoryActions
            //page.History.HistoryActions = revisedHistoryActions;

            ////Recursion for multiple passes.
            ////if (isHistoryActiosAltered)
            ////{
            ////    AnalyzeHistoryActions(page);
            ////}
        }
    }
}