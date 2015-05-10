using System.Collections.Generic;
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
                    // check what type for all history items
                    // do jordan's and john's first
                    // step 1 only
                    if (objectChanged.IsUsingPageObjects &&
                        !objectChanged.IsUsingStrokes)
                    {
                        var isAdd = !objectChanged.PageObjectIDsRemoved.Any() && objectChanged.PageObjectIDsAdded.Any();

                        //get pageObject from ID
                        //use pageObject to generate bounds in the form of a Rect
                        //get history location.
                        var historyAction = new GeneralPageObjectAction(page,
                                                                        new List<IHistoryItem>
                                                                        {
                                                                            objectChanged
                                                                        });
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
                    var historyAction = new GeneralPageObjectAction(page,
                                                                    new List<IHistoryItem>
                                                                    {
                                                                        objectMoved
                                                                    });
                    page.History.HistoryActions.Add(historyAction);
                    page.History.Redo();
                    pageObjectNumberInHistory++;
                    continue;
                }

                var objectResized = redoItem as PageObjectResizeBatchHistoryItem;
                if (objectResized != null)
                {
                    var historyAction = new GeneralPageObjectAction(page,
                                                                    new List<IHistoryItem>
                                                                    {
                                                                        objectResized
                                                                    });
                    page.History.HistoryActions.Add(historyAction);
                    page.History.Redo();
                    pageObjectNumberInHistory++;
                    continue;
                }

                var objectCut = redoItem as PageObjectCutHistoryItem; //assuming it's an array
                if (objectCut != null)
                {
                    var arrayAction = new ArrayHistoryAction(page,
                                                             new List<IHistoryItem>
                                                             {
                                                                 objectCut
                                                             });
                    page.History.HistoryActions.Add(arrayAction);
                    page.History.Redo();
                    pageObjectNumberInHistory++;
                    continue;
                }

                var numberLineEndPointsChanged = redoItem as NumberLineEndPointsChangedHistoryItem;
                if (numberLineEndPointsChanged != null)
                {
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
                    var numberLineAction = new NumberLineHistoryAction(page, jumpItems);
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
                    var arrayAction = new ArrayHistoryAction(page,
                                                             new List<IHistoryItem>
                                                             {
                                                                 arraySnap
                                                             });
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
            foreach (var historyAction in page.History.HistoryActions)
            {
                //TODO: Adisa: Do this for the other HistoryActions on John and Jordan's pages that don't change between step 1 and step 2.
                var generalPageObjectAction = historyAction as GeneralPageObjectAction;
                if (generalPageObjectAction != null)
                {
                    revisedHistoryActions.Add(generalPageObjectAction);
                    continue;
                }

                var inkAction = historyAction as InkAction;
                if (inkAction != null)
                {
                    //TODO: Adisa: This is 2nd pass, so this will always be "INK changed", sub-divide into INK strokes over array, left of array, etc
                    //Keep track of InkGroups you make, each one you make you'll want to increment currentInkGroup++;
                    //Then convert the currentInkGroup from int to string (0 = A, 1 = B, 2 = C, etc), pass that value in to the constructor for the InkAction, then you'll have that for the Ink group ID.
                }
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