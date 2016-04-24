using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using CLP.InkInterpretation;
using CLP.MachineAnalysis;

namespace CLP.Entities
{
    public class InkCluster
    {
        public enum ClusterTypes
        {
            Ignore,
            InkDivide,
            Unknown,
            ANS_FI,
            PossibleARITH,
            ARITH,
            PossibleARReqn,
            ARReqn,
            ARRskip
        }

        public InkCluster(StrokeCollection strokes)
        {
            Strokes = strokes;
            ClusterName = string.Empty;
            ClusterType = ClusterTypes.Unknown;
            PageObjectReferenceID = string.Empty;
            LocationReference = Codings.ACTIONID_INK_LOCATION_NONE;
        }

        public StrokeCollection Strokes { get; set; }
        public string ClusterName { get; set; }
        public ClusterTypes ClusterType { get; set; }
        public string PageObjectReferenceID { get; set; }
        public string LocationReference { get; set; }

        public List<Stroke> GetStrokesOnPageAtHistoryIndex(CLPPage page, int historyIndex) { return page.GetStrokesOnPageAtHistoryIndex(historyIndex).Where(s => Strokes.Contains(s)).ToList(); }
    }

    public static class InkCodedActions
    {
        #region Initialization

        public static IHistoryAction ChangeOrIgnore(CLPPage page, List<ObjectsOnPageChangedHistoryItem> objectsOnPageChangedHistoryItems, bool isChange = true)
        {
            if (page == null ||
                objectsOnPageChangedHistoryItems == null ||
                !objectsOnPageChangedHistoryItems.Any() ||
                !objectsOnPageChangedHistoryItems.All(h => h.IsUsingStrokes && !h.IsUsingPageObjects))
            {
                return null;
            }

            var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItems.Cast<IHistoryItem>().ToList())
            {
                CodedObject = Codings.OBJECT_INK,
                CodedObjectAction = isChange ? Codings.ACTION_INK_CHANGE : Codings.ACTION_INK_IGNORE
            };

            return historyAction;
        }

        #endregion // Initialization

        #region Clustering

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
                cluster = new InkCluster(new StrokeCollection())
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

        public static void GenerateInitialInkClusters(CLPPage page, List<IHistoryAction> historyActions)
        {
            var inkActions = historyActions.Where(h => h.CodedObject == Codings.OBJECT_INK && h.CodedObjectAction == Codings.ACTION_INK_CHANGE).ToList();
            var historyItems = inkActions.SelectMany(h => h.HistoryItems).OfType<ObjectsOnPageChangedHistoryItem>().ToList();
            var strokesAdded = historyItems.SelectMany(i => i.StrokesAdded).ToList();
            var unclusteredStrokes = new StrokeCollection();
            var smallStrokes = strokesAdded.Where(s => s.IsInvisiblySmall()).ToList();
            foreach (var smallStroke in smallStrokes)
            {
                strokesAdded.Remove(smallStroke);
                unclusteredStrokes.Add(smallStroke);
            }
            var strokeClusters = new List<StrokeCollection>();

            if (strokesAdded.Count > 1)
            {
                var maxEpsilon = 1000;
                var minimumStrokesInCluster = 1;
                Func<Stroke, Stroke, double> distanceEquation = (s1, s2) => Math.Sqrt(s1.DistanceSquaredByClosestPoint(s2));
                var optics = new OPTICS<Stroke>(maxEpsilon, minimumStrokesInCluster, strokesAdded, distanceEquation);
                optics.BuildReachability();
                var reachabilityDistances = optics.ReachabilityDistances().ToList();

                #region Cluster by K-Means

                //var normalizedReachabilityPlot = reachabilityDistances.Select(i => new Point(0, i.ReachabilityDistance)).Skip(1).ToList();
                //var rawData = new double[normalizedReachabilityPlot.Count][];
                //for (var i = 0; i < rawData.Length; i++)
                //{
                //    rawData[i] = new[] { 0.0, normalizedReachabilityPlot[i].Y };
                //}

                //var clustering = InkClustering.K_MEANS_Clustering(rawData, 2);

                //var zeroCount = 0;
                //var zeroTotal = 0.0;
                //var oneCount = 0;
                //var oneTotal = 0.0;
                //for (var i = 0; i < clustering.Length; i++)
                //{
                //    if (clustering[i] == 0)
                //    {
                //        zeroCount++;
                //        zeroTotal += normalizedReachabilityPlot[i].Y;
                //    }
                //    if (clustering[i] == 1)
                //    {
                //        oneCount++;
                //        oneTotal += normalizedReachabilityPlot[i].Y;
                //    }
                //}
                //var zeroMean = zeroTotal / zeroCount;
                //var oneMean = oneTotal / oneCount;
                //var clusterWithHighestMean = zeroMean > oneMean ? 0 : 1;

                #endregion // Cluster by K-Means

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

                    // K-Means cluster decision.
                    //if (clustering[i - 1] != clusterWithHighestMean)
                    //{
                    //    currentCluster.Add(stroke);
                    //    allClusteredStrokes.Add(stroke);
                    //    continue;
                    //}

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
            else if (strokesAdded.Count == 1)
            {
                strokeClusters.Add(new StrokeCollection(strokesAdded));
            }

            var ignoredCluster = new InkCluster(unclusteredStrokes)
            {
                ClusterName = "IGNORED",
                ClusterType = InkCluster.ClusterTypes.Ignore
            };

            InkClusters.Add(ignoredCluster);

            foreach (var strokeCluster in strokeClusters)
            {
                InkClusters.Add(new InkCluster(strokeCluster));
            }

            //Console.WriteLine("Num of Clusters: {0}", InkClusters.Count);
            //Console.WriteLine("Num of Strokes in IGNORED: {0}", ignoredCluster.Strokes.Count);

            // HACK: INK ignore testing
            //foreach (var stroke in ignoredCluster.Strokes)
            //{
            //    stroke.DrawingAttributes.Width = 12;
            //    stroke.DrawingAttributes.Height = 12;
            //    stroke.DrawingAttributes.Color = Colors.DarkCyan;
            //}
        }

        public static List<IHistoryAction> RefineInkDivideClusters(CLPPage page, List<IHistoryAction> historyActions)
        {
            var allRefinedActions = new List<IHistoryAction>();

            foreach (var historyAction in historyActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK &&
                    historyAction.CodedObjectAction == Codings.ACTION_INK_CHANGE)
                {
                    var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
                    var refinedInkActions = new List<IHistoryAction>();
                    var historyItemBuffer = new List<IHistoryItem>();

                    foreach (var currentHistoryItem in historyItems)
                    {
                        var inkDivideAction = ArrayCodedActions.InkDivide(page, currentHistoryItem);
                        if (inkDivideAction != null)
                        {
                            if (historyItemBuffer.Any())
                            {
                                var bufferedHistoryAction = new HistoryAction(page, historyItemBuffer)
                                                            {
                                                                CodedObject = Codings.OBJECT_INK,
                                                                CodedObjectAction = Codings.ACTION_INK_CHANGE
                                                            };
                                refinedInkActions.Add(bufferedHistoryAction);
                                historyItemBuffer.Clear();
                            }

                            refinedInkActions.Add(inkDivideAction);
                            continue;
                        }

                        historyItemBuffer.Add(currentHistoryItem);
                    }

                    if (historyItemBuffer.Any())
                    {
                        var bufferedHistoryAction = new HistoryAction(page, historyItemBuffer)
                                                    {
                                                        CodedObject = Codings.OBJECT_INK,
                                                        CodedObjectAction = Codings.ACTION_INK_CHANGE
                                                    };
                        refinedInkActions.Add(bufferedHistoryAction);
                        historyItemBuffer.Clear();
                    }

                    allRefinedActions.AddRange(refinedInkActions);
                }
                else
                {
                    allRefinedActions.Add(historyAction);
                }
            }

            return allRefinedActions;
        }

        public static List<IHistoryAction> RefineANS_FIClusters(CLPPage page, List<IHistoryAction> historyActions)
        {
            var interpretationRegion = page.PageObjects.FirstOrDefault(p => p is InterpretationRegion) as InterpretationRegion;
            if (interpretationRegion == null)
            {
                return historyActions;
            }

            var ansCluster = InkClusters.FirstOrDefault(c => c.ClusterType == InkCluster.ClusterTypes.ANS_FI);
            if (ansCluster == null)
            {
                ansCluster = new InkCluster(new StrokeCollection())
                             {
                                 ClusterType = InkCluster.ClusterTypes.ANS_FI,
                                 LocationReference = Codings.ACTIONID_INK_LOCATION_OVER,
                                 PageObjectReferenceID = interpretationRegion.ID
                             };

                InkClusters.Add(ansCluster);
            }

            var allRefinedActions = new List<IHistoryAction>();

            foreach (var historyAction in historyActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK &&
                    historyAction.CodedObjectAction == Codings.ACTION_INK_CHANGE)
                {
                    var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
                    var historyItemBuffer = new List<IHistoryItem>();

                    var isPreviousInkAdd = false;
                    var isPreviousStrokeOverInterpretationRegion = false;

                    var firstHistoryItem = historyItems.First();
                    historyItemBuffer.Add(firstHistoryItem);
                    var previousStrokes = firstHistoryItem.StrokesAdded;
                    isPreviousInkAdd = true;
                    if (!previousStrokes.Any())
                    {
                        previousStrokes = firstHistoryItem.StrokesRemoved;
                        isPreviousInkAdd = false;
                    }

                    var previousStroke = previousStrokes.First();
                    var isPreviousStrokeInvisiblySmall = previousStroke.IsInvisiblySmall();
                    if (!isPreviousStrokeInvisiblySmall)
                    {
                        var percentOfPreviousStrokeOverlap = PercentageOfStrokeOverPageObjectAtHistoryIndex(page, interpretationRegion, previousStroke, firstHistoryItem.HistoryIndex);
                        if (percentOfPreviousStrokeOverlap > 95.0)
                        {
                            isPreviousStrokeOverInterpretationRegion = true;
                        }
                    }

                    for (var i = 1; i < historyItems.Count; i++)
                    {
                        var currentHistoryItem = historyItems[i];
                        var currentStrokes = currentHistoryItem.StrokesAdded;
                        var isCurrentInkAdd = true;
                        if (!currentStrokes.Any())
                        {
                            currentStrokes = currentHistoryItem.StrokesRemoved;
                            isCurrentInkAdd = false;
                        }

                        var currentStroke = currentStrokes.First();
                        var isCurrentStrokeInvisiblySmall = currentStroke.IsInvisiblySmall();
                        var percentOfCurrentStrokeOverlap = PercentageOfStrokeOverPageObjectAtHistoryIndex(page, interpretationRegion, currentStroke, currentHistoryItem.HistoryIndex);
                        var isCurrentStrokeOverInterpretationRegion = percentOfCurrentStrokeOverlap > 95.0 && !isCurrentStrokeInvisiblySmall;

                        var isBreakCondition = isCurrentStrokeOverInterpretationRegion != isPreviousStrokeOverInterpretationRegion ||
                                               (isPreviousStrokeOverInterpretationRegion && isCurrentInkAdd != isPreviousInkAdd);

                        if (isBreakCondition)
                        {
                            if (isPreviousStrokeOverInterpretationRegion)
                            {
                                var strokes = historyItemBuffer.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => isPreviousInkAdd ? h.StrokesAdded : h.StrokesRemoved).ToList();
                                foreach (var stroke in strokes)
                                {
                                    MoveStrokeToDifferentCluster(ansCluster, stroke);
                                }

                                var orderedStrokes = GetOrderStrokesWereAddedToPage(page, strokes);
                                var orderedStrokesOnPage = GetOrderStrokesWereAddedToPage(page, ansCluster.GetStrokesOnPageAtHistoryIndex(page, currentHistoryItem.HistoryIndex - 1));

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

                                var fillInAction = new HistoryAction(page, historyItemBuffer)
                                                   {
                                                       CodedObject = Codings.OBJECT_FILL_IN,
                                                       CodedObjectAction = isPreviousInkAdd ? Codings.ACTION_FILL_IN_ADD : Codings.ACTION_FILL_IN_ERASE,
                                                       IsObjectActionVisible = !isPreviousInkAdd,
                                                       CodedObjectID = answer,
                                                       CodedObjectActionID = string.Format("\"{0}\"; \"{1}\", {2}", interpretation, interpretationOnPage, correctness)
                                                   };

                                allRefinedActions.Add(fillInAction);
                                historyItemBuffer.Clear();
                            }
                            else
                            {
                                var inkChangeAction = new HistoryAction(page, historyItemBuffer)
                                                      {
                                                          CodedObject = Codings.OBJECT_INK,
                                                          CodedObjectAction = Codings.ACTION_INK_CHANGE
                                                      };

                                allRefinedActions.Add(inkChangeAction);
                                historyItemBuffer.Clear();
                            }
                        }

                        historyItemBuffer.Add(currentHistoryItem);
                        isPreviousInkAdd = isCurrentInkAdd;
                        isPreviousStrokeOverInterpretationRegion = isCurrentStrokeOverInterpretationRegion;
                    }

                    if (historyItemBuffer.Any())
                    {
                        if (isPreviousStrokeOverInterpretationRegion)
                        {
                            var strokes = historyItemBuffer.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => isPreviousInkAdd ? h.StrokesAdded : h.StrokesRemoved).ToList();
                            foreach (var stroke in strokes)
                            {
                                MoveStrokeToDifferentCluster(ansCluster, stroke);
                            }

                            var orderedStrokes = GetOrderStrokesWereAddedToPage(page, strokes);
                            var orderedStrokesOnPage = GetOrderStrokesWereAddedToPage(page, ansCluster.GetStrokesOnPageAtHistoryIndex(page, historyItems.Last().HistoryIndex));

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

                            var fillInAction = new HistoryAction(page, historyItemBuffer)
                            {
                                CodedObject = Codings.OBJECT_FILL_IN,
                                CodedObjectAction = isPreviousInkAdd ? Codings.ACTION_FILL_IN_ADD : Codings.ACTION_FILL_IN_ERASE,
                                IsObjectActionVisible = !isPreviousInkAdd,
                                CodedObjectID = answer,
                                CodedObjectActionID = string.Format("\"{0}\"; \"{1}\", {2}", interpretation, interpretationOnPage, correctness)
                            };

                            allRefinedActions.Add(fillInAction);
                            historyItemBuffer.Clear();
                        }
                        else
                        {
                            var inkChangeAction = new HistoryAction(page, historyItemBuffer)
                            {
                                CodedObject = Codings.OBJECT_INK,
                                CodedObjectAction = Codings.ACTION_INK_CHANGE
                            };

                            allRefinedActions.Add(inkChangeAction);
                            historyItemBuffer.Clear();
                        }
                    }
                }
                else
                {
                    allRefinedActions.Add(historyAction);
                }
            }

            return allRefinedActions;
        }

        public static void RefineSkipCountClusters(CLPPage page, List<IHistoryAction> historyActions)
        {
            if (!historyActions.Any())
            {
                return;
            }

            // This code looks for any of the following patterns for each array, where all other Event types are ignored:
            // ARR add, INK change, ARR delete/ARR move/End
            // ARR move, INK change, ARR delete/ARR move/End
            // When one of the above patterns is recognized, the start and end historyIndex and associated arrayID are stored in a list
            // of patternEndPoints. Every identified patternEndPoint will be analyzed at that point in history for skip counting
            // and any skip count strokes found at that point will be isolated in their own clusters.
            var patternStartPoints = new  Dictionary<string, string>();
            var patternEndPoints = new List<dynamic>();

            foreach (var currentHistoryAction in historyActions)
            {
                if (currentHistoryAction.CodedObject == Codings.OBJECT_ARRAY)
                {
                    var arrayID = currentHistoryAction.ReferencePageObjectID;

                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_OBJECT_ADD)
                    {
                        if (!patternStartPoints.Keys.Contains(arrayID))
                        {
                            var startPattern = string.Format("{0};{1}", Codings.ACTION_OBJECT_ADD, currentHistoryAction.HistoryItems.First().HistoryIndex);
                            patternStartPoints.Add(arrayID, startPattern);
                        }
                    }
                    else if (currentHistoryAction.CodedObjectAction == Codings.ACTION_OBJECT_MOVE)
                    {
                        var startPattern = string.Format("{0};{1}", Codings.ACTION_OBJECT_MOVE, currentHistoryAction.HistoryItems.First().HistoryIndex);
                        if (patternStartPoints.Keys.Contains(arrayID))
                        {
                            if (patternStartPoints[arrayID].Contains(Codings.ACTION_INK_CHANGE))
                            {
                                var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                                var endHistoryIndex = currentHistoryAction.HistoryItems.Last().HistoryIndex;
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
                    else if (currentHistoryAction.CodedObjectAction == Codings.ACTION_OBJECT_DELETE)
                    {
                        if (!patternStartPoints.Keys.Contains(arrayID))
                        {
                            continue;
                        }

                        if (patternStartPoints[arrayID].Contains(Codings.ACTION_INK_CHANGE))
                        {
                            var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                            var endHistoryIndex = currentHistoryAction.HistoryItems.Last().HistoryIndex;
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
                else if (currentHistoryAction.CodedObjectAction == Codings.ACTION_INK_CHANGE)
                {
                    var arrayIDs = patternStartPoints.Keys.ToList();
                    foreach (var arrayID in arrayIDs)
                    {
                        var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                        var startPattern = string.Format("{0};{1}", Codings.ACTION_INK_CHANGE, startHistoryIndex);
                        patternStartPoints[arrayID] = startPattern;
                    }
                }
            }

            foreach (var arrayID in patternStartPoints.Keys)
            {
                if (!patternStartPoints[arrayID].Contains(Codings.ACTION_INK_CHANGE))
                {
                    continue;
                }

                var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                var endHistoryIndex = historyActions.Last().HistoryItems.Last().HistoryIndex;
                patternEndPoints.Add(new
                                     {
                                         ArrayID = arrayID,
                                         StartHistoryIndex = startHistoryIndex,
                                         EndHistoryIndex = endHistoryIndex
                                     });
            }

            // Test for skip counting at each patternEndPoint. If it exists at the given point in history, move all skip 
            // count strokes from their current clusters into a new cluster for that historyIndex and tag as ARRskip cluster type.
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
                var strokeGroupPerRowHistory = ArrayCodedActions.GroupPossibleSkipCountStrokes(page, array, strokesAddedToPage, endHistoryIndex);
                var interpretedRowValuesHistory = ArrayCodedActions.InterpretSkipCountGroups(page, array, strokeGroupPerRowHistory, endHistoryIndex);
                var isSkipCountingHistory = ArrayCodedActions.IsSkipCounting(interpretedRowValuesHistory);

                var strokesOnPage = page.GetStrokesOnPageAtHistoryIndex(endHistoryIndex);
                var strokeGroupPerRowOnPage = ArrayCodedActions.GroupPossibleSkipCountStrokes(page, array, strokesOnPage, endHistoryIndex);
                var interpretedRowValuesOnPage = ArrayCodedActions.InterpretSkipCountGroups(page, array, strokeGroupPerRowOnPage, endHistoryIndex);
                var isSkipCountingOnPage = ArrayCodedActions.IsSkipCounting(interpretedRowValuesOnPage);

                if (!isSkipCountingHistory &&
                    !isSkipCountingOnPage)
                {
                    continue;
                }

                var skipCluster = new InkCluster(new StrokeCollection())
                                  {
                                      ClusterType = InkCluster.ClusterTypes.ARRskip,
                                      PageObjectReferenceID = arrayID,
                                      LocationReference = Codings.ACTIONID_INK_LOCATION_RIGHT_SKIP
                                  };
                InkClusters.Add(skipCluster);

                var skipStrokesHistory = strokeGroupPerRowHistory.Where(kv => kv.Key != 0 && kv.Key != -1).SelectMany(kv => kv.Value).Distinct().ToList();
                var skipStrokesOnPage = strokeGroupPerRowOnPage.Where(kv => kv.Key != 0 && kv.Key != -1).SelectMany(kv => kv.Value).Distinct().ToList();
                var skipStrokes = skipStrokesHistory.Concat(skipStrokesOnPage).Distinct().ToList();
                foreach (var stroke in skipStrokes)
                {
                    MoveStrokeToDifferentCluster(skipCluster, stroke);
                }

                // TODO: Test for skip counting along the bottom  here, give it it's own cluster as above, then continue below with ARReqn.

                var arrEqnCluster = new InkCluster(new StrokeCollection())
                                    {
                                        ClusterType = InkCluster.ClusterTypes.PossibleARReqn,
                                        PageObjectReferenceID = arrayID,
                                        LocationReference = Codings.ACTIONID_INK_LOCATION_OVER
                                    };
                InkClusters.Add(arrEqnCluster);

                var rejectedStrokesHistory = strokeGroupPerRowHistory.Where(kv => kv.Key == -1).SelectMany(kv => kv.Value).Distinct().ToList();
                var rejectedStrokesOnPage = strokeGroupPerRowOnPage.Where(kv => kv.Key == -1).SelectMany(kv => kv.Value).Distinct().ToList();
                var rejectedStrokes = rejectedStrokesHistory.Concat(rejectedStrokesOnPage).Distinct().Where(s => !skipStrokes.Contains(s)).ToList();
                var bounds = array.GetBoundsAtHistoryIndex(endHistoryIndex);
                foreach (var stroke in rejectedStrokes)
                {
                    var strokeCopy = stroke.GetStrokeCopyAtHistoryIndex(page, endHistoryIndex);
                    var isStrokeOverArray = strokeCopy.HitTest(bounds, 90);
                    if (isStrokeOverArray)
                    {
                        var currentCluster = GetContainingCluster(stroke);
                        if (currentCluster.ClusterType != InkCluster.ClusterTypes.InkDivide)
                        {
                            MoveStrokeToDifferentCluster(arrEqnCluster, stroke);
                        }
                    }
                }
            }
        }

        #endregion // Clustering

        /// <summary>Processes "INK change" action into "INK strokes (erase) [ID: location RefObject [RefObjectID]]" actions</summary>
        /// <param name="page">Parent page the history action belongs to.</param>
        /// <param name="historyAction">"INK change" history action to process</param>
        public static List<IHistoryAction> ProcessInkChangeHistoryAction(CLPPage page, IHistoryAction historyAction)
        {
            var processedInkActions = new List<IHistoryAction>();

            var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
            var historyItemBuffer = new List<IHistoryItem>();

            var firstHistoryItem = historyItems.First();
            historyItemBuffer.Add(firstHistoryItem);
            var previousStrokes = firstHistoryItem.StrokesAdded;
            var isPreviousInkAdd = true;
            if (!previousStrokes.Any())
            {
                previousStrokes = firstHistoryItem.StrokesRemoved;
                isPreviousInkAdd = false;
            }

            var previousStroke = previousStrokes.First();
            var previousClusterReference = GetContainingCluster(previousStroke);

            for (var i = 1; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                var currentStrokes = currentHistoryItem.StrokesAdded;
                var isCurrentInkAdd = true;
                if (!currentStrokes.Any())
                {
                    currentStrokes = currentHistoryItem.StrokesRemoved;
                    isCurrentInkAdd = false;
                }

                var currentStroke = currentStrokes.First();
                var currentClusterReference = GetContainingCluster(currentStroke);

                var isBreakCondition = isPreviousInkAdd != isCurrentInkAdd || previousClusterReference != currentClusterReference;

                if (isBreakCondition)
                {
                    if (previousClusterReference.ClusterType == InkCluster.ClusterTypes.Ignore)
                    {
                        var inkIgnoreAction = new HistoryAction(page, historyItemBuffer)
                                              {
                                                  CodedObject = Codings.OBJECT_INK,
                                                  CodedObjectAction = Codings.ACTION_INK_IGNORE
                                              };
                        processedInkActions.Add(inkIgnoreAction);
                    }
                    else
                    {
                        var processedInkAction = new HistoryAction(page, historyItemBuffer)
                                                 {
                                                     CodedObject = Codings.OBJECT_INK,
                                                     CodedObjectAction = isPreviousInkAdd ? Codings.ACTION_INK_ADD : Codings.ACTION_INK_ERASE
                                                 };

                        if (string.IsNullOrEmpty(previousClusterReference.ClusterName))
                        {
                            var numberOfNamedClusters = InkClusters.Count(c => !string.IsNullOrEmpty(c.ClusterName));
                            previousClusterReference.ClusterName = numberOfNamedClusters.ToLetter().ToUpper();
                        }
                        processedInkAction.CodedObjectID = previousClusterReference.ClusterName;

                        var previousHistoryIndex = currentHistoryItem.HistoryIndex - 1;
                        if (previousClusterReference.ClusterType == InkCluster.ClusterTypes.ARRskip)
                        {
                            var arrayID = previousClusterReference.PageObjectReferenceID;
                            var array = page.GetPageObjectByIDOnPageOrInHistory(arrayID);
                            if (array != null)
                            {
                                var locationReference = Codings.ACTIONID_INK_LOCATION_RIGHT_SKIP;
                                var codedObject = Codings.OBJECT_ARRAY;
                                var codedID = array.GetCodedIDAtHistoryIndex(previousHistoryIndex);

                                processedInkAction.CodedObjectActionID = string.Format("{0} {1} [{2}]", locationReference, codedObject, codedID);
                                processedInkAction.ReferencePageObjectID = arrayID;
                            }
                        }
                        else if (previousClusterReference.ClusterType == InkCluster.ClusterTypes.PossibleARReqn)
                        {
                            var arrayID = previousClusterReference.PageObjectReferenceID;
                            var array = page.GetPageObjectByIDOnPageOrInHistory(arrayID);
                            if (array != null)
                            {
                                var locationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                                var codedObject = Codings.OBJECT_ARRAY;
                                var codedID = array.GetCodedIDAtHistoryIndex(previousHistoryIndex);

                                processedInkAction.CodedObjectActionID = string.Format("{0} {1} [{2}]", locationReference, codedObject, codedID);
                                processedInkAction.ReferencePageObjectID = arrayID;
                            }
                        }
                        else
                        {
                            var pageObjectsOnPage = page.GetPageObjectsOnPageAtHistoryIndex(previousHistoryIndex);
                            var pageObjectReference = FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, previousStroke, previousHistoryIndex);
                            var locationReference = Codings.ACTIONID_INK_LOCATION_NONE;
                            if (pageObjectReference != null)
                            {
                                locationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                            }
                            else
                            {
                                var clusterCentroid = previousClusterReference.Strokes.WeightedCentroid();

                                pageObjectReference = FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, clusterCentroid, previousHistoryIndex);
                                if (pageObjectReference != null)
                                {
                                    locationReference = FindLocationReferenceAtHistoryLocation(page, pageObjectReference, clusterCentroid, previousHistoryIndex);
                                }
                            }

                            if (pageObjectReference != null)
                            {
                                var codedObject = pageObjectReference.CodedName;
                                var codedID = pageObjectReference.GetCodedIDAtHistoryIndex(previousHistoryIndex);
                                processedInkAction.CodedObjectActionID = string.Format("{0} {1} [{2}]", locationReference, codedObject, codedID);
                                processedInkAction.ReferencePageObjectID = pageObjectReference.ID;
                            }
                        }

                        processedInkActions.Add(processedInkAction);
                    }
                    
                    historyItemBuffer.Clear();
                }

                historyItemBuffer.Add(currentHistoryItem);
                isPreviousInkAdd = isCurrentInkAdd;
                previousStroke = currentStroke;
                previousClusterReference = currentClusterReference;
            }

            if (historyItemBuffer.Any())
            {
                if (previousClusterReference.ClusterType == InkCluster.ClusterTypes.Ignore)
                {
                    var inkIgnoreAction = new HistoryAction(page, historyItemBuffer)
                    {
                        CodedObject = Codings.OBJECT_INK,
                        CodedObjectAction = Codings.ACTION_INK_IGNORE
                    };
                    processedInkActions.Add(inkIgnoreAction);
                }
                else
                {
                    var processedInkAction = new HistoryAction(page, historyItemBuffer)
                    {
                        CodedObject = Codings.OBJECT_INK,
                        CodedObjectAction = isPreviousInkAdd ? Codings.ACTION_INK_ADD : Codings.ACTION_INK_ERASE
                    };

                    if (string.IsNullOrEmpty(previousClusterReference.ClusterName))
                    {
                        var numberOfNamedClusters = InkClusters.Count(c => !string.IsNullOrEmpty(c.ClusterName));
                        previousClusterReference.ClusterName = numberOfNamedClusters.ToLetter().ToUpper();
                    }
                    processedInkAction.CodedObjectID = previousClusterReference.ClusterName;

                    var previousHistoryIndex = historyItems.Last().HistoryIndex;
                    if (previousClusterReference.ClusterType == InkCluster.ClusterTypes.ARRskip)
                    {
                        var arrayID = previousClusterReference.PageObjectReferenceID;
                        var array = page.GetPageObjectByIDOnPageOrInHistory(arrayID);
                        if (array != null)
                        {
                            var locationReference = Codings.ACTIONID_INK_LOCATION_RIGHT_SKIP;
                            var codedObject = Codings.OBJECT_ARRAY;
                            var codedID = array.GetCodedIDAtHistoryIndex(previousHistoryIndex);

                            processedInkAction.CodedObjectActionID = string.Format("{0} {1} [{2}]", locationReference, codedObject, codedID);
                            processedInkAction.ReferencePageObjectID = arrayID;
                        }
                    }
                    else
                    {
                        var pageObjectsOnPage = page.GetPageObjectsOnPageAtHistoryIndex(previousHistoryIndex);
                        var pageObjectReference = FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, previousStroke, previousHistoryIndex);
                        var locationReference = Codings.ACTIONID_INK_LOCATION_NONE;
                        if (pageObjectReference != null)
                        {
                            locationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                        }
                        else
                        {
                            var clusterCentroid = previousClusterReference.Strokes.WeightedCentroid();

                            pageObjectReference = FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, clusterCentroid, previousHistoryIndex);
                            if (pageObjectReference != null)
                            {
                                locationReference = FindLocationReferenceAtHistoryLocation(page, pageObjectReference, clusterCentroid, previousHistoryIndex);
                            }
                        }

                        if (pageObjectReference != null)
                        {
                            var codedObject = pageObjectReference.CodedName;
                            var codedID = pageObjectReference.GetCodedIDAtHistoryIndex(previousHistoryIndex);
                            processedInkAction.CodedObjectActionID = string.Format("{0} {1} [{2}]", locationReference, codedObject, codedID);
                            processedInkAction.ReferencePageObjectID = pageObjectReference.ID;
                        }
                    }

                    processedInkActions.Add(processedInkAction);
                }
            }

            return processedInkActions;
        }

        public static IHistoryAction Arithmetic(CLPPage page, IHistoryAction inkAction)
        {
            if (page == null ||
                inkAction == null ||
                inkAction.CodedObject != Codings.OBJECT_INK ||
                !(inkAction.CodedObjectAction == Codings.ACTION_INK_ADD || inkAction.CodedObjectAction == Codings.ACTION_INK_ERASE))
            {
                return null;
            }

            var isArithAdd = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD;

            var strokes = isArithAdd
                              ? inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesRemoved).ToList();

            var firstStroke = strokes.First();
            var cluster = GetContainingCluster(firstStroke);
            if (cluster.ClusterType == InkCluster.ClusterTypes.PossibleARITH || 
                cluster.ClusterType == InkCluster.ClusterTypes.Unknown)
            {
                var interpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(strokes));
                if (interpretation == null ||
                    !isArithAdd)
                {
                    return null;
                }

                cluster.ClusterType = InkCluster.ClusterTypes.ARITH;

                var historyAction = new HistoryAction(page, inkAction)
                {
                    CodedObject = Codings.OBJECT_ARITH,
                    CodedObjectAction = isArithAdd ? Codings.ACTION_ARITH_ADD : Codings.ACTION_ARITH_ERASE,
                    IsObjectActionVisible = !isArithAdd,
                    CodedObjectID = inkAction.CodedObjectID,
                    CodedObjectActionID = string.Format("\"{0}\"", interpretation)
                };

                return historyAction;
            }

            if (cluster.ClusterType == InkCluster.ClusterTypes.ARITH)
            {
                List<string> interpretations;
                if (!isArithAdd)
                {
                    var orderedStrokes = GetOrderStrokesWereAddedToPage(page, strokes);
                    interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));
                }
                else
                {
                    interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(strokes));
                }

                var interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                var changedInterpretation = string.Format("\"{0}\"", interpretation);

                var strokesOnPage = cluster.GetStrokesOnPageAtHistoryIndex(page, inkAction.HistoryItems.Last().HistoryIndex);
                var onPageInterpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(strokesOnPage)) ?? string.Empty;
                onPageInterpretation = string.Format("\"{0}\"", onPageInterpretation);
                var formattedInterpretation = string.Format("{0}; {1}", changedInterpretation, onPageInterpretation);

                var historyAction = new HistoryAction(page, inkAction)
                {
                    CodedObject = Codings.OBJECT_ARITH,
                    CodedObjectAction = isArithAdd ? Codings.ACTION_ARITH_ADD : Codings.ACTION_ARITH_ERASE,
                    IsObjectActionVisible = !isArithAdd,
                    CodedObjectID = inkAction.CodedObjectID,
                    CodedObjectActionID = formattedInterpretation
                };

                return historyAction;
            }

            return null;
        }

        #region Utility Static Methods

        public static List<Stroke> GetOrderStrokesWereAddedToPage(CLPPage page, List<Stroke> strokes)
        {
            var historyItems = page.History.CompleteOrderedHistoryItems.OfType<ObjectsOnPageChangedHistoryItem>().Where(h => h.StrokesAdded.Any()).ToList();
            var strokesAdded = historyItems.SelectMany(h => h.StrokesAdded).ToList();
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
                    return Codings.ACTIONID_INK_LOCATION_TOP_RIGHT;
                }
                if (point.Y > position.Y + dimensions.Y)
                {
                    return Codings.ACTIONID_INK_LOCATION_BOTTOM_RIGHT;
                }
                return Codings.ACTIONID_INK_LOCATION_RIGHT;
            }

            if (point.X <= position.X)
            {
                if (point.Y < position.Y)
                {
                    return Codings.ACTIONID_INK_LOCATION_TOP_LEFT;
                }
                if (point.Y > position.Y + dimensions.Y)
                {
                    return Codings.ACTIONID_INK_LOCATION_BOTTOM_LEFT;
                }
                return Codings.ACTIONID_INK_LOCATION_LEFT;
            }

            if (point.Y <= position.Y)
            {
                return Codings.ACTIONID_INK_LOCATION_TOP;
            }
            if (point.Y >= position.Y + dimensions.Y)
            {
                return Codings.ACTIONID_INK_LOCATION_BOTTOM;
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
                return Codings.ACTIONID_INK_LOCATION_RIGHT;
            }

            if (centroidArcFromMid >= bottomRightArc)
            {
                return Codings.ACTIONID_INK_LOCATION_RIGHT;
            }

            if (centroidArcFromMid >= topLeftArc &&
                centroidArcFromMid <= bottomLeftArc)
            {
                return Codings.ACTIONID_INK_LOCATION_LEFT;
            }

            if (centroidArcFromMid > topRightArc &&
                centroidArcFromMid < topLeftArc)
            {
                return Codings.ACTIONID_INK_LOCATION_TOP;
            }

            if (centroidArcFromMid > bottomLeftArc &&
                centroidArcFromMid < bottomRightArc)
            {
                return Codings.ACTIONID_INK_LOCATION_BOTTOM;
            }

            return Codings.ACTIONID_INK_LOCATION_NONE;
        }

        #endregion // Utility Static Methods
    }
}