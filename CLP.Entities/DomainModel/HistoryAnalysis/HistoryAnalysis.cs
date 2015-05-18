using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CLP.Entities
{
    public static class HistoryAnalysis
    {
        public class InkGroupKey
        {
            public readonly InkAction.InkLocations Location;
            public readonly string NearestObject;
            public InkGroupKey(InkAction.InkLocations location, string nearestObject)
            {
                Location = location;
                NearestObject = nearestObject;
            }

            /*public override bool Equals(object obj)
            {
                if (!(obj is InkGroupKey)) return false;
                InkGroupKey keyObject = obj as InkGroupKey;
                return (keyObject.Location == Location && keyObject.NearestObject.ID == NearestObject.ID);

            }*/
        }
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

            //Associate number line IDs with letter ID
            var numberLineLetterIDs = new Dictionary<string, string>();

            //same deal as with numberLineSizes above, but Key<string>:array dimensions, in the form of [8x8]
            //increment values on creation, cut, and snap
            var arrayDimensions = new Dictionary<string, int>();

            //Associate array IDs wtih letter ID
            var arrayLetterIDs = new Dictionary<string, string>();

            while (page.History.RedoItems.Any())
            {
                var redoItem = page.History.RedoItems.First();

                var objectChanged = redoItem as ObjectsOnPageChangedHistoryItem;
                if (objectChanged != null) //object added or deleted, strokes added or deleted
                {                  
                    if (objectChanged.IsUsingPageObjects &&
                        !objectChanged.IsUsingStrokes)
                    {
                        var isAdd = !objectChanged.PageObjectIDsRemoved.Any() && objectChanged.PageObjectIDsAdded.Any();
                        var objectIdentifier = "";
                        if (isAdd)
                        {
                            var objectAddedID = objectChanged.PageObjectIDsAdded.First();
                            var objectAdded = page.GetPageObjectByIDOnPageOrInHistory(objectAddedID);
                            
                            if (objectAdded.GetType().Name == "CLPArray")
                            {
                                var arr = objectAdded as CLPArray;
                                var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                                if (arrayDimensions.ContainsKey(dimensions))
                                {
                                    arrayDimensions[dimensions] += 1;
                                    if (arrayDimensions[dimensions] > 1)
                                    {
                                        objectIdentifier = ((char)(arrayDimensions[dimensions] + 95)).ToString();                                        
                                    }
                                }
                                else
                                {
                                    arrayDimensions[dimensions] = 1;
                                }
                                arrayLetterIDs[objectAddedID] = objectIdentifier;
                            }
                            else if (objectAdded.GetType().Name == "NumberLine")
                            {
                                var nl = objectAdded as NumberLine;
                                var size = nl.NumberLineSize;
                                if (numberLineSizes.ContainsKey(size))
                                {
                                    numberLineSizes[size] += 1;
                                    if (numberLineSizes[size] > 1)
                                    {
                                        objectIdentifier = ((char)(numberLineSizes[size] + 96)).ToString();                                        
                                    }
                                }
                                else
                                {
                                    numberLineSizes[size] = 1;
                                }
                                numberLineLetterIDs[objectAddedID] = objectIdentifier;
                            }
                        }
                        else
                        {
                            var objectRemovedID = objectChanged.PageObjectIDsRemoved.First();
                            var objectRemoved = page.GetPageObjectByIDOnPageOrInHistory(objectRemovedID);
                            if (objectRemoved.GetType().Name == "CLPArray")
                            {
                                var arr = objectRemoved as CLPArray;
                                var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                                objectIdentifier = arrayLetterIDs[objectRemovedID];
                                
                                arrayDimensions[dimensions] -= 1;
                                arrayLetterIDs[objectRemovedID] = "No longer on page";
                            }
                            else if (objectRemoved.GetType().Name == "NumberLine")
                            {
                                var nl = objectRemoved as NumberLine;
                                var size = nl.NumberLineSize;
                                objectIdentifier = numberLineLetterIDs[objectRemovedID];
                                
                                numberLineSizes[size] -= 1;
                                numberLineLetterIDs[objectRemovedID] = "No longer on page";
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
                            if (changeItem != null) //iterate through all ink strokes in this group
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
                if (objectMoved != null) //object moved
                {
                    var objectMovedID = objectMoved.PageObjectIDs.First().Key;
                    var objectMovedOnPage = page.GetPageObjectByIDOnPageOrInHistory(objectMovedID);
                    var objectIdentifier = "";
                    if (objectMovedOnPage.GetType().Name == "CLPArray")
                    {
                        var arr = objectMovedOnPage as CLPArray;
                        var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                        objectIdentifier = arrayLetterIDs[objectMovedID];
                    }
                    else if (objectMovedOnPage.GetType().Name == "NumberLine")
                    {
                        var nl = objectMovedOnPage as NumberLine;
                        var size = nl.NumberLineSize;
                        objectIdentifier = numberLineLetterIDs[objectMovedID];
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
                if (objectResized != null) //object resized
                {
                    var objectResizedID = objectResized.PageObjectID;
                    var objectResizedOnPage = page.GetPageObjectByIDOnPageOrInHistory(objectResizedID);
                    var objectIdentifier = "";
                    if (objectResizedOnPage.GetType().Name == "CLPArray")
                    {
                        var arr = objectResizedOnPage as CLPArray;
                        var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                        objectIdentifier = arrayLetterIDs[objectResizedID];
                    }
                    else if (objectResizedOnPage.GetType().Name == "NumberLine")
                    {
                        var nl = objectResizedOnPage as NumberLine;
                        var size = nl.NumberLineSize;
                        objectIdentifier = numberLineLetterIDs[objectResizedID];
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
                if (objectCut != null) //array cut
                {
                    var originalIdentifiers = new List<string>();
                    var newIdentifiers = new List<string>();

                    var cutArrayID = objectCut.CutPageObjectID;
                    var cutArray = page.GetPageObjectByIDOnPageOrInHistory(objectCut.CutPageObjectID) as CLPArray;
                    var dimensionsCut = string.Format("{0}x{1}", cutArray.Rows, cutArray.Columns);
                    originalIdentifiers.Add(arrayLetterIDs[cutArrayID]);
                    arrayDimensions[dimensionsCut] -= 1;
                    arrayLetterIDs[cutArrayID] = "no longer exists";

                    page.History.Redo();

                    var halfArrays = objectCut.HalvedPageObjectIDs.Select(h => page.GetPageObjectByIDOnPageOrInHistory(h) as CLPArray).ToList();

                    //var halfRows1 = (cutArray.Rows == halfArrays[1].Rows) ? halfArrays[0].Rows : halfArrays[0].Rows - halfArrays[1].Rows;
                    //var halfColumns1 = (cutArray.Rows == halfArrays[1].Rows) ? halfArrays[0].Columns - halfArrays[1].Columns : halfArrays[0].Columns;
                    var dimensionsHalf1 = string.Format("{0}x{1}", halfArrays[0].Rows, halfArrays[0].Columns);
                    if (arrayDimensions.ContainsKey(dimensionsHalf1))
                    {
                        var halfID = ((char)(arrayDimensions[dimensionsHalf1] + 96)).ToString();
                        newIdentifiers.Add(halfID);
                        arrayLetterIDs[objectCut.HalvedPageObjectIDs[0]] = halfID;
                        arrayDimensions[dimensionsHalf1] += 1;
                    }
                    else
                    {
                        newIdentifiers.Add("");
                        arrayLetterIDs[objectCut.HalvedPageObjectIDs[0]] = "";
                        arrayDimensions[dimensionsHalf1] = 1;
                    }

                    var halfRows2 = halfArrays[1].Rows;
                    var halfColumns2 = halfArrays[1].Columns;
                    var dimensionsHalf2 = string.Format("{0}x{1}", halfArrays[1].Rows, halfArrays[1].Columns);
                    if (arrayDimensions.ContainsKey(dimensionsHalf2))
                    {
                        var halfID = ((char)(arrayDimensions[dimensionsHalf2] + 96)).ToString();
                        newIdentifiers.Add(halfID);
                        arrayLetterIDs[objectCut.HalvedPageObjectIDs[1]] = halfID;                        
                        arrayDimensions[dimensionsHalf2] += 1;
                    }
                    else
                    {
                        newIdentifiers.Add("");
                        arrayLetterIDs[objectCut.HalvedPageObjectIDs[1]] = "";
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
                if (numberLineEndPointsChanged != null) //number line endpoints changed
                {
                    var numberLineID = numberLineEndPointsChanged.NumberLineID;
                    var numberLineChanged = page.GetPageObjectByIDOnPageOrInHistory(numberLineID) as NumberLine;
                    var size = numberLineChanged.NumberLineSize;
                    var oldSize = numberLineEndPointsChanged.PreviousEndValue - numberLineEndPointsChanged.PreviousStartValue;
                    if (numberLineSizes.ContainsKey(size))
                    {
                        numberLineSizes[size] += 1;
                        if (numberLineSizes[size] > 1)
                        {
                            numberLineLetterIDs[numberLineID] = ((char)(numberLineSizes[size] + 95)).ToString();
                        }
                        else
                        {
                            numberLineLetterIDs[numberLineID] = "";
                        }                   
                    }
                    else
                    {
                        numberLineSizes[size] = 1;
                        numberLineLetterIDs[numberLineID] = "";
                    }
                    numberLineSizes[oldSize] -= 1;

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
                if (numberLineJumpSizesChanged != null) //number line jump sizes changed
                {
                    var numberLineIdentifier = "";
                    var nlID = numberLineJumpSizesChanged.NumberLineID;
                    var nl = page.GetPageObjectByIDOnPageOrInHistory(nlID) as NumberLine;
                    numberLineIdentifier = numberLineLetterIDs[nlID];

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
                    var persistingID = arraySnap.PersistingArrayID;
                    var persistingArray = page.GetPageObjectByIDOnPageOrInHistory(persistingID) as CLPArray;
                    var snappedID = arraySnap.SnappedArrayID;
                    var snappedArray = page.GetPageObjectByIDOnPageOrInHistory(snappedID) as CLPArray;

                    var dimensionsSnap1 = string.Format("{0}x{1}", snappedArray.Rows, snappedArray.Columns);
                    originalIdentifiers.Add(arrayLetterIDs[snappedID]);
                    arrayDimensions[dimensionsSnap1] -= 1;
                    arrayLetterIDs[snappedID] = "No longer on page";
                   
                    var dimensionsSnap2 = string.Format("{0}x{1}", persistingArray.Rows, persistingArray.Columns);
                    originalIdentifiers.Add(arrayLetterIDs[persistingID]);
                    arrayDimensions[dimensionsSnap2] -= 1;

                    var newArrayRows = (direction) ? persistingArray.Rows + snappedArray.Rows : persistingArray.Rows;
                    var newArrayColumns = (direction) ? snappedArray.Columns : persistingArray.Columns + snappedArray.Columns;
                    var dimensionsNew = string.Format("{0}x{1}", newArrayRows, newArrayColumns);
                    var newArrayIdentifier = "";
                    if (arrayDimensions.ContainsKey(dimensionsNew))
                    {
                        arrayDimensions[dimensionsNew] += 1;
                        if (arrayDimensions[dimensionsNew] > 1)
                        {
                            newArrayIdentifier = ((char)(arrayDimensions[dimensionsNew] + 95)).ToString();
                        }                                       
                    }
                    else
                    {
                        arrayDimensions[dimensionsNew] = 1;
                    }
                    newIdentifiers.Add(newArrayIdentifier);
                    arrayLetterIDs[persistingID] = newArrayIdentifier;
              
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
            var inkGroupLetterIDs = new Dictionary<InkGroupKey,string>();
            var currentObjectsOnPage = new List<IPageObject>();

            var numberLineSizes = new Dictionary<int,int>();
            var numberLineLetterIDs = new Dictionary<string, string>();
            var arrayDimensions = new Dictionary<string, int>();
            var arrayLetterIDs = new Dictionary<string, string>();
            
            foreach (var historyAction in page.History.HistoryActions)
            {
                var generalPageObjectAction = historyAction as GeneralPageObjectAction;
                if (generalPageObjectAction != null)
                {
                    if (generalPageObjectAction.GeneralAction == GeneralPageObjectAction.GeneralActions.Add)
                    {
                        var obj = generalPageObjectAction.AddedPageObjects.First();
                        currentObjectsOnPage.Add(obj);
                        if (obj.GetType().Name == "CLPArray")
                        {
                            var arr = obj as CLPArray;
                            var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                            if (arrayDimensions.ContainsKey(dimensions))
                            {
                                arrayDimensions[dimensions] += 1;
                            }
                            else
                            {
                                arrayDimensions[dimensions] = 1;
                            }
                            
                            arrayLetterIDs[arr.ID] = "";
                            if (arrayDimensions[dimensions] > 1)
                            {
                                arrayLetterIDs[arr.ID] = ((char)(arrayDimensions[dimensions] + 95)).ToString();
                            }
                        }
                        else if (obj.GetType().Name == "NumberLine")
                        {
                            var nl = obj as NumberLine;
                            var size = nl.NumberLineSize;
                            if (numberLineSizes.ContainsKey(size))
                            {
                                numberLineSizes[size] += 1;
                            }
                            else
                            {
                                numberLineSizes[size] = 1;
                            }

                            numberLineLetterIDs[nl.ID] = "";
                            if (numberLineSizes[size] > 1)
                            {
                                numberLineLetterIDs[nl.ID] = ((char)(numberLineSizes[size] + 95)).ToString();
                            }
                        }
                    }
                    else if (generalPageObjectAction.GeneralAction == GeneralPageObjectAction.GeneralActions.Delete)
                    {
                        var obj = generalPageObjectAction.RemovedPageObjects.First();
                        currentObjectsOnPage.Remove(obj);
                        if (obj.GetType().Name == "CLPArray")
                        {
                            var arr = obj as CLPArray;
                            var dimensions = string.Format("{0}x{1}", arr.Rows, arr.Columns);
                            if (arrayDimensions.ContainsKey(dimensions))
                            {
                                arrayDimensions[dimensions] -= 1;
                            }
                            
                            arrayLetterIDs[arr.ID] = "";
                            if (arrayDimensions[dimensions] > 1)
                            {
                                arrayLetterIDs[arr.ID] = ((char)(arrayDimensions[dimensions] + 95)).ToString();
                            }
                        }
                        else if (obj.GetType().Name == "NumberLine")
                        {
                            var nl = obj as NumberLine;
                            var size = nl.NumberLineSize;
                            if (numberLineSizes.ContainsKey(size))
                            {
                                numberLineSizes[size] -= 1;
                            }

                            numberLineLetterIDs[nl.ID] = "";
                            if (numberLineSizes[size] > 1)
                            {
                                numberLineLetterIDs[nl.ID] = ((char)(numberLineSizes[size] + 95)).ToString();
                            }    
                        }
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
                        //cut array
                        var cutArray = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.CutPageObjectID) as CLPArray;
                        currentObjectsOnPage.Remove(cutArray);
                        var cutDim = string.Format("{0}x{1}", cutArray.Rows, cutArray.Columns);
                        arrayDimensions[cutDim] -= 1;
                        arrayLetterIDs[cutArray.ID] = "no longer on page";

                        var halfArray1 = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.HalvedPageObjectIDs[0]) as CLPArray;
                        var halfArray2 = page.GetPageObjectByIDOnPageOrInHistory(arrayCutHistoryItem.HalvedPageObjectIDs[1]) as CLPArray;
                        currentObjectsOnPage.Add(halfArray1);
                        currentObjectsOnPage.Add(halfArray2);
                        
                        //half array 1
                        var halfDim1 = string.Format("{0}x{1}", halfArray1.Rows, halfArray1.Columns);
                        if (arrayDimensions.ContainsKey(halfDim1))
                        {
                            arrayDimensions[halfDim1] += 1;
                        }
                        else
                        {
                            arrayDimensions[halfDim1] = 1;
                            
                        }
                        arrayLetterIDs[halfArray1.ID] = "";
                        if (arrayDimensions[halfDim1] > 1)
                        {
                            arrayLetterIDs[halfArray1.ID] = ((char)(arrayDimensions[halfDim1] + 95)).ToString();
                        }

                        //half array 2
                        var halfDim2 = string.Format("{0}x{1}", halfArray2.Rows, halfArray2.Columns);
                        if (arrayDimensions.ContainsKey(halfDim2))
                        {
                            arrayDimensions[halfDim2] += 1;
                        }
                        else
                        {
                            arrayDimensions[halfDim2] = 1;
                            
                        }
                        arrayLetterIDs[halfArray2.ID] = "";
                        if (arrayDimensions[halfDim2] > 1)
                        {
                            arrayLetterIDs[halfArray2.ID] = ((char)(arrayDimensions[halfDim2] + 95)).ToString();
                        }

                        System.Console.WriteLine("Cut");
                        foreach (var i in arrayLetterIDs)
                        {
                            var arr = page.GetPageObjectByIDOnPageOrInHistory(i.Key) as CLPArray;
                            var arrDim = string.Format("Dimensions:{0}x{1}", arr.Rows, arr.Columns);
                            System.Console.WriteLine(arrDim);
                            System.Console.WriteLine("Key:" + i.Key);
                            System.Console.WriteLine("Value:" + i.Value);
                        }
                    }
                    else if (arrayHistoryAction.ArrayAction == ArrayHistoryAction.ArrayActions.Snap)
                    {
                        var arraySnapHistoryItem = arrayHistoryAction.HistoryItems.First() as CLPArraySnapHistoryItem;
                        //snapped array
                        var snappedArray = page.GetPageObjectByIDOnPageOrInHistory(arraySnapHistoryItem.SnappedArrayID) as CLPArray;
                        currentObjectsOnPage.Remove(snappedArray);
                        var dim = string.Format("{0}x{1}", snappedArray.Rows, snappedArray.Columns);
                        if (arrayDimensions.ContainsKey(dim))
                        {
                            arrayDimensions[dim] -= 1;
                        }
                        
                        arrayLetterIDs[snappedArray.ID] = "no longer on page";

                        //persisting array, before snap
                        var persistArray = page.GetPageObjectByIDOnPageOrInHistory(arraySnapHistoryItem.PersistingArrayID) as CLPArray;
                        var direction = arraySnapHistoryItem.IsHorizontal;
                        var removedRows = (direction) ? persistArray.Rows - snappedArray.Rows : persistArray.Rows;
                        var removedColumns = (direction) ? snappedArray.Columns : persistArray.Columns - snappedArray.Columns;
                        dim = string.Format("{0}x{1}", removedRows, removedColumns);
                        arrayDimensions[dim] -= 1;
                        
                        //persisting(large) array
                        dim = string.Format("{0}x{1}", persistArray.Rows, persistArray.Columns);
                        if (arrayDimensions.ContainsKey(dim))
                        {
                            arrayDimensions[dim] += 1;
                        }
                        else
                        {
                            arrayDimensions[dim] = 1;

                        }
                        arrayLetterIDs[persistArray.ID] = "";
                        if (arrayDimensions[dim] > 1)
                        {
                            arrayLetterIDs[persistArray.ID] = ((char)(arrayDimensions[dim] + 95)).ToString();
                        }

                        System.Console.WriteLine("Snap");
                        foreach (var i in arrayLetterIDs)
                        {
                            var arr = page.GetPageObjectByIDOnPageOrInHistory(i.Key) as CLPArray;
                            var arrDim = string.Format("Dimensions:{0}x{1}", arr.Rows, arr.Columns);
                            System.Console.WriteLine(arrDim);
                            System.Console.WriteLine("Key:" + i.Key);
                            System.Console.WriteLine("Value:" + i.Value);
                        }

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
                        System.Windows.Ink.Stroke inkStroke = null;

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

                            //location & nearest object
                            if (!currentObjectsOnPage.Any())
                            {
                                inkLocation = InkAction.InkLocations.Over;
                            }
                            else
                            {
                                var inkX1 = inkStroke.GetBounds().X;
                                var inkX2 = inkStroke.GetBounds().X + inkStroke.GetBounds().Width;
                                var inkY1 = inkStroke.GetBounds().Y;
                                var inkY2 = inkStroke.GetBounds().Y + inkStroke.GetBounds().Height;

                                double minDistance = double.PositiveInfinity;
                                foreach(var pageObject in currentObjectsOnPage)
                                {
                                    var x1 = pageObject.XPosition;
                                    var x2 = pageObject.XPosition + pageObject.Width;
                                    var y1 = pageObject.YPosition;
                                    var y2 = pageObject.YPosition + pageObject.Height;
                                    
                                    //over object
                                    if ((inkX1 > x1 && inkX2 < x2) &&
                                        (inkY1 > y1 && inkY2 < y2))
                                    {
                                        inkLocation = InkAction.InkLocations.Over;
                                        nearestObject = pageObject;
                                        minDistance = 0;
                                        break;
                                    }
                                    
                                    //relative position to object
                                    var minDistanceToObject = double.PositiveInfinity;
                                    var locationToObject = InkAction.InkLocations.None;

                                    //right or left
                                    if (inkX2 > x2) //to right
                                    {
                                        double d = System.Math.Abs(inkX1 - x2);
                                        if (d < minDistanceToObject)
                                        {
                                            minDistanceToObject = d;
                                            locationToObject = InkAction.InkLocations.Right;
                                        }
                                    }else if (inkX1 < x1) //to left
                                    {
                                        double d = System.Math.Abs(inkX2 - x1);
                                        if (d < minDistanceToObject)
                                        {
                                            minDistanceToObject = d;
                                            inkLocation = InkAction.InkLocations.Left;
                                        }
                                    }
                                    
                                    //top or bottom
                                    if (inkY1 > y2)
                                    {
                                        double d = System.Math.Abs(inkY1 - y2);
                                        if (d < minDistanceToObject)
                                        {
                                            minDistanceToObject = d;
                                            inkLocation = InkAction.InkLocations.Bottom;
                                        }
                                    }else if (inkY2 < y1)
                                    {
                                        double d = System.Math.Abs(inkY2 - y1);
                                        if (d < minDistanceToObject)
                                        {
                                            minDistanceToObject = d;
                                            inkLocation = InkAction.InkLocations.Top;
                                        }
                                    }

                                    if (minDistanceToObject < minDistance)
                                    {
                                        minDistance = minDistanceToObject;
                                        inkLocation = locationToObject;
                                        nearestObject = pageObject;
                                    }
                                }
                            }

                            //TESTING
                            currentActionType = inkActionType;
                            currentHistoryItems.Add(inkHistoryItem);
                            currentLocation = inkLocation;
                            currentNearestObject = nearestObject;
                            //group letter
                            var inkGroup = "";
                            var nearestID = "";
                            if (currentNearestObject != null)
                            {
                                nearestID = currentNearestObject.ID;
                            }
                            if (currentActionType == InkAction.InkActions.Add) //add ink
                            {
                                currentInkGroup += 1;
                                inkGroup = ((char)(currentInkGroup + 64)).ToString();
                                var key = new InkGroupKey(currentLocation, nearestID);
                                inkGroupLetterIDs[key] = inkGroup;
                            }
                            else if (currentActionType == InkAction.InkActions.Erase) //erase ink
                            {
                                foreach (var key in inkGroupLetterIDs.Keys)
                                {
                                    if (key.Location == currentLocation &&
                                        key.NearestObject == nearestID)
                                    {
                                        inkGroup = inkGroupLetterIDs[key];
                                        break;
                                    }
                                }
                            }
                                
                            //nearest object
                            var locationDescription = "";
                            var currentObjectId = "";
                            if (currentNearestObject == null)
                            {
                                locationDescription = "page";
                            }
                            else
                            {
                                var nearestObjectName = currentNearestObject.GetType().Name;
                                if (nearestObjectName == "CLPArray")
                                {
                                    var objectID = currentNearestObject.ID;
                                    var array = currentNearestObject as CLPArray;
                                    locationDescription = objectID;
                                    //locationDescription = string.Format("ARR [{0}x{1}{2}]", array.Rows, array.Columns, arrayLetterIDs[objectID]);
                                }
                                else if (nearestObjectName == "NumberLine")
                                {
                                    var objectID = currentNearestObject.ID;
                                    var nl = currentNearestObject as NumberLine;
                                    locationDescription = string.Format("NL [{0}{1}]", nl.NumberLineSize, numberLineLetterIDs[objectID]);
                                }
                                else if (nearestObjectName == "Stamp")
                                {
                                    locationDescription = "STAMP";
                                }
                                else if (nearestObjectName == "StampedObject")
                                {
                                    locationDescription = "STAMP IMAGE";
                                }
                            }
                                
                            var inkActionSecondPass = new InkAction(page, currentHistoryItems, currentActionType, currentLocation, locationDescription, inkGroup);
                            revisedHistoryActions.Add(inkActionSecondPass);

                            //END TESTING

                            //decide whether to group
                            /*if (currentActionType == InkAction.InkActions.Ignore || //first item
                                currentLocation == InkAction.InkLocations.None ||
                                currentNearestObject == null)
                            {
                                currentActionType = inkActionType;
                                currentHistoryItems.Add(inkHistoryItem);
                                currentLocation = inkLocation;
                                currentNearestObject = nearestObject;
                            }
                            else if (!(inkActionType == currentActionType && //different action type or location
                                       inkLocation == currentLocation && nearestObject == currentNearestObject)) //last ink stroke in group
                            {
                                //make action out of previous group

                                //group letter
                                var inkGroup = "";
                                var nearestID = "";
                                if (currentNearestObject != null)
                                {
                                    nearestID = currentNearestObject.ID;
                                }
                                if (currentActionType == InkAction.InkActions.Add) //add ink
                                {
                                    currentInkGroup += 1;
                                    inkGroup = ((char)(currentInkGroup + 64)).ToString();
                                    var key = new InkGroupKey(currentLocation, nearestID);
                                    inkGroupLetterIDs[key] = inkGroup;
                                }
                                else if (currentActionType == InkAction.InkActions.Erase) //erase ink
                                {
                                    foreach (var key in inkGroupLetterIDs.Keys)
                                    {
                                        if (key.Location == currentLocation &&
                                            key.NearestObject == nearestID)
                                        {
                                            inkGroup = inkGroupLetterIDs[key];
                                            break;
                                        }
                                    }
                                }
                                
                                //nearest object
                                var locationDescription = "";
                                var currentObjectId = "";
                                if (currentNearestObject == null)
                                {
                                    locationDescription = "page";
                                }
                                else
                                {
                                    var nearestObjectName = currentNearestObject.GetType().Name;
                                    if (nearestObjectName == "CLPArray")
                                    {
                                        var objectID = currentNearestObject.ID;
                                        var array = currentNearestObject as CLPArray;
                                        locationDescription = objectID;
                                        //locationDescription = string.Format("ARR [{0}x{1}{2}]", array.Rows, array.Columns, arrayLetterIDs[objectID]);
                                    }
                                    else if (nearestObjectName == "NumberLine")
                                    {
                                        var objectID = currentNearestObject.ID;
                                        var nl = currentNearestObject as NumberLine;
                                        locationDescription = string.Format("NL [{0}{1}]", nl.NumberLineSize, numberLineLetterIDs[objectID]);
                                    }
                                    else if (nearestObjectName == "Stamp")
                                    {
                                        locationDescription = "STAMP";
                                    }
                                    else if (nearestObjectName == "StampedObject")
                                    {
                                        locationDescription = "STAMP IMAGE";
                                    }
                                }
                                
                                var inkActionSecondPass = new InkAction(page, currentHistoryItems, currentActionType, currentLocation, locationDescription, inkGroup);
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
                            else //same action type and location, not last in group
                            {
                                currentHistoryItems.Add(inkHistoryItem);
                            }

                            if (id == idLast)
                            {
                                //group letter
                                var inkGroup = "";
                                var nearestID = "";
                                if (currentNearestObject != null)
                                {
                                    nearestID = currentNearestObject.ID;
                                }
                                if (currentActionType == InkAction.InkActions.Add) //add ink
                                {
                                    currentInkGroup += 1;
                                    inkGroup = ((char)(currentInkGroup + 64)).ToString();
                                    var key = new InkGroupKey(currentLocation, nearestID);
                                    inkGroupLetterIDs[key] = inkGroup;
                                }
                                else if (currentActionType == InkAction.InkActions.Erase) //erase ink
                                {
                                    foreach (var key in inkGroupLetterIDs.Keys)
                                    {
                                        if (key.Location == currentLocation &&
                                            key.NearestObject == nearestID)
                                        {
                                            inkGroup = inkGroupLetterIDs[key];
                                            break;
                                        }
                                    }
                                }

                                //nearest object
                                var locationDescription = "";
                                var currentObjectId = "";
                                if (currentNearestObject == null)
                                {
                                    locationDescription = "page";
                                }
                                else
                                {
                                    var nearestObjectName = currentNearestObject.GetType().Name;
                                    if (nearestObjectName == "CLPArray")
                                    {
                                        var objectID = currentNearestObject.ID;
                                        var array = currentNearestObject as CLPArray;
                                        locationDescription = string.Format("ARR [{0}x{1}{2}]", array.Rows, array.Columns, arrayLetterIDs[objectID]);
                                    }
                                    else if (nearestObjectName == "NumberLine")
                                    {
                                        var objectID = currentNearestObject.ID;
                                        var nl = currentNearestObject as NumberLine;
                                        locationDescription = string.Format("NL [{0}{1}]", nl.NumberLineSize, numberLineLetterIDs[objectID]);
                                    }
                                    else if (nearestObjectName == "Stamp")
                                    {
                                        locationDescription = "STAMP";
                                    }
                                    else if (nearestObjectName == "StampedObject")
                                    {
                                        locationDescription = "STAMP IMAGE";
                                    }
                                }

                                var inkActionSecondPass = new InkAction(page, currentHistoryItems, currentActionType, currentLocation, locationDescription, inkGroup);
                                revisedHistoryActions.Add(inkActionSecondPass);
                            }*/
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