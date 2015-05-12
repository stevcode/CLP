﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CLP.Entities
{
    public static class HistoryAnalysis
    {
        public static void GenerateInitialHistoryActions(CLPPage page)
        {
            //TODO: For Steve: Look into TypeSwitch static class as seen here: http://stackoverflow.com/questions/298976/is-there-a-better-alternative-than-this-to-switch-on-type

            page.History.IsAnimating = true;
            page.History.RefreshHistoryIndexes();

            //Start from beginning of the History, e.g. the original page.
            while (page.History.UndoItems.Any())
            {
                page.History.Undo();
            }

            var pageObjectNumberInHistory = 1;

            //keep track of numberlines throughout history Key<int>:NumberLineSize, Value<int>:Number of Number Lines with that Size, increment on NumberLine creation and when NL size changes
            //Pass into constructors of actions that need identifiers, first converting int value to string (0 = string.Empty, 1 = a, 2 = b, etc)
            var numberLineSizes = new Dictionary<int,int>();

            //same deal as with numberLineSizes above, but Key<string>:array dimensions, in the form of [8x8]
            //increment values on creation, cut, and snap
            var arrayDimensions = new Dictionary<string, int>();

            while (page.History.RedoItems.Any())
            {
                var redoItem = page.History.RedoItems.First();

                var objectChanged = redoItem as ObjectsOnPageChangedHistoryItem;
                if (objectChanged != null)
                {                  
                    if (objectChanged.IsUsingPageObjects &&
                        !objectChanged.IsUsingStrokes)
                    {
                        var isAdd = !objectChanged.PageObjectIDsRemoved.Any() && objectChanged.PageObjectIDsAdded.Any();
                        var objectIdentifier = "";
                        if (isAdd)
                        {
                            var objectAdded = page.GetPageObjectByIDOnPageOrInHistory(objectChanged.PageObjectIDsAdded.First());
                            if (objectAdded.GetType().Name == "CLPArray")
                            {
                                var arr = (CLPArray)objectAdded;
                                var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                                if (arrayDimensions.ContainsKey(dimensions))
                                {
                                    objectIdentifier = ((char)arrayDimensions[dimensions] + 96).ToString();
                                    arrayDimensions[dimensions] += 1;
                                }
                                else
                                {
                                    arrayDimensions[dimensions] = 1;
                                }
                            }
                            else if (objectAdded.GetType().Name == "NumberLine")
                            {
                                var nl = objectAdded as NumberLine;
                                var size = nl.NumberLineSize;
                                if (numberLineSizes.ContainsKey(size))
                                {
                                    objectIdentifier = ((char)numberLineSizes[size] + 96).ToString();
                                    numberLineSizes[size] += 1;
                                }
                                else
                                {
                                    numberLineSizes[size] = 1;
                                }
                            }
                        }
                        else
                        {
                            var objectRemoved = page.GetPageObjectByIDOnPageOrInHistory(objectChanged.PageObjectIDsRemoved.First());
                            if (objectRemoved.GetType().Name == "CLPArray")
                            {
                                var arr = (CLPArray)objectRemoved;
                                var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                                if (arrayDimensions.ContainsKey(dimensions))
                                {
                                    objectIdentifier = ((char)arrayDimensions[dimensions] + 96).ToString();
                                    arrayDimensions[dimensions] -= 1;
                                }
                            }
                            else if (objectRemoved.GetType().Name == "NumberLine")
                            {
                                var nl = (NumberLine)objectRemoved;
                                var size = nl.NumberLineSize;
                                if (numberLineSizes.ContainsKey(size))
                                {
                                    objectIdentifier = ((char)numberLineSizes[size] + 96).ToString();
                                    numberLineSizes[size] -= 1;
                                }
                            }
                        }

                        //get pageObject from ID
                        //use pageObject to generate bounds in the form of a Rect
                        //get history location.
                        var historyAction = new GeneralPageObjectAction(page,
                                                                        new List<IHistoryItem>
                                                                        {
                                                                            objectChanged
                                                                        }, objectIdentifier);
                        page.History.HistoryActions.Add(historyAction);
                        page.History.Redo();
                        pageObjectNumberInHistory++;
                        continue;
                    }

                    if (objectChanged.IsUsingStrokes &&
                        !objectChanged.IsUsingPageObjects)
                    {
                        var inkChangeItems = new List<IHistoryItem>();
                        while (page.History.RedoItems.Any())
                        {
                            var changeItem = page.History.RedoItems.First() as ObjectsOnPageChangedHistoryItem;
                            if (changeItem != null)
                            {
                                if (changeItem.IsUsingStrokes &&
                                    !changeItem.IsUsingPageObjects)
                                {
                                    inkChangeItems.Add(changeItem);
                                    page.History.Redo();
                                    pageObjectNumberInHistory++;
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        var historyAction = new InkAction(page, inkChangeItems);
                        page.History.HistoryActions.Add(historyAction);
                    }
                }

                var objectMoved = redoItem as ObjectsMovedBatchHistoryItem;
                if (objectMoved != null)
                {
                    var objectMovedOnPage = page.GetPageObjectByIDOnPageOrInHistory(objectMoved.PageObjectIDs.First().Key);
                    var objectIdentifier = "";
                    if (objectMovedOnPage.GetType().Name == "CLPArray")
                    {
                        var arr = objectMovedOnPage as CLPArray;
                        var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                        if (arrayDimensions.ContainsKey(dimensions))
                        {
                            if (arrayDimensions[dimensions] > 1)
                            {
                                objectIdentifier = ((char)(arrayDimensions[dimensions] + 95)).ToString();
                            }                           
                        }
                    }
                    else if (objectMovedOnPage.GetType().Name == "NumberLine")
                    {
                        var nl = objectMovedOnPage as NumberLine;
                        var size = nl.NumberLineSize;
                        if (numberLineSizes.ContainsKey(size))
                        {
                            if (numberLineSizes[size] > 1)
                            {
                                objectIdentifier = ((char)(numberLineSizes[size] + 95)).ToString();
                            }
                        }
                    }
                    var historyAction = new GeneralPageObjectAction(page,
                                                                    new List<IHistoryItem>
                                                                    {
                                                                        objectMoved
                                                                    }, objectIdentifier);
                    page.History.HistoryActions.Add(historyAction);
                    page.History.Redo();
                    pageObjectNumberInHistory++;
                    continue;
                }

                var objectResized = redoItem as PageObjectResizeBatchHistoryItem;
                if (objectResized != null)
                {
                    var objectResizedOnPage = page.GetPageObjectByIDOnPageOrInHistory(objectResized.PageObjectID);
                    var objectIdentifier = "";
                    if (objectResizedOnPage.GetType().Name == "CLPArray")
                    {
                        var arr = objectResizedOnPage as CLPArray;
                        var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                        if (arrayDimensions.ContainsKey(dimensions))
                        {
                            if (arrayDimensions[dimensions] > 1)
                            {
                                objectIdentifier = ((char)arrayDimensions[dimensions] + 95).ToString();
                            }
                        }
                    }
                    else if (objectResizedOnPage.GetType().Name == "NumberLine")
                    {
                        var nl = objectResizedOnPage as NumberLine;
                        var size = nl.NumberLineSize;
                        if (numberLineSizes.ContainsKey(size))
                        {
                            if (numberLineSizes[size] > 1)
                            {
                                objectIdentifier = ((char)numberLineSizes[size] + 95).ToString();
                            }
                        }
                    }

                    var historyAction = new GeneralPageObjectAction(page,
                                                                    new List<IHistoryItem>
                                                                    {
                                                                        objectResized
                                                                    }, objectIdentifier);
                    page.History.HistoryActions.Add(historyAction);
                    page.History.Redo();
                    pageObjectNumberInHistory++;
                    continue;
                }

                var objectCut = redoItem as PageObjectCutHistoryItem; //assuming it's an array
                if (objectCut != null)
                {
                    var originalIdentifiers = new List<string>();
                    var newIdentifiers = new List<string>();

                    var cutArray = page.GetPageObjectByIDOnPageOrInHistory(objectCut.CutPageObjectID) as CLPArray;
                    var dimensionsCut = string.Format("{0}x{1}", cutArray.Rows, cutArray.Columns);
                    if (arrayDimensions[dimensionsCut] > 1)
                    {
                        originalIdentifiers.Add(((char)arrayDimensions[dimensionsCut] + 95).ToString());
                    }
                    else
                    {
                        originalIdentifiers.Add("");
                    }
                    arrayDimensions[dimensionsCut] -= 1;

                    page.History.Redo();

                    var halfArrays = objectCut.HalvedPageObjectIDs.Select(h => page.GetPageObjectByIDOnPageOrInHistory(h) as CLPArray).ToList();

                    var halfRows1 = (cutArray.Rows == halfArrays[1].Rows) ? halfArrays[0].Rows : halfArrays[0].Rows - halfArrays[1].Rows;
                    var halfColumns1 = (cutArray.Rows == halfArrays[1].Rows) ? halfArrays[0].Columns - halfArrays[1].Columns : halfArrays[0].Columns;
                    var dimensionsHalf1 = string.Format("{0}x{1}", halfRows1, halfColumns1);
                    if (arrayDimensions.ContainsKey(dimensionsHalf1))
                    {
                        newIdentifiers.Add(((char)(arrayDimensions[dimensionsHalf1] + 96)).ToString());
                        arrayDimensions[dimensionsHalf1] += 1;
                    }
                    else
                    {
                        newIdentifiers.Add("");
                        arrayDimensions[dimensionsHalf1] = 1;
                    }

                    var halfRows2 = halfArrays[1].Rows;
                    var halfColumns2 = halfArrays[1].Columns;
                    var dimensionsHalf2 = string.Format("{0}x{1}", halfRows2, halfColumns2);
                    if (arrayDimensions.ContainsKey(dimensionsHalf2))
                    {
                        newIdentifiers.Add(((char)(arrayDimensions[dimensionsHalf2] + 96)).ToString());                        
                        arrayDimensions[dimensionsHalf2] += 1;
                    }
                    else
                    {
                        newIdentifiers.Add("");
                        arrayDimensions[dimensionsHalf2] = 1;
                    }


                    var arrayAction = new ArrayHistoryAction(page,
                                                             new List<IHistoryItem>
                                                             {
                                                                 objectCut
                                                             }, originalIdentifiers, newIdentifiers);
                    page.History.HistoryActions.Add(arrayAction);
                    
                    pageObjectNumberInHistory++;
                    continue;
                }

                var numberLineEndPointsChanged = redoItem as NumberLineEndPointsChangedHistoryItem;
                if (numberLineEndPointsChanged != null)
                {
                    var numberLineChanged = page.GetPageObjectByIDOnPageOrInHistory(numberLineEndPointsChanged.NumberLineID) as NumberLine;
                    var size = numberLineChanged.NumberLineSize;
                    if (numberLineSizes.ContainsKey(size))
                    {
                        numberLineSizes[size] += 1;
                    }
                    else
                    {
                        numberLineSizes[size] = 1;
                    }

                    var numberLineAction = new NumberLineHistoryAction(page,
                                                                       new List<IHistoryItem>
                                                                       {
                                                                           numberLineEndPointsChanged
                                                                       });
                    page.History.HistoryActions.Add(numberLineAction);
                    page.History.Redo();
                    pageObjectNumberInHistory++;
                    continue;
                }

                var numberLineJumpSizesChanged = redoItem as NumberLineJumpSizesChangedHistoryItem;
                if (numberLineJumpSizesChanged != null)
                {
                    var numberLineIdentifier = "";
                    var nl = page.GetPageObjectByIDOnPageOrInHistory(numberLineJumpSizesChanged.NumberLineID) as NumberLine;
                    var size = nl.NumberLineSize;
                    if (numberLineSizes.ContainsKey(size))
                    {
                        if (numberLineSizes[size] > 1)
                        {
                            numberLineIdentifier = ((char)(numberLineSizes[size] + 95)).ToString();
                        }
                    }
                    var jumpItems = new List<IHistoryItem>();
                    while (page.History.RedoItems.Any())
                    {
                        var jumpItem = page.History.RedoItems.First() as NumberLineJumpSizesChangedHistoryItem;
                        if (jumpItem != null)
                        {
                            jumpItems.Add(jumpItem);
                            page.History.Redo();
                            pageObjectNumberInHistory++;
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    var numberLineAction = new NumberLineHistoryAction(page, jumpItems, numberLineIdentifier);
                    page.History.HistoryActions.Add(numberLineAction);
                }

                var arrayDivisionsChanged = redoItem as CLPArrayDivisionsChangedHistoryItem;
                if (arrayDivisionsChanged != null) { }

                var arrayDivisionValueChanged = redoItem as CLPArrayDivisionValueChangedHistoryItem;
                if (arrayDivisionValueChanged != null) { }

                var arrayGridToggle = redoItem as CLPArrayGridToggleHistoryItem;
                if (arrayGridToggle != null) { }

                var arrayRotate = redoItem as CLPArrayRotateHistoryItem;
                if (arrayRotate != null) { }

                var arraySnap = redoItem as CLPArraySnapHistoryItem;
                if (arraySnap != null)
                {
                    var originalIdentifiers = new List<string>();
                    var newIdentifiers = new List<string> ();
                    var direction = arraySnap.IsHorizontal;
                    var persistingArray = page.GetPageObjectByIDOnPageOrInHistory(arraySnap.PersistingArrayID) as CLPArray;
                    var dimensionsNew = string.Format("{0}x{1}", persistingArray.Rows, persistingArray.Columns);
                    if (arrayDimensions.ContainsKey(dimensionsNew))
                    {
                        if (arrayDimensions[dimensionsNew] > 1)
                        {
                            newIdentifiers.Add(((char)(arrayDimensions[dimensionsNew] + 95)).ToString());
                        }
                        else
                        {
                            newIdentifiers.Add("");
                        }
                        arrayDimensions[dimensionsNew] += 1;
                    }
                    else
                    {
                        newIdentifiers.Add("");
                        arrayDimensions[dimensionsNew] = 1;
                    }
                    var snappedArray = page.GetPageObjectByIDOnPageOrInHistory(arraySnap.SnappedArrayID) as CLPArray;
                    var dimensionsSnap1 = string.Format("{0}x{1}", snappedArray.Rows, snappedArray.Columns);
                    if (arrayDimensions[dimensionsSnap1] > 1)
                    {
                        originalIdentifiers.Add(((char)(arrayDimensions[dimensionsSnap1] + 95)).ToString());
                    }
                    else
                    {
                        originalIdentifiers.Add("");
                    }
                    arrayDimensions[dimensionsSnap1] -= 1;
                    
                    var persistingArrayRows = (direction) ? persistingArray.Rows - snappedArray.Rows : persistingArray.Rows;
                    var persistingArrayColumns = (direction) ? snappedArray.Columns:persistingArray.Columns - snappedArray.Columns;
                    var dimensionsSnap2 = string.Format("{0}x{1}", persistingArrayRows, persistingArrayColumns);
                    if (arrayDimensions[dimensionsSnap2] > 1)
                    {
                        originalIdentifiers.Add(((char)(arrayDimensions[dimensionsSnap2] + 95)).ToString());
                    }
                    else
                    {
                        originalIdentifiers.Add("");
                    }
                    arrayDimensions[dimensionsSnap2] -= 1;
              
                    var arrayAction = new ArrayHistoryAction(page,new List<IHistoryItem>{arraySnap}, originalIdentifiers, newIdentifiers);                   
                    page.History.HistoryActions.Add(arrayAction);
                    page.History.Redo();
                    pageObjectNumberInHistory++;
                    continue;
                }
            }
        }

        public static void AnalyzeHistoryActions(CLPPage page)
        {
            var revisedHistoryActions = new List<IHistoryAction>();
            var isHistoryActiosAltered = false;

            var currentInkGroup = 0;
            var currentObjectsOnPage = new List<IPageObject>();
            
            foreach (var historyAction in page.History.HistoryActions)
            {
                //TODO: Adisa: Do this for the other HistoryActions on John and Jordan's pages that don't change between step 1 and step 2.
                var generalPageObjectAction = historyAction as GeneralPageObjectAction;
                if (generalPageObjectAction != null)
                {
                    if (generalPageObjectAction.GeneralAction == GeneralPageObjectAction.GeneralActions.Add)
                    {
                        currentObjectsOnPage.Add(generalPageObjectAction.AddedPageObjects.First());
                    }
                    else if (generalPageObjectAction.GeneralAction == GeneralPageObjectAction.GeneralActions.Delete)
                    {
                        currentObjectsOnPage.Remove(generalPageObjectAction.RemovedPageObjects.First());
                    }
                    revisedHistoryActions.Add(generalPageObjectAction);
                    continue;
                }
                
                var arrayHistoryAction = historyAction as ArrayHistoryAction;
                if (arrayHistoryAction != null)
                {
                    if (arrayHistoryAction.ArrayAction == ArrayHistoryAction.ArrayActions.Cut)
                    {
                        var arrayCutHistoryItem = arrayHistoryAction.HistoryItems.First() as PageObjectCutHistoryItem;
                        var cutArray = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.CutPageObjectID) as CLPArray;
                        currentObjectsOnPage.Remove(cutArray);
                        var halfArray1 = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.HalvedPageObjectIDs[0]) as CLPArray;
                        var halfArray2 = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.HalvedPageObjectIDs[1]) as CLPArray;
                        currentObjectsOnPage.Add(halfArray1);
                        currentObjectsOnPage.Add(halfArray2);
                    }
                    else if (arrayHistoryAction.ArrayAction == ArrayHistoryAction.ArrayActions.Snap)
                    {
                        var arraySnapHistoryItem = arrayHistoryAction.HistoryItems.First() as CLPArraySnapHistoryItem;
                        var snappedArray = page.GetPageObjectByIDOnPageOrInHistory(arraySnapHistoryItem.SnappedArrayID) as CLPArray;
                        currentObjectsOnPage.Remove(snappedArray);
                    }
                    revisedHistoryActions.Add(arrayHistoryAction);
                    continue;
                }

                var numberLineHistoryAction = historyAction as NumberLineHistoryAction;
                if (numberLineHistoryAction != null)
                {
                    revisedHistoryActions.Add(numberLineHistoryAction);
                    continue;
                }

                var inkAction = historyAction as InkAction;
                if (inkAction != null)
                {
                    var inkHistoryItemIDs = inkAction.HistoryItemIDs;
                    var currentActionType = InkAction.InkActions.Ignore;
                    var currentHistoryItems = new List<IHistoryItem>();
                    var currentLocation = InkAction.InkLocations.None;
                    IPageObject currentNearestObject = null;

                    var idLast = inkHistoryItemIDs.Last();
                    foreach (var id in inkHistoryItemIDs)
                    {
                        var inkHistoryItem = page.History.UndoItems.FirstOrDefault(h => h.ID == id) as ObjectsOnPageChangedHistoryItem;

                        var inkActionType = InkAction.InkActions.Ignore;
                        var inkLocation = InkAction.InkLocations.None;
                        IPageObject nearestObject = null;
                        Stroke inkStroke;
                        if (inkHistoryItem != null)
                        {
                            //type of action
                            if (inkHistoryItem.StrokeIDsAdded.Any() &&
                                !inkHistoryItem.StrokeIDsRemoved.Any())
                            {
                                inkStroke = page.GetStrokeByIDOnPageOrInHistory(inkHistoryItem.StrokeIDsAdded.First());
                                inkActionType = InkAction.InkActions.Add;
                            }
                            else if (!inkHistoryItem.StrokeIDsAdded.Any() &&
                                     inkHistoryItem.StrokeIDsRemoved.Any())
                            {
                                inkStroke = page.GetStrokeByIDOnPageOrInHistory(inkHistoryItem.StrokeIDsRemoved.First());                               
                                inkActionType = InkAction.InkActions.Erase;
                            }
                            else
                            {
                                break; //throw error, neither inks add nor erase
                            }

                            //location
                            if (!currentObjectsOnPage.Any())
                            {
                                inkLocation = InkAction.InkLocations.Over;
                            }
                            else
                            {
                                var inkX1 = inkStroke.GetBounds().X;
                                var inkX2 = inkStroke.GetBounds().X + inkStroke.GetBounds().Width;
                                var inkY1 = inkStroke.GetBounds().Y;
                                var inkY2 = inkStroke.GetBounds().Y + inkStroke.GetBouds().Height;

                                var minDistance = Double.MaxValue;
                                foreach(var pageObject in currentObjectsOnPage)
                                {
                                    var x1 = pageObject.XPosition;
                                    var x2 = pageObject.XPosition + pageObject.Width;
                                    var y1 = pageObject.YPosition;
                                    var y2 = pageObject.YPosition + pageObject.Height;
                                    if ((inkX1 > x1 && inkX2 < x2) &&
                                        (inkY1 > y1 && inkY2 < y2))
                                    {
                                        inkLocation = InkAction.InkLocations.Over;
                                        nearestObject = 
                                        minDistance = 0;
                                        break;
                                    }else if (inkX1 > x2)
                                    {
                                        if (inkX1 - x2 < minDistance)
                                        {
                                            minDistance = inkX1 - x2;
                                            nearestObject = pageObject;
                                            inkLocation = InkAction.InkLocations.Right;
                                        }
                                    }else if (inkX2 < x1)
                                    {
                                        if (x1 - inkX2 < minDistance)
                                        {
                                            minDistance = x1 - inkX2;
                                            nearestObject = pageObject;
                                            inkLocation = InkAction.InkLocations.Left;
                                        }
                                    }else if (inkY1 > y2)
                                    {
                                        if (inkY1 - y2 < minDistance)
                                        {
                                            minDistance = inkY1 - y2;
                                            nearestObject = pageObject;
                                            inkLocation = InkAction.InkLocations.Bottom;
                                        }
                                    }else if (inkY2 < y1)
                                    {
                                        if (y1- inkY2 < minDistance)
                                        {
                                            minDistance = inkY2 - y1;
                                            nearestObject = pageObject;
                                            inkLocation = InkAction.InkLocations.Top;
                                        }
                                    }
                                }
                            }

                            //decide whether to group
                            if (currentActionType == InkAction.InkActions.Ignore || //first item
                                currentLocation == InkAction.InkLocations.None ||
                                currentNearestObject == null)
                            {
                                currentActionType = inkActionType;
                                currentHistoryItems.Add(inkHistoryItem);
                                currentLocation = inkLocation;
                                currentNearestObject = nearestObject;
                            }
                            else if (inkActionType == currentActionType && //same type of action
                                inkLocation == currentLocation &&
                                nearestObject == currentNearestObject)
                            {
                                currentActionType = inkActionType;
                                currentHistoryItems.Add(inkHistoryItem);
                            }
                            else //new group: different action type or location
                            {
                                //make action out of previous group
                                currentInkGroup += 1;
                                var inkGroup = ((char)(currentInkGroup + 64)).ToString();
                                var inkActionSecondPass = new InkAction(page, currentHistoryItems, currentActionType, currentLocation, "array [4x8]", inkGroup);
                                revisedHistoryActions.Add(inkActionSecondPass);

                                //restart grouping
                                currentActionType = inkActionType;
                                currentHistoryItems = new List<IHistoryItem>
                                                      {
                                                          inkHistoryItem
                                                      };
                                currentLocation = inkLocation;
                                currentNearestObject = nearestObject;
                            }

                            if (id == idLast) //make sure last group of strokes
                            {
                                currentInkGroup += 1;
                                var inkGroup = ((char)(currentInkGroup + 64)).ToString();
                                var inkActionSecondPass = new InkAction(page, currentHistoryItems, inkActionType, InkAction.InkLocations.Over, "array [4x8]", inkGroup);
                                revisedHistoryActions.Add(inkActionSecondPass);
                            }
                        }
                    }
                }

                    //TODO: Adisa: This is 2nd pass, so this will always be "INK changed", sub-divide into INK strokes over array, left of array, etc
                    //Keep track of InkGroups you make, each one you make you'll want to increment currentInkGroup++;
                    //Then convert the currentInkGroup from int to string (0 = A, 1 = B, 2 = C, etc), pass that value in to the constructor for the InkAction, then you'll have that for the Ink group ID.
            }

            //Replace historyActions with revisedHistoryActions
            page.History.HistoryActions = revisedHistoryActions;

            //Recursion for multiple passes.
            //if (isHistoryActiosAltered)
            //{
            //    AnalyzeHistoryActions(page);
            //}
        }
    }
}