﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Ink;
using Catel.Collections;

namespace CLP.Entities
{
    // Derived from: http://www.johndcook.com/blog/standard_deviation/
    public class RollingStandardDeviation
    {
        private int _numberOfDataValues;
        private double _oldMean;
        private double _newMean;
        private double _oldSum;
        private double _newSum;

        public RollingStandardDeviation() { _numberOfDataValues = 0; }

        public double Mean { get { return _numberOfDataValues > 0 ? _newMean : 0.0; } }

        public double Variance {  get { return _numberOfDataValues > 1 ? _newSum / (_numberOfDataValues - 1) : 0.0; } }

        public double StandardDeviation { get { return Math.Sqrt(Variance); } }

        public void Clear() { _numberOfDataValues = 0; }

        public void Update(double x)
        {
            _numberOfDataValues++;

            if (_numberOfDataValues == 1)
            {
                _oldMean = x;
                _newMean = x;
                _oldSum = 0.0;
                return;
            }

            _newMean = _oldMean + (x - _oldMean) / _numberOfDataValues;
            _newSum = _oldSum + (x - _oldMean) * (x - _oldMean);

            _oldMean = _newMean;
            _oldSum = _newSum;
        }
    }

    public static class HistoryAnalysis
    {
        public static void GenerateHistoryActions(CLPPage page)
        {
            HistoryAction.CurrentIncrementID.Clear();
            HistoryAction.MaxIncrementID.Clear();

            // First Pass
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

            // Second Pass
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "2"
                                            });
            var refinedInkHistoryActions = RefineInkHistoryActions(page, initialHistoryActions);
            page.History.HistoryActions.AddRange(refinedInkHistoryActions);

            File.AppendAllText(filePath, "PASS [2]" + "\n");
            foreach (var item in refinedInkHistoryActions)
            {
                var semi = item == refinedInkHistoryActions.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Third Pass
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
            {
                CodedObject = "PASS",
                CodedObjectID = "3"
            });
            var interpretedHistoryActions = InterpretHistoryActions(page, refinedInkHistoryActions);
            page.History.HistoryActions.AddRange(interpretedHistoryActions);
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

        // HANNAH CHANGES HERE
        public const double MAX_DISTANCE_Z_SCORE = 3.0;
        public const double DIMENSION_MULTIPLIER_THRESHOLD = 3.0;

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

        /// <summary>
        /// Processes "INK change" action into "INK strokes (erase) [ID: location RefObject [RefObjectID]]" actions
        /// </summary>
        /// <param name="page">Parent page the history action belongs to.</param>
        /// <param name="historyAction">"INK change" history action to process</param>
        public static List<IHistoryAction> ProcessInkChangeHistoryAction(CLPPage page, IHistoryAction historyAction)
        {
            var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
            var processedInkActions = new List<IHistoryAction>();
            var pageObjectsOnPage = ObjectCodedActions.GetPageObjectsOnPageAtHistoryIndex(page, historyItems.First().HistoryIndex);
            // TODO: validation

            var averageStrokeDimensions = InkCodedActions.GetAverageStrokeDimensions(page);
            var averageClosestStrokeDistance = InkCodedActions.GetAverageClosestStrokeDistance(page);
            var closestStrokeDistanceStandardDeviation = InkCodedActions.GetStandardDeviationOfClosestStrokeDistance(page);
            var rollingStatsForDistanceFromCluster = new RollingStandardDeviation();
            var historyItemBuffer = new List<IHistoryItem>();
            IPageObject currentPageObjectReference = null;
            Stroke currentStrokeReference = null;
            var currentLocationReference = Codings.ACTIONID_INK_LOCATION_NONE;
            var isInkAdd = true;
            var currentClusterCentroid = new Point(0, 0);
            var currentClusterWeight = 0.0;
            var isMatchingAgainstCluster = false;

            for (var i = 0; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                historyItemBuffer.Add(currentHistoryItem);
                if (historyItemBuffer.Count == 1)
                {
                    var strokes = currentHistoryItem.StrokesAdded;
                    isInkAdd = true;
                    if (!strokes.Any())
                    {
                        strokes = currentHistoryItem.StrokesRemoved;
                        isInkAdd = false;
                    }

                    // TODO: If strokes.count != 1, deal with point erase
                    // TODO: Validation (strokes is empty)
                    currentStrokeReference = strokes.First();
                    currentPageObjectReference = InkCodedActions.FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, currentStrokeReference, currentHistoryItem.HistoryIndex);
                    currentLocationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                    isMatchingAgainstCluster = false;
                    if (currentPageObjectReference == null)
                    {
                        isMatchingAgainstCluster = true;

                        var strokeCopy = currentStrokeReference.GetStrokeCopyAtHistoryIndex(page, currentHistoryItem.HistoryIndex);
                        currentClusterCentroid = strokeCopy.WeightedCenter();
                        currentClusterWeight = strokeCopy.StrokeWeight();

                        currentPageObjectReference = InkCodedActions.FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, currentClusterCentroid, currentHistoryItem.HistoryIndex);
                        if (currentPageObjectReference != null)
                        {
                            currentLocationReference = InkCodedActions.FindLocationReferenceAtHistoryLocation(page, currentPageObjectReference, currentClusterCentroid, currentHistoryItem.HistoryIndex);
                        }
                    }
                }

                var nextHistoryItem = i + 1 < historyItems.Count ? historyItems[i + 1] : null;
                if (nextHistoryItem != null)
                {
                    var nextStrokes = nextHistoryItem.StrokesAdded;

                    if (!nextStrokes.Any())
                    {
                        nextStrokes = nextHistoryItem.StrokesRemoved;
                    }

                    var nextStroke = nextStrokes.First();
                    var nextPageObjectReference = InkCodedActions.FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, nextStroke, nextHistoryItem.HistoryIndex);
                    var nextLocationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                    var isNextPartOfCurrentCluster = false;
                    if (nextPageObjectReference == null)
                    {
                        if (isMatchingAgainstCluster)
                        {
                            var currentStrokeCopy = currentStrokeReference.GetStrokeCopyAtHistoryIndex(page, currentHistoryItem.HistoryIndex);
                            var currentCentroid = currentStrokeCopy.WeightedCenter();
                            var nextStrokeCopy = nextStroke.GetStrokeCopyAtHistoryIndex(page, nextHistoryItem.HistoryIndex);
                            var nextCentroid = nextStrokeCopy.WeightedCenter();
                            var distanceFromLastStroke = Math.Sqrt(InkCodedActions.DistanceSquaredBetweenPoints(currentCentroid, nextCentroid));
                            var lastStrokeDistanceZScore = (distanceFromLastStroke - averageClosestStrokeDistance) / closestStrokeDistanceStandardDeviation;
                            var isCloseToLastStroke = lastStrokeDistanceZScore <= MAX_DISTANCE_Z_SCORE ||
                                                      distanceFromLastStroke <= averageStrokeDimensions.X * DIMENSION_MULTIPLIER_THRESHOLD ||
                                                      distanceFromLastStroke <= averageStrokeDimensions.Y * DIMENSION_MULTIPLIER_THRESHOLD;

                            bool isCloseToCluster;
                            var distanceFromCluster = Math.Sqrt(InkCodedActions.DistanceSquaredBetweenPoints(currentClusterCentroid, nextCentroid));
                            if (historyItemBuffer.Count == 1)
                            {
                                isCloseToCluster = isCloseToLastStroke;
                            }
                            else
                            {
                                var clusterDistanceZScore = (distanceFromCluster - rollingStatsForDistanceFromCluster.Mean) / rollingStatsForDistanceFromCluster.StandardDeviation;
                                isCloseToCluster = clusterDistanceZScore <= MAX_DISTANCE_Z_SCORE;
                            }

                            if (isCloseToLastStroke ||
                                isCloseToCluster)
                            {
                                isNextPartOfCurrentCluster = true;

                                nextPageObjectReference = InkCodedActions.FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, currentClusterCentroid, nextHistoryItem.HistoryIndex);
                                if (nextPageObjectReference != null)
                                {
                                    nextLocationReference = InkCodedActions.FindLocationReferenceAtHistoryLocation(page, nextPageObjectReference, currentClusterCentroid, nextHistoryItem.HistoryIndex);
                                }

                                var oldClusterWeight = currentClusterWeight;
                                var nextStrokeWeight = nextStrokeCopy.StrokeWeight();
                                currentClusterWeight += nextStrokeWeight;

                                var totalImportance = oldClusterWeight / currentClusterWeight;
                                var importance = nextStrokeWeight / currentClusterWeight;
                                var weightedXAverage = (totalImportance + currentClusterCentroid.X) + (importance * nextCentroid.X);
                                var weightedYAverage = (totalImportance + currentClusterCentroid.Y) + (importance * nextCentroid.Y);
                                currentClusterCentroid = new Point(weightedXAverage, weightedYAverage);

                                rollingStatsForDistanceFromCluster.Update(distanceFromCluster);
                            }
                        }
                    }

                    var isNextInkPartOfCurrent = isInkAdd == nextHistoryItem.StrokesAdded.Any() && isInkAdd == !nextHistoryItem.StrokesRemoved.Any();
                    var isNextPageObjectReferencePartOfCurrent = nextPageObjectReference == null && currentPageObjectReference == null;
                    if (nextPageObjectReference != null &&
                        currentPageObjectReference != null)
                    {
                        isNextPageObjectReferencePartOfCurrent = nextPageObjectReference.ID == currentPageObjectReference.ID;
                    }
                    var isNextLocationReferencePartOfCurrent = nextLocationReference == currentLocationReference;
                    if (isNextInkPartOfCurrent &&
                        (isNextPartOfCurrentCluster || (currentLocationReference == Codings.ACTIONID_INK_LOCATION_OVER && isNextPageObjectReferencePartOfCurrent)))
                    {
                        continue;
                    }
                }

                var refinedHistoryAction = InkCodedActions.GroupAddOrErase(page, historyItemBuffer.Cast<ObjectsOnPageChangedHistoryItem>().ToList(), isInkAdd);
                refinedHistoryAction.CodedObjectID = "A";
                if (currentPageObjectReference != null)
                {
                    refinedHistoryAction.CodedObjectActionID = string.Format("{0} {1} [{2}]",
                                                                             currentLocationReference,
                                                                             currentPageObjectReference.CodedName,
                                                                             currentPageObjectReference.GetCodedIDAtHistoryIndex(refinedHistoryAction.HistoryItems.Last().HistoryIndex));
                }
                refinedHistoryAction.MetaData.Add("REFERENCE_PAGE_OBJECT_ID", currentPageObjectReference.ID);

                processedInkActions.Add(refinedHistoryAction);
                historyItemBuffer.Clear();
            }

            return processedInkActions;
        }

        #endregion // Second Pass: Ink Refinement

        #region Third Pass: Interpretation

        public static List<IHistoryAction> InterpretHistoryActions(CLPPage page, List<IHistoryAction> historyActions)
        {
            var interpretedHistoryActions = new List<IHistoryAction>();

            foreach (var historyAction in historyActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK &&
                    historyAction.CodedObjectAction == Codings.ACTION_INK_ADD)
                {
                    var interpretedHistoryAction = AttemptHistoryActionInterpretation(page, historyAction);
                    interpretedHistoryActions.Add(interpretedHistoryAction);
                }
                else
                {
                    interpretedHistoryActions.Add(historyAction);
                }
            }

            return interpretedHistoryActions;
        }

        public static IHistoryAction AttemptHistoryActionInterpretation(CLPPage page, IHistoryAction historyaction)
        {
            IHistoryAction interpretedAction = null;
            if (historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT) &&
                historyaction.CodedObjectActionID.Contains(Codings.OBJECT_ARRAY))
            {
                interpretedAction = ArrayCodedActions.SkipCounting(page, historyaction);
            }

            return interpretedAction ?? historyaction;
        }

        #endregion // Third Pass: Interpretation

        // 4th pass: simple pattern interpretations

        // 5th pass: complex pattern interpretations

        // 6th pass: Tag generation
    }
}