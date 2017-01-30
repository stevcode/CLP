using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel;
using CLP.InkInterpretation;
using CLP.MachineAnalysis;

namespace CLP.Entities
{
    public class InkCluster
    {
        public enum ClusterTypes
        {
            Unknown,
            Ignore,
            FinalAnswerFillIn,
            FinalAnswerMultipleChoice,
            InkDivide,
            ArraySkipCounting,
            ArrayEquation,
            ARITH,
            PossibleARReqn
        }

        public const string IGNORE_NAME = "IGNORE";
        public const string ANS_MC_NAME = "ANS MC";
        public const string ANS_FI_NAME = "ANS FI";
        public const string INK_DIVIDE_NAME = "INK DIVIDE";

        public InkCluster()
        {
            ClusterName = string.Empty;
            ClusterType = ClusterTypes.Unknown;
            PageObjectReferenceID = string.Empty;
            LocationReference = Codings.EVENT_INFO_INK_LOCATION_NONE;
            Strokes = new StrokeCollection();
        }

        public string ClusterName { get; set; }
        public ClusterTypes ClusterType { get; set; }
        public string PageObjectReferenceID { get; set; }
        public string LocationReference { get; set; }
        public StrokeCollection Strokes { get; }

        public List<Stroke> GetClusterStrokesOnPageAtHistoryIndex(CLPPage page, int historyIndex)
        {
            return page.GetStrokesOnPageAtHistoryIndex(historyIndex).Where(s => Strokes.Contains(s)).ToList();
        }
    }

    public static class InkSemanticEvents
    {
        #region Initialization

        public static ISemanticEvent ChangeOrIgnore(CLPPage page, List<ObjectsOnPageChangedHistoryAction> objectsOnPageChangedHistoryActions, bool isChange = true)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(objectsOnPageChangedHistoryActions), objectsOnPageChangedHistoryActions);

            if (!objectsOnPageChangedHistoryActions.Any())
            {
                return SemanticEvent.GetErrorSemanticEvent(page, objectsOnPageChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_EMPTY_LIST, "ChangeOrIgnore, No Actions");
            }

            if (!objectsOnPageChangedHistoryActions.All(h => h.IsUsingStrokes && !h.IsUsingPageObjects))
            {
                return SemanticEvent.GetErrorSemanticEvent(page, objectsOnPageChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_MIXED_LIST, "ChangeOrIgnore, No Stroke Only Actions");
            }

            var codedObject = Codings.OBJECT_INK;
            var eventType = isChange ? Codings.EVENT_INK_CHANGE : Codings.EVENT_INK_IGNORE;

            var semanticEvent = new SemanticEvent(page, objectsOnPageChangedHistoryActions.Cast<IHistoryAction>().ToList())
                                {
                                    CodedObject = codedObject,
                                    EventType = eventType
                                };

            return semanticEvent;
        }

        #endregion // Initialization

        #region Clustering

        #region Cluster Helpers

        public static readonly List<InkCluster> InkClusters = new List<InkCluster>();

        public static InkCluster GetContainingCluster(Stroke stroke)
        {
            var cluster = InkClusters.FirstOrDefault(c => c.Strokes.Contains(stroke));
            if (cluster != null)
            {
                return cluster;
            }

            var ignoreCluster = InkClusters.FirstOrDefault(c => c.ClusterType == InkCluster.ClusterTypes.Ignore);
            if (ignoreCluster == null)
            {
                cluster = new InkCluster
                          {
                              ClusterType = InkCluster.ClusterTypes.Ignore
                          };
                InkClusters.Add(cluster);
            }
            else
            {
                cluster = ignoreCluster;
            }
            cluster.Strokes.Add(stroke);

            return cluster;
        }

        public static string GetNextAvailableClusterName()
        {
            var numberOfNamedClusters = InkClusters.Count(c => !string.IsNullOrEmpty(c.ClusterName) &&
                                                               (c.ClusterName != InkCluster.IGNORE_NAME ||
                                                                c.ClusterName != InkCluster.ANS_MC_NAME ||
                                                                c.ClusterName != InkCluster.ANS_FI_NAME ||
                                                                c.ClusterName != InkCluster.INK_DIVIDE_NAME));
            numberOfNamedClusters++;
            return numberOfNamedClusters.ToLetter().ToUpper();
        }

        public static void MoveStrokeToDifferentCluster(InkCluster toCluster, Stroke stroke)
        {
            if (!toCluster.Strokes.Contains(stroke))
            {
                toCluster.Strokes.Add(stroke);
            }

            var fromCluster = InkClusters.FirstOrDefault(c => c.Strokes.Contains(stroke) && c != toCluster);
            if (fromCluster == null)
            {
                return;
            }

            if (fromCluster.Strokes.Contains(stroke))
            {
                fromCluster.Strokes.Remove(stroke);
            }

            if (!fromCluster.Strokes.Any())
            {
                InkClusters.Remove(fromCluster);
            }
        }

        public static void MoveStrokesToDifferentCluster(InkCluster toCluster, List<Stroke> strokes)
        {
            foreach (var stroke in strokes)
            {
                MoveStrokeToDifferentCluster(toCluster, stroke);
            }
        }

        #endregion // Cluster Helpers

        #region Pre-Clustering

        public static List<ISemanticEvent> GenerateArrayInkDivideSemanticEvents(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvents), semanticEvents);

            var allRefinedEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in semanticEvents)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_INK &&
                    semanticEvent.EventType == Codings.EVENT_INK_CHANGE)
                {
                    var historyActions = semanticEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().OrderBy(h => h.HistoryActionIndex).ToList();
                    var refinedInkEvents = new List<ISemanticEvent>();
                    var historyActionBuffer = new List<IHistoryAction>();

                    foreach (var currentHistoryAction in historyActions)
                    {
                        var inkDivideEvent = ArraySemanticEvents.InkDivide(page, currentHistoryAction);
                        if (inkDivideEvent != null)
                        {
                            if (historyActionBuffer.Any())
                            {
                                var bufferedSemanticEvent = new SemanticEvent(page, historyActionBuffer)
                                {
                                    CodedObject = Codings.OBJECT_INK,
                                    EventType = Codings.EVENT_INK_CHANGE
                                };
                                refinedInkEvents.Add(bufferedSemanticEvent);
                                historyActionBuffer.Clear();
                            }

                            refinedInkEvents.Add(inkDivideEvent);
                            continue;
                        }

                        historyActionBuffer.Add(currentHistoryAction);
                    }

                    if (historyActionBuffer.Any())
                    {
                        var bufferedSemanticEvent = new SemanticEvent(page, historyActionBuffer)
                        {
                            CodedObject = Codings.OBJECT_INK,
                            EventType = Codings.EVENT_INK_CHANGE
                        };
                        refinedInkEvents.Add(bufferedSemanticEvent);
                        historyActionBuffer.Clear();
                    }

                    allRefinedEvents.AddRange(refinedInkEvents);
                }
                else
                {
                    allRefinedEvents.Add(semanticEvent);
                }
            }

            return allRefinedEvents;
        }

        public static void DefineMultipleChoiceClusters(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvents), semanticEvents);

            if (!semanticEvents.Any())
            {
                return;
            }

            var multipleChoiceSemanticEvents = semanticEvents.Where(e => e.CodedObject == Codings.OBJECT_MULTIPLE_CHOICE).ToList();
            var eventsGroupedByMultipleChoicePageObject = multipleChoiceSemanticEvents.GroupBy(e => e.ReferencePageObjectID).ToList();
            foreach (var group in eventsGroupedByMultipleChoicePageObject)
            {
                var multipleChoiceID = group.Key;

                var historyActions = group.SelectMany(e => e.HistoryActions.Cast<MultipleChoiceBubbleStatusChangedHistoryAction>()).ToList();
                var strokeIDsAdded = historyActions.SelectMany(h => h.StrokeIDsAdded).ToList();
                var strokeIDsRemoved = historyActions.SelectMany(h => h.StrokeIDsRemoved).ToList();
                var strokeIDs = strokeIDsAdded.Concat(strokeIDsRemoved).Distinct().ToList();
                var strokes = strokeIDs.Select(page.GetStrokeByIDOnPageOrInHistory).ToList();

                var multiplieChoiceCluster = new InkCluster
                                             {
                                                 ClusterName = InkCluster.ANS_MC_NAME,
                                                 ClusterType = InkCluster.ClusterTypes.FinalAnswerMultipleChoice,
                                                 PageObjectReferenceID = multipleChoiceID,
                                                 LocationReference = Codings.EVENT_INFO_INK_LOCATION_OVER
                                             };
                MoveStrokesToDifferentCluster(multiplieChoiceCluster, strokes);
                InkClusters.Add(multiplieChoiceCluster);
            }
        }

        public static void DefineFillInClusters(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvents), semanticEvents);

            if (!semanticEvents.Any())
            {
                return;
            }

            var fillInSemanticEvents = semanticEvents.Where(e => e.CodedObject == Codings.OBJECT_FILL_IN).ToList();
            var eventsGroupedByInterpretationRegionPageObject = fillInSemanticEvents.GroupBy(e => e.ReferencePageObjectID).ToList();
            foreach (var group in eventsGroupedByInterpretationRegionPageObject)
            {
                var interpretationID = group.Key;

                var historyActions = group.SelectMany(e => e.HistoryActions.Cast<FillInAnswerChangedHistoryAction>()).ToList();
                var strokeIDsAdded = historyActions.SelectMany(h => h.StrokeIDsAdded).ToList();
                var strokeIDsRemoved = historyActions.SelectMany(h => h.StrokeIDsRemoved).ToList();
                var strokeIDs = strokeIDsAdded.Concat(strokeIDsRemoved).Distinct().ToList();
                var strokes = strokeIDs.Select(page.GetStrokeByIDOnPageOrInHistory).ToList();

                var fillInCluster = new InkCluster
                                    {
                                        ClusterName = InkCluster.ANS_FI_NAME,
                                        ClusterType = InkCluster.ClusterTypes.FinalAnswerFillIn,
                                        PageObjectReferenceID = interpretationID,
                                        LocationReference = Codings.EVENT_INFO_INK_LOCATION_OVER
                                    };
                MoveStrokesToDifferentCluster(fillInCluster, strokes);
                InkClusters.Add(fillInCluster);
            }
        }

        public static void DefineArrayInkDivideClusters(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvents), semanticEvents);

            if (!semanticEvents.Any())
            {
                return;
            }

            var inkDivideSemanticEvents =
                semanticEvents.Where(e => e.CodedObject == Codings.OBJECT_ARRAY && (e.EventType == Codings.EVENT_ARRAY_DIVIDE_INK || e.EventType == Codings.EVENT_ARRAY_DIVIDE_INK_ERASE)).ToList();
            var eventsGroupedByArrayPageObject = inkDivideSemanticEvents.GroupBy(e => e.ReferencePageObjectID).ToList();
            foreach (var group in eventsGroupedByArrayPageObject)
            {
                var arrayID = group.Key;

                var historyActions = group.SelectMany(e => e.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>()).ToList();
                var strokeIDsAdded = historyActions.SelectMany(h => h.StrokeIDsAdded).ToList();
                var strokeIDsRemoved = historyActions.SelectMany(h => h.StrokeIDsRemoved).ToList();
                var strokeIDs = strokeIDsAdded.Concat(strokeIDsRemoved).Distinct().ToList();
                var strokes = strokeIDs.Select(page.GetStrokeByIDOnPageOrInHistory).ToList();

                var fillInCluster = new InkCluster
                                    {
                                        ClusterName = InkCluster.INK_DIVIDE_NAME,
                                        ClusterType = InkCluster.ClusterTypes.FinalAnswerFillIn,
                                        PageObjectReferenceID = arrayID,
                                        LocationReference = Codings.EVENT_INFO_INK_LOCATION_OVER
                                    };
                MoveStrokesToDifferentCluster(fillInCluster, strokes);
                InkClusters.Add(fillInCluster);
            }
        }

        public static void DefineSkipCountClusters(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvents), semanticEvents);

            if (!semanticEvents.Any())
            {
                return;
            }

            // This code looks for any of the following patterns for each array, where all other Event types are ignored:
            // ARR add, INK change, ARR delete/ARR move/End
            // ARR move, INK change, ARR delete/ARR move/End
            // When one of the above patterns is recognized, the start and end historyIndex and associated arrayID are stored in a list
            // of patternEndPoints. Every identified patternEndPoint will be analyzed at that point in history for skip counting
            // and any skip count strokes found at that point will be isolated in their own clusters.
            var patternStartPoints = new Dictionary<string, string>();
            var patternEndPoints = new List<dynamic>();

            #region Recognized Pattern End Points

            foreach (var currentSemanticEvent in semanticEvents)
            {
                if (currentSemanticEvent.CodedObject == Codings.OBJECT_ARRAY)
                {
                    var arrayID = currentSemanticEvent.ReferencePageObjectID;

                    if (currentSemanticEvent.EventType == Codings.EVENT_OBJECT_ADD)
                    {
                        if (!patternStartPoints.Keys.Contains(arrayID))
                        {
                            var startPattern = $"{Codings.EVENT_OBJECT_ADD};{currentSemanticEvent.HistoryActions.First().HistoryActionIndex}";
                            patternStartPoints.Add(arrayID, startPattern);
                        }
                    }
                    else if (currentSemanticEvent.EventType == Codings.EVENT_OBJECT_MOVE)
                    {
                        var startPattern = $"{Codings.EVENT_OBJECT_MOVE};{currentSemanticEvent.HistoryActions.First().HistoryActionIndex}";
                        if (patternStartPoints.Keys.Contains(arrayID))
                        {
                            if (patternStartPoints[arrayID].Contains(Codings.EVENT_INK_CHANGE))
                            {
                                var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                                var endHistoryIndex = currentSemanticEvent.HistoryActions.Last().HistoryActionIndex;
                                patternEndPoints.Add(new
                                {
                                    ArrayID = arrayID,
                                    StartHistoryIndex = startHistoryIndex,
                                    EndHistoryIndex = endHistoryIndex
                                });
                            }

                            patternStartPoints[arrayID] = startPattern;
                        }
                        else
                        {
                            patternStartPoints.Add(arrayID, startPattern);
                        }
                    }
                    else if (currentSemanticEvent.EventType == Codings.EVENT_OBJECT_DELETE)
                    {
                        if (!patternStartPoints.Keys.Contains(arrayID))
                        {
                            continue;
                        }

                        if (patternStartPoints[arrayID].Contains(Codings.EVENT_INK_CHANGE))
                        {
                            var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                            var endHistoryIndex = currentSemanticEvent.HistoryActions.Last().HistoryActionIndex;
                            patternEndPoints.Add(new
                            {
                                ArrayID = arrayID,
                                StartHistoryIndex = startHistoryIndex,
                                EndHistoryIndex = endHistoryIndex
                            });
                        }

                        patternStartPoints.Remove(arrayID);
                    }
                }
                else if (currentSemanticEvent.EventType == Codings.EVENT_INK_CHANGE)
                {
                    var arrayIDs = patternStartPoints.Keys.ToList();
                    foreach (var arrayID in arrayIDs)
                    {
                        var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                        var startPattern = $"{Codings.EVENT_INK_CHANGE};{startHistoryIndex}";
                        patternStartPoints[arrayID] = startPattern;
                    }
                }
            }

            foreach (var arrayID in patternStartPoints.Keys)
            {
                if (!patternStartPoints[arrayID].Contains(Codings.EVENT_INK_CHANGE))
                {
                    continue;
                }

                var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                var endHistoryIndex = semanticEvents.Last().HistoryActions.Last().HistoryActionIndex;
                patternEndPoints.Add(new
                {
                    ArrayID = arrayID,
                    StartHistoryIndex = startHistoryIndex,
                    EndHistoryIndex = endHistoryIndex
                });
            }

            #endregion // Recognized Pattern End Points

            #region Test for Skip Counting at Pattern End Points

            // Test for skip counting at each patternEndPoint. If it exists at the given point in history, move all skip 
            // count strokes from their current clusters into a new cluster for that historyIndex and tag as ArraySkipCounting cluster type.
            foreach (var patternEndPoint in patternEndPoints)
            {
                var arrayID = (string)patternEndPoint.ArrayID;
                var startHistoryIndex = int.Parse((string)patternEndPoint.StartHistoryIndex);
                var endHistoryIndex = (int)patternEndPoint.EndHistoryIndex;

                var array = page.GetPageObjectByIDOnPageOrInHistory(arrayID) as CLPArray;
                if (array == null)
                {
                    continue;
                }

                var strokesAddedToPage = page.GetStrokesAddedToPageBetweenHistoryIndexes(startHistoryIndex, endHistoryIndex);
                var strokeGroupPerRowHistory = ArraySemanticEvents.GroupPossibleSkipCountStrokes(page, array, strokesAddedToPage, endHistoryIndex);
                var interpretedRowValuesHistory = ArraySemanticEvents.InterpretSkipCountGroups(page, array, strokeGroupPerRowHistory, endHistoryIndex);
                var isSkipCountingHistory = ArraySemanticEvents.IsSkipCounting(interpretedRowValuesHistory);

                var strokesOnPage = page.GetStrokesOnPageAtHistoryIndex(endHistoryIndex);
                var strokeGroupPerRowOnPage = ArraySemanticEvents.GroupPossibleSkipCountStrokes(page, array, strokesOnPage, endHistoryIndex);
                var interpretedRowValuesOnPage = ArraySemanticEvents.InterpretSkipCountGroups(page, array, strokeGroupPerRowOnPage, endHistoryIndex);
                var isSkipCountingOnPage = ArraySemanticEvents.IsSkipCounting(interpretedRowValuesOnPage);

                if (isSkipCountingHistory || isSkipCountingOnPage)
                {
                    var skipCluster = new InkCluster
                                      {
                                          ClusterType = InkCluster.ClusterTypes.ArraySkipCounting,
                                          PageObjectReferenceID = arrayID,
                                          LocationReference = Codings.EVENT_INFO_INK_LOCATION_RIGHT_SKIP
                                      };
                    var skipStrokesHistory = strokeGroupPerRowHistory.Where(kv => kv.Key != 0 && kv.Key != -1).SelectMany(kv => kv.Value).Distinct().ToList();
                    var skipStrokesOnPage = strokeGroupPerRowOnPage.Where(kv => kv.Key != 0 && kv.Key != -1).SelectMany(kv => kv.Value).Distinct().ToList();
                    var skipStrokes = skipStrokesHistory.Concat(skipStrokesOnPage).Distinct().ToList();
                    foreach (var stroke in skipStrokes)
                    {
                        var currentCluster = GetContainingCluster(stroke);
                        if (currentCluster.ClusterType != InkCluster.ClusterTypes.InkDivide)
                        {
                            MoveStrokeToDifferentCluster(skipCluster, stroke);
                        }
                    }
                    InkClusters.Add(skipCluster);
                }

                // TODO: ELSE: Test for skip counting along the bottom  here, give it it's own cluster as above, then continue below with ArrayEquation.
                // BUG: Right now, ARR eqn relies on OPTICS to create an Unknown cluster completely over the array, See "Update to Pre-Clustering, Line 545
                // Will need to create a temp cluster of possible arr eqn, then after optics runs, any cluster  that contains strokes from these temp clusters will
                // be split into just a cluster containing the strokes from temp cluster and a cluster containing the rest from the optics cluster
            }

            #endregion // Test for Skip Counting at Pattern End Points
        }

        #endregion // Pre-Clustering

        #region OPTICS Clustering

        public static void GenerateInitialInkClusters(List<ISemanticEvent> semanticEvents)
        {
            Argument.IsNotNull(nameof(semanticEvents), semanticEvents);

            if (!semanticEvents.Any())
            {
                return;
            }

            var inkEvents = semanticEvents.Where(h => h.CodedObject == Codings.OBJECT_INK && h.EventType == Codings.EVENT_INK_CHANGE).ToList();
            var historyActions = inkEvents.SelectMany(h => h.HistoryActions).OfType<ObjectsOnPageChangedHistoryAction>().ToList();
            var strokesAdded = historyActions.SelectMany(i => i.StrokesAdded).ToList();

            var unclusteredStrokes = new StrokeCollection();
            var smallStrokes = strokesAdded.Where(s => s.IsInvisiblySmall()).ToList();
            foreach (var smallStroke in smallStrokes)
            {
                strokesAdded.Remove(smallStroke);
                unclusteredStrokes.Add(smallStroke);
            }

            var strokeClusters = new List<StrokeCollection>();

            if (strokesAdded.Count == 1)
            {
                strokeClusters.Add(new StrokeCollection(strokesAdded));
            }
            else if (strokesAdded.Count > 1)
            {
                const int MAX_EPSILON = 1000;
                const int MINIMUM_STROKES_IN_CLUSTER = 1;

                Func<Stroke, Stroke, double> distanceEquation = (s1, s2) => Math.Sqrt(s1.DistanceSquaredByClosestPoint(s2));

                var optics = new OPTICS<Stroke>(MAX_EPSILON, MINIMUM_STROKES_IN_CLUSTER, strokesAdded, distanceEquation);
                optics.BuildReachability();
                var reachabilityDistances = optics.ReachabilityDistances().ToList();

                const double CLUSTERING_EPSILON = 51.0;

                var currentCluster = new StrokeCollection();
                var allClusteredStrokes = new List<Stroke>();
                var firstStrokeIndex = (int)reachabilityDistances[0].OriginalIndex;
                var firstStroke = strokesAdded[firstStrokeIndex];
                currentCluster.Add(firstStroke);
                allClusteredStrokes.Add(firstStroke);

                for (var i = 1; i < reachabilityDistances.Count; i++)
                {
                    var strokeIndex = (int)reachabilityDistances[i].OriginalIndex;
                    var stroke = strokesAdded[strokeIndex];

                    // Epsilon cluster decision.
                    var currentReachabilityDistance = reachabilityDistances[i].ReachabilityDistance;
                    if (currentReachabilityDistance < CLUSTERING_EPSILON)
                    {
                        currentCluster.Add(stroke);
                        allClusteredStrokes.Add(stroke);
                        continue;
                    }

                    var fullCluster = currentCluster.ToList();
                    currentCluster.Clear();
                    currentCluster.Add(stroke);
                    allClusteredStrokes.Add(stroke);
                    strokeClusters.Add(new StrokeCollection(fullCluster));
                }
                if (currentCluster.Any())
                {
                    var finalCluster = currentCluster.ToList();
                    strokeClusters.Add(new StrokeCollection(finalCluster));
                }

                foreach (var stroke in strokesAdded.Where(stroke => !allClusteredStrokes.Contains(stroke)))
                {
                    unclusteredStrokes.Add(stroke);
                }
            }

            var ignoredCluster = new InkCluster
            {
                ClusterName = InkCluster.IGNORE_NAME,
                ClusterType = InkCluster.ClusterTypes.Ignore
            };
            MoveStrokesToDifferentCluster(ignoredCluster, unclusteredStrokes.ToList());
            InkClusters.Add(ignoredCluster);

            foreach (var strokeCluster in strokeClusters)
            {
                var inkCluster = new InkCluster();
                MoveStrokesToDifferentCluster(inkCluster, strokeCluster.ToList());
                InkClusters.Add(inkCluster);
            }
        }

        #endregion // OPTICS Clustering

        #region Refine OPTICS Clusters

        /// <summary>Processes "INK change" event into "INK strokes (erase) [ID] location RefObject [RefObjectID]" events</summary>
        public static List<ISemanticEvent> ProcessInkChangeSemanticEvent(CLPPage page, ISemanticEvent semanticEvent)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvent), semanticEvent);

            var processedInkEvents = new List<ISemanticEvent>();

            var historyActions = semanticEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().OrderBy(h => h.HistoryActionIndex).ToList();

            var firstHistoryAction = historyActions.First();
            var previousStrokes = firstHistoryAction.StrokesAdded;
            var isPreviousInkAdd = true;
            if (!previousStrokes.Any())
            {
                previousStrokes = firstHistoryAction.StrokesRemoved;
                isPreviousInkAdd = false;
            }

            var previousStroke = previousStrokes.First();
            var previousClusterReference = GetContainingCluster(previousStroke);

            var historyActionBuffer = new List<IHistoryAction>
                                      {
                                          firstHistoryAction
                                      };

            for (var i = 1; i < historyActions.Count; i++)
            {
                var currentHistoryAction = historyActions[i];
                var currentStrokes = currentHistoryAction.StrokesAdded;
                var isCurrentInkAdd = true;
                if (!currentStrokes.Any())
                {
                    currentStrokes = currentHistoryAction.StrokesRemoved;
                    isCurrentInkAdd = false;
                }

                var currentStroke = currentStrokes.First();
                var currentClusterReference = GetContainingCluster(currentStroke);

                var isBreakCondition = isPreviousInkAdd != isCurrentInkAdd || previousClusterReference != currentClusterReference;
                if (isBreakCondition)
                {
                    var processedInkEvent = ProcessINKChangeHistoryActionBuffer(page,
                                                                                previousClusterReference,
                                                                                historyActionBuffer,
                                                                                isPreviousInkAdd,
                                                                                currentHistoryAction.HistoryActionIndex,
                                                                                previousStroke);

                    processedInkEvents.Add(processedInkEvent);

                    historyActionBuffer.Clear();
                }

                historyActionBuffer.Add(currentHistoryAction);
                isPreviousInkAdd = isCurrentInkAdd;
                previousStroke = currentStroke;
                previousClusterReference = currentClusterReference;
            }

            if (historyActionBuffer.Any())
            {
                var processedInkEvent = ProcessINKChangeHistoryActionBuffer(page,
                                                                                previousClusterReference,
                                                                                historyActionBuffer,
                                                                                isPreviousInkAdd,
                                                                                historyActions.Last().HistoryActionIndex + 1,
                                                                                previousStroke);

                processedInkEvents.Add(processedInkEvent);
            }

            return processedInkEvents;
        }

        public static ISemanticEvent ProcessINKChangeHistoryActionBuffer(CLPPage page, InkCluster cluster, List<IHistoryAction> historyActionBuffer, bool isPreviousInkAdd, int currentHistoryActionIndex, Stroke previousStroke)
        {
            if (cluster.ClusterType == InkCluster.ClusterTypes.Ignore)
            {
                var inkIgnoreEvent = new SemanticEvent(page, historyActionBuffer)
                                     {
                                         CodedObject = Codings.OBJECT_INK,
                                         EventType = Codings.EVENT_INK_IGNORE
                                     };

                return inkIgnoreEvent;
            }

            var codedObject = Codings.OBJECT_INK;
            var eventType = isPreviousInkAdd ? Codings.EVENT_INK_ADD : Codings.EVENT_INK_ERASE;
            if (string.IsNullOrEmpty(cluster.ClusterName))
            {
                cluster.ClusterName = GetNextAvailableClusterName();
            }
            var codedID = cluster.ClusterName;
            var eventInfo = string.Empty;
            var referencePageObjectID = string.Empty;

            var previousHistoryIndex = currentHistoryActionIndex - 1;
            if (cluster.ClusterType == InkCluster.ClusterTypes.ArraySkipCounting)
            {
                var arrayID = cluster.PageObjectReferenceID;
                var array = page.GetPageObjectByIDOnPageOrInHistory(arrayID);
                if (array != null)
                {
                    var locationReference = Codings.EVENT_INFO_INK_LOCATION_RIGHT_SKIP;
                    var referenceCodedObject = Codings.OBJECT_ARRAY;
                    var referenceCodedID = array.GetCodedIDAtHistoryIndex(previousHistoryIndex);

                    eventInfo = $"{locationReference} {referenceCodedObject} [{referenceCodedID}]";
                    referencePageObjectID = arrayID;
                }
            }
            else
            {
                var pageObjectsOnPage = page.GetPageObjectsOnPageAtHistoryIndex(previousHistoryIndex);
                var pageObjectReference = FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, previousStroke, previousHistoryIndex);
                var locationReference = Codings.EVENT_INFO_INK_LOCATION_NONE;
                if (pageObjectReference != null)
                {
                    locationReference = Codings.EVENT_INFO_INK_LOCATION_OVER;
                }
                else
                {
                    var clusterCentroid = cluster.Strokes.WeightedCentroid();

                    pageObjectReference = FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, clusterCentroid, previousHistoryIndex);
                    if (pageObjectReference != null)
                    {
                        locationReference = FindLocationReferenceAtHistoryLocation(page, pageObjectReference, clusterCentroid, previousHistoryIndex);
                    }
                }

                if (pageObjectReference != null)
                {
                    var referenceCodedObject = pageObjectReference.CodedName;
                    var referenceCodedID = pageObjectReference.GetCodedIDAtHistoryIndex(previousHistoryIndex);

                    eventInfo = $"{locationReference} {referenceCodedObject} [{referenceCodedID}]";
                    referencePageObjectID = pageObjectReference.ID;
                }
            }

            var processedInkEvent = new SemanticEvent(page, historyActionBuffer)
                                    {
                                        CodedObject = codedObject,
                                        EventType = eventType,
                                        CodedObjectID = codedID,
                                        EventInformation = eventInfo,
                                        ReferencePageObjectID = referencePageObjectID
                                    };

            return processedInkEvent;
        }

        #endregion // Refine OPTICS Clusters

        #endregion // Clustering

        #region Interpretation

        public static ISemanticEvent Arithmetic(CLPPage page, ISemanticEvent inkEvent)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(inkEvent), inkEvent);

            if (inkEvent.CodedObject != Codings.OBJECT_INK ||
                !(inkEvent.EventType == Codings.EVENT_INK_ADD || 
                  inkEvent.EventType == Codings.EVENT_INK_ERASE))
            {
                return null;
            }

            var isArithAdd = inkEvent.EventType == Codings.EVENT_INK_ADD;
            var strokes = isArithAdd
                              ? inkEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => h.StrokesRemoved).ToList();

            var firstStroke = strokes.First();
            var cluster = GetContainingCluster(firstStroke);
            switch (cluster.ClusterType)
            {
                case InkCluster.ClusterTypes.Unknown:
                {
                    if (!isArithAdd)
                    {
                        return null;
                    }

                    var orderedStrokes = GetOrderStrokesWereAddedToPage(page, strokes);
                    var interpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(orderedStrokes));
                    if (interpretation == null)
                    {
                        return null;
                    }

                    cluster.ClusterType = InkCluster.ClusterTypes.ARITH;

                    var semanticEvent = new SemanticEvent(page, inkEvent)
                                        {
                                            CodedObject = Codings.OBJECT_ARITH,
                                            EventType = Codings.EVENT_ARITH_ADD,
                                            CodedObjectID = inkEvent.CodedObjectID,
                                            EventInformation = $"\"{interpretation}\""
                                        };

                    return semanticEvent;
                }
                case InkCluster.ClusterTypes.ARITH:
                {
                    var orderedStrokes = GetOrderStrokesWereAddedToPage(page, strokes);
                    var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));
                    var interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                    var changedInterpretation = $"\"{interpretation}\"";

                    var strokesOnPage = cluster.GetClusterStrokesOnPageAtHistoryIndex(page, inkEvent.HistoryActions.Last().HistoryActionIndex);
                    var orderedStrokesOnPage = GetOrderStrokesWereAddedToPage(page, strokesOnPage);
                    var onPageInterpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(orderedStrokesOnPage)) ?? string.Empty;
                    onPageInterpretation = $"\"{onPageInterpretation}\"";

                    var formattedInterpretation = $"{changedInterpretation}; {onPageInterpretation}";

                    var semanticEvent = new SemanticEvent(page, inkEvent)
                                        {
                                            CodedObject = Codings.OBJECT_ARITH,
                                            EventType = isArithAdd ? Codings.EVENT_ARITH_ADD : Codings.EVENT_ARITH_ERASE,
                                            CodedObjectID = inkEvent.CodedObjectID,
                                            EventInformation = formattedInterpretation
                                        };

                    return semanticEvent;
                }
            }

            return null;
        }

        public static List<ISemanticEvent> RefineANS_FIClusters(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var interpretationRegion = page.PageObjects.FirstOrDefault(p => p is InterpretationRegion) as InterpretationRegion;
            if (interpretationRegion == null)
            {
                return semanticEvents;
            }

            var ansCluster = InkClusters.FirstOrDefault(c => c.ClusterType == InkCluster.ClusterTypes.FinalAnswerFillIn);
            if (ansCluster == null)
            {
                ansCluster = new InkCluster
                {
                    ClusterType = InkCluster.ClusterTypes.FinalAnswerFillIn,
                    LocationReference = Codings.EVENT_INFO_INK_LOCATION_OVER,
                    PageObjectReferenceID = interpretationRegion.ID
                };

                InkClusters.Add(ansCluster);
            }

            var allRefinedEvents = new List<ISemanticEvent>();

            foreach (var semanticEvent in semanticEvents)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_INK &&
                    semanticEvent.EventType == Codings.EVENT_INK_CHANGE)
                {
                    var historyActions = semanticEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().OrderBy(h => h.HistoryActionIndex).ToList();
                    var historyActionBuffer = new List<IHistoryAction>();

                    var isPreviousInkAdd = false;
                    var isPreviousStrokeOverInterpretationRegion = false;

                    var firstHistoryAction = historyActions.First();
                    historyActionBuffer.Add(firstHistoryAction);
                    var previousStrokes = firstHistoryAction.StrokesAdded;
                    isPreviousInkAdd = true;
                    if (!previousStrokes.Any())
                    {
                        previousStrokes = firstHistoryAction.StrokesRemoved;
                        isPreviousInkAdd = false;
                    }

                    var previousStroke = previousStrokes.First();
                    var isPreviousStrokeInvisiblySmall = previousStroke.IsInvisiblySmall();
                    if (!isPreviousStrokeInvisiblySmall)
                    {
                        var percentOfPreviousStrokeOverlap = PercentageOfStrokeOverPageObjectAtHistoryIndex(page, interpretationRegion, previousStroke, firstHistoryAction.HistoryActionIndex);
                        if (percentOfPreviousStrokeOverlap > 95.0)
                        {
                            isPreviousStrokeOverInterpretationRegion = true;
                        }
                    }

                    for (var i = 1; i < historyActions.Count; i++)
                    {
                        var currentHistoryAction = historyActions[i];
                        var currentStrokes = currentHistoryAction.StrokesAdded;
                        var isCurrentInkAdd = true;
                        if (!currentStrokes.Any())
                        {
                            currentStrokes = currentHistoryAction.StrokesRemoved;
                            isCurrentInkAdd = false;
                        }

                        var currentStroke = currentStrokes.First();
                        var isCurrentStrokeInvisiblySmall = currentStroke.IsInvisiblySmall();
                        var percentOfCurrentStrokeOverlap = PercentageOfStrokeOverPageObjectAtHistoryIndex(page, interpretationRegion, currentStroke, currentHistoryAction.HistoryActionIndex);
                        var isCurrentStrokeOverInterpretationRegion = percentOfCurrentStrokeOverlap > 95.0 && !isCurrentStrokeInvisiblySmall;

                        var isBreakCondition = isCurrentStrokeOverInterpretationRegion != isPreviousStrokeOverInterpretationRegion ||
                                               (isPreviousStrokeOverInterpretationRegion && isCurrentInkAdd != isPreviousInkAdd);

                        if (isBreakCondition)
                        {
                            if (isPreviousStrokeOverInterpretationRegion)
                            {
                                var strokes = historyActionBuffer.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => isPreviousInkAdd ? h.StrokesAdded : h.StrokesRemoved).ToList();
                                foreach (var stroke in strokes)
                                {
                                    MoveStrokeToDifferentCluster(ansCluster, stroke);
                                }

                                var orderedStrokes = GetOrderStrokesWereAddedToPage(page, strokes);
                                var orderedStrokesOnPage = GetOrderStrokesWereAddedToPage(page, ansCluster.GetClusterStrokesOnPageAtHistoryIndex(page, currentHistoryAction.HistoryActionIndex - 1));

                                var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));
                                string interpretation;

                                var interpretationsOnPage = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokesOnPage));
                                string interpretationOnPage;

                                var relationDefinitionTag = page.Tags.FirstOrDefault(t => t is IRelationPart || t is DivisionRelationDefinitionTag);
                                var answer = relationDefinitionTag == null
                                                 ? "UNDEFINED"
                                                 : relationDefinitionTag is IRelationPart
                                                     ? (relationDefinitionTag as IRelationPart).RelationPartAnswerValue.ToString()
                                                     : (relationDefinitionTag as DivisionRelationDefinitionTag).Quotient.ToString();

                                if (answer == "UNDEFINED")
                                {
                                    interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                                    interpretationOnPage = InkInterpreter.InterpretationClosestToANumber(interpretationsOnPage);
                                }
                                else
                                {
                                    int expectedValue;
                                    if (int.TryParse(answer, out expectedValue))
                                    {
                                        interpretation = InkInterpreter.MatchInterpretationToExpectedInt(interpretations, expectedValue);
                                        interpretationOnPage = InkInterpreter.MatchInterpretationToExpectedInt(interpretationsOnPage, expectedValue);
                                        if (string.IsNullOrEmpty(interpretation))
                                        {
                                            interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                                        }
                                        if (string.IsNullOrEmpty(interpretationOnPage))
                                        {
                                            interpretationOnPage = InkInterpreter.InterpretationClosestToANumber(interpretationsOnPage);
                                        }
                                    }
                                    else
                                    {
                                        interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                                        interpretationOnPage = InkInterpreter.InterpretationClosestToANumber(interpretationsOnPage);
                                    }
                                }

                                var correctness = answer == "UNDEFINED" ? "unknown" : answer == interpretationOnPage ? "COR" : "INC";

                                var fillInEvent = new SemanticEvent(page, historyActionBuffer)
                                {
                                    CodedObject = Codings.OBJECT_FILL_IN,
                                    EventType = isPreviousInkAdd ? Codings.EVENT_FILL_IN_ADD : Codings.EVENT_FILL_IN_ERASE,
                                    CodedObjectID = answer,
                                    EventInformation = $"\"{interpretation}\"; \"{interpretationOnPage}\", {correctness}"
                                };

                                allRefinedEvents.Add(fillInEvent);
                                historyActionBuffer.Clear();
                            }
                            else
                            {
                                var inkChangeEvent = new SemanticEvent(page, historyActionBuffer)
                                {
                                    CodedObject = Codings.OBJECT_INK,
                                    EventType = Codings.EVENT_INK_CHANGE
                                };

                                allRefinedEvents.Add(inkChangeEvent);
                                historyActionBuffer.Clear();
                            }
                        }

                        historyActionBuffer.Add(currentHistoryAction);
                        isPreviousInkAdd = isCurrentInkAdd;
                        isPreviousStrokeOverInterpretationRegion = isCurrentStrokeOverInterpretationRegion;
                    }

                    if (historyActionBuffer.Any())
                    {
                        if (isPreviousStrokeOverInterpretationRegion)
                        {
                            var strokes = historyActionBuffer.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => isPreviousInkAdd ? h.StrokesAdded : h.StrokesRemoved).ToList();
                            foreach (var stroke in strokes)
                            {
                                MoveStrokeToDifferentCluster(ansCluster, stroke);
                            }

                            var orderedStrokes = GetOrderStrokesWereAddedToPage(page, strokes);
                            var orderedStrokesOnPage = GetOrderStrokesWereAddedToPage(page, ansCluster.GetClusterStrokesOnPageAtHistoryIndex(page, historyActions.Last().HistoryActionIndex));

                            var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));
                            string interpretation;

                            var interpretationsOnPage = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokesOnPage));
                            string interpretationOnPage;

                            var relationDefinitionTag = page.Tags.FirstOrDefault(t => t is IRelationPart || t is DivisionRelationDefinitionTag);
                            var answer = relationDefinitionTag == null
                                             ? "UNDEFINED"
                                             : relationDefinitionTag is IRelationPart
                                                 ? (relationDefinitionTag as IRelationPart).RelationPartAnswerValue.ToString()
                                                 : (relationDefinitionTag as DivisionRelationDefinitionTag).Quotient.ToString();

                            if (answer == "UNDEFINED")
                            {
                                interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                                interpretationOnPage = InkInterpreter.InterpretationClosestToANumber(interpretationsOnPage);
                            }
                            else
                            {
                                int expectedValue;
                                if (int.TryParse(answer, out expectedValue))
                                {
                                    interpretation = InkInterpreter.MatchInterpretationToExpectedInt(interpretations, expectedValue);
                                    interpretationOnPage = InkInterpreter.MatchInterpretationToExpectedInt(interpretationsOnPage, expectedValue);
                                    if (string.IsNullOrEmpty(interpretation))
                                    {
                                        interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                                    }
                                    if (string.IsNullOrEmpty(interpretationOnPage))
                                    {
                                        interpretationOnPage = InkInterpreter.InterpretationClosestToANumber(interpretationsOnPage);
                                    }
                                }
                                else
                                {
                                    interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                                    interpretationOnPage = InkInterpreter.InterpretationClosestToANumber(interpretationsOnPage);
                                }
                            }

                            var correctness = answer == "UNDEFINED" ? "unknown" : answer == interpretationOnPage ? "COR" : "INC";

                            var fillInEvent = new SemanticEvent(page, historyActionBuffer)
                            {
                                CodedObject = Codings.OBJECT_FILL_IN,
                                EventType = isPreviousInkAdd ? Codings.EVENT_FILL_IN_ADD : Codings.EVENT_FILL_IN_ERASE,
                                CodedObjectID = answer,
                                EventInformation = string.Format("\"{0}\"; \"{1}\", {2}", interpretation, interpretationOnPage, correctness)
                            };

                            allRefinedEvents.Add(fillInEvent);
                            historyActionBuffer.Clear();
                        }
                        else
                        {
                            var inkChangeEvent = new SemanticEvent(page, historyActionBuffer)
                            {
                                CodedObject = Codings.OBJECT_INK,
                                EventType = Codings.EVENT_INK_CHANGE
                            };

                            allRefinedEvents.Add(inkChangeEvent);
                            historyActionBuffer.Clear();
                        }
                    }
                }
                else
                {
                    allRefinedEvents.Add(semanticEvent);
                }
            }

            return allRefinedEvents;
        }

        #endregion // Interpretation

        #region Utility

        public static List<Stroke> GetOrderStrokesWereAddedToPage(CLPPage page, List<Stroke> strokes)
        {
            var historyActions = page.History.CompleteOrderedHistoryActions.OfType<ObjectsOnPageChangedHistoryAction>().Where(h => h.StrokesAdded.Any()).ToList();
            var strokesAdded = historyActions.SelectMany(h => h.StrokesAdded).ToList();
            var orderedStrokes = strokesAdded.Where(strokes.Contains).ToList();

            return orderedStrokes;
        }

        public static Point GetAverageStrokeDimensions(CLPPage page)
        {
            var strokes = page.InkStrokes.ToList().Concat(page.History.TrashedInkStrokes.ToList()).ToList();
            if (!strokes.Any())
            {
                return new Point(0.0, 0.0);
            }
            var averageWidth = strokes.Average(s => s.GetBounds().Width);
            var averageHeight = strokes.Average(s => s.GetBounds().Height);
            return new Point(averageWidth, averageHeight);
        }

        public static double GetAverageClosestStrokeDistance(CLPPage page)
        {
            var strokes = page.InkStrokes.ToList().Concat(page.History.TrashedInkStrokes.ToList()).ToList();
            if (strokes.Count <= 1)
            {
                return 0.0;
            }
            var allClosestDistances = (from stroke in strokes
                                       let centroid = stroke.WeightedCenter()
                                       select (from otherStroke in strokes
                                               where stroke.GetStrokeID() != otherStroke.GetStrokeID()
                                               select otherStroke.WeightedCenter()
                                               into otherCentroid
                                               select DistanceSquaredBetweenPoints(centroid, otherCentroid)).Min()
                                       into closestDistanceSquared
                                       select Math.Sqrt(closestDistanceSquared)).ToList();

            return allClosestDistances.Average();
        }

        public static double GetStandardDeviationOfClosestStrokeDistance(CLPPage page)
        {
            var strokes = page.InkStrokes.ToList().Concat(page.History.TrashedInkStrokes.ToList()).ToList();
            if (strokes.Count <= 1)
            {
                return 0.0;
            }
            var allClosestDistances = (from stroke in strokes
                                       let centroid = stroke.WeightedCenter()
                                       select (from otherStroke in strokes
                                               where stroke.GetStrokeID() != otherStroke.GetStrokeID()
                                               select otherStroke.WeightedCenter()
                                               into otherCentroid
                                               select DistanceSquaredBetweenPoints(centroid, otherCentroid)).Min()
                                       into closestDistanceSquared
                                       select Math.Sqrt(closestDistanceSquared)).ToList();

            var averageDistance = allClosestDistances.Average();
            var standardDeviation = Math.Sqrt(allClosestDistances.Average(x => Math.Pow(x - averageDistance, 2)));

            return standardDeviation;
        }

        public static IPageObject FindMostOverlappedPageObjectAtHistoryIndex(CLPPage page, List<IPageObject> pageObjects, Stroke stroke, int historyIndex)
        {
            IPageObject mostOverlappedPageObject = null;

            foreach (var pageObject in pageObjects)
            {
                var percentOfStrokeOverlap = PercentageOfStrokeOverPageObjectAtHistoryIndex(page, pageObject, stroke, historyIndex);
                if (percentOfStrokeOverlap < 20.0) // Stroke not overlapping if only 20 percent of the stroke is on top of a pageObject.
                {
                    continue;
                }

                if (pageObject is InterpretationRegion &&
                    percentOfStrokeOverlap > 95.0)
                {
                    return pageObject;
                }

                if (mostOverlappedPageObject == null)
                {
                    mostOverlappedPageObject = pageObject;
                    continue;
                }

                var mostOverlappedPercentOfStrokeOverlap = PercentageOfStrokeOverPageObjectAtHistoryIndex(page, mostOverlappedPageObject, stroke, historyIndex);
                if (percentOfStrokeOverlap > mostOverlappedPercentOfStrokeOverlap)
                {
                    mostOverlappedPageObject = pageObject;
                }

                if (Math.Abs(percentOfStrokeOverlap - mostOverlappedPercentOfStrokeOverlap) < 0.01 &&
                    pageObject is InterpretationRegion)
                {
                    mostOverlappedPageObject = pageObject;
                }
            }

            return mostOverlappedPageObject;
        }

        public static double PercentageOfStrokeOverPageObjectAtHistoryIndex(CLPPage page, IPageObject pageObject, Stroke stroke, int historyIndex)
        {
            var position = pageObject.GetPositionAtHistoryIndex(historyIndex);
            var dimensions = pageObject.GetDimensionsAtHistoryIndex(historyIndex);
            var bounds = new Rect(position.X, position.Y, dimensions.X, dimensions.Y);
            var array = pageObject as ACLPArrayBase;
            if (array != null)
            {
                bounds = new Rect(position.X + array.LabelLength, position.Y + array.LabelLength, dimensions.X - (2 * array.LabelLength), dimensions.Y - (2 * array.LabelLength));
            }
            // HACK: make the above more abstract

            var strokeCopy = stroke.GetStrokeCopyAtHistoryIndex(page, historyIndex);

            return strokeCopy.PercentContainedByBounds(bounds) * 100;
        }

        public static IPageObject FindClosestPageObjectAtHistoryIndex(CLPPage page, List<IPageObject> pageObjects, Stroke stroke, int historyIndex)
        {
            IPageObject closestPageObject = null;

            foreach (var pageObject in pageObjects)
            {
                if (closestPageObject == null)
                {
                    closestPageObject = pageObject;
                    continue;
                }

                var distanceSquared = DistanceSquaredFromStrokeToPageObjectAtHistoryIndex(page, pageObject, stroke, historyIndex);

                var closestDistanceSquared = DistanceSquaredFromStrokeToPageObjectAtHistoryIndex(page, closestPageObject, stroke, historyIndex);
                if (distanceSquared < closestDistanceSquared)
                {
                    closestPageObject = pageObject;
                }
            }

            return closestPageObject;
        }

        public static IPageObject FindClosestPageObjectByPointAtHistoryIndex(CLPPage page, List<IPageObject> pageObjects, Point point, int historyIndex)
        {
            IPageObject closestPageObject = null;

            foreach (var pageObject in pageObjects)
            {
                if (closestPageObject == null)
                {
                    closestPageObject = pageObject;
                    continue;
                }

                var distanceSquared = DistanceSquaredFromPointToPageObjectAtHistoryIndex(page, pageObject, point, historyIndex);

                var closestDistanceSquared = DistanceSquaredFromPointToPageObjectAtHistoryIndex(page, closestPageObject, point, historyIndex);
                if (distanceSquared < closestDistanceSquared)
                {
                    closestPageObject = pageObject;
                }
            }

            return closestPageObject;
        }

        // DistanceSquared is used here for performance purposes. You get the same comparison results as normal distance and
        // Math.Sqrt() is really (relatively) slow. Math.Sqrt() can still be called on this result when needed.
        public static double DistanceSquaredFromStrokeToPageObjectAtHistoryIndex(CLPPage page, IPageObject pageObject, Stroke stroke, int historyIndex)
        {
            var position = pageObject.GetPositionAtHistoryIndex(historyIndex);
            var dimensions = pageObject.GetDimensionsAtHistoryIndex(historyIndex);
            var midX = position.X + (dimensions.X / 2.0);
            var midY = position.Y + (dimensions.Y / 2.0);

            var strokeCopy = stroke.GetStrokeCopyAtHistoryIndex(page, historyIndex);
            var strokeCentroid = strokeCopy.WeightedCenter();

            return DistanceSquaredBetweenPoints(strokeCentroid, new Point(midX, midY));
        }

        public static double DistanceSquaredFromPointToPageObjectAtHistoryIndex(CLPPage page, IPageObject pageObject, Point point, int historyIndex)
        {
            var position = pageObject.GetPositionAtHistoryIndex(historyIndex);
            var dimensions = pageObject.GetDimensionsAtHistoryIndex(historyIndex);
            var midX = position.X + (dimensions.X / 2.0);
            var midY = position.Y + (dimensions.Y / 2.0);

            return DistanceSquaredBetweenPoints(point, new Point(midX, midY));
        }

        public static double DistanceSquaredBetweenPoints(Point p1, Point p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            var distanceSquared = (dx * dx) + (dy * dy); // Again, for performance purposes, multiplication is used here instead of Math.Pow(). 20x performance boost.

            return distanceSquared;
        }

        public static string FindLocationReferenceAtHistoryLocation(CLPPage page, IPageObject pageObject, Point point, int historyIndex)
        {
            var position = pageObject.GetPositionAtHistoryIndex(historyIndex);
            var dimensions = pageObject.GetDimensionsAtHistoryIndex(historyIndex);

            if (point.X >= position.X + dimensions.X)
            {
                if (point.Y < position.Y)
                {
                    return Codings.EVENT_INFO_INK_LOCATION_TOP_RIGHT;
                }
                if (point.Y > position.Y + dimensions.Y)
                {
                    return Codings.EVENT_INFO_INK_LOCATION_BOTTOM_RIGHT;
                }
                return Codings.EVENT_INFO_INK_LOCATION_RIGHT;
            }

            if (point.X <= position.X)
            {
                if (point.Y < position.Y)
                {
                    return Codings.EVENT_INFO_INK_LOCATION_TOP_LEFT;
                }
                if (point.Y > position.Y + dimensions.Y)
                {
                    return Codings.EVENT_INFO_INK_LOCATION_BOTTOM_LEFT;
                }
                return Codings.EVENT_INFO_INK_LOCATION_LEFT;
            }

            if (point.Y <= position.Y)
            {
                return Codings.EVENT_INFO_INK_LOCATION_TOP;
            }
            if (point.Y >= position.Y + dimensions.Y)
            {
                return Codings.EVENT_INFO_INK_LOCATION_BOTTOM;
            }

            // Point is over the pageObject to some degree, use polar coords.
            var midX = position.X + (dimensions.X / 2.0);
            var midY = position.Y + (dimensions.Y / 2.0);

            var dx = point.X - midX;
            var dy = midY - point.Y; // swapped because page's Y coords run opposite of standard.
            var centroidArcFromMid = Math.Atan2(dy, dx);
            var topRightArc = Math.Atan2(dimensions.Y / 2, dimensions.X / 2);
            var topLeftArc = Math.Atan2(dimensions.Y / 2, -dimensions.X / 2);
            var bottomLeftArc = Math.Atan2(-dimensions.Y / 2, -dimensions.X / 2);
            var bottomRightArc = Math.Atan2(-dimensions.Y / 2, dimensions.X / 2);

            if (centroidArcFromMid >= 0 &&
                centroidArcFromMid <= topRightArc)
            {
                return Codings.EVENT_INFO_INK_LOCATION_RIGHT;
            }

            if (centroidArcFromMid >= bottomRightArc)
            {
                return Codings.EVENT_INFO_INK_LOCATION_RIGHT;
            }

            if (centroidArcFromMid >= topLeftArc &&
                centroidArcFromMid <= bottomLeftArc)
            {
                return Codings.EVENT_INFO_INK_LOCATION_LEFT;
            }

            if (centroidArcFromMid > topRightArc &&
                centroidArcFromMid < topLeftArc)
            {
                return Codings.EVENT_INFO_INK_LOCATION_TOP;
            }

            if (centroidArcFromMid > bottomLeftArc &&
                centroidArcFromMid < bottomRightArc)
            {
                return Codings.EVENT_INFO_INK_LOCATION_BOTTOM;
            }

            return Codings.EVENT_INFO_INK_LOCATION_NONE;
        }

        #endregion // Utility
    }
}