using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media.TextFormatting;
using CLP.InkInterpretation;
using CLP.MachineAnalysis;

namespace CLP.Entities
{
    public class InkCluster
    {
        public enum ClusterTypes
        {
            Ignore,
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
            StrokesOnPage = new StrokeCollection();
            StrokesErased = new StrokeCollection();
            ClusterName = string.Empty;
            ClusterType = ClusterTypes.Unknown;
            PageObjectReferenceID = string.Empty;
            LocationReference = Codings.ACTIONID_INK_LOCATION_NONE;
        }

        public StrokeCollection Strokes { get; set; }
        public StrokeCollection StrokesOnPage { get; set; }
        public StrokeCollection StrokesErased { get; set; }
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

        public static IHistoryAction StrokesAddOrErase(CLPPage page, List<ObjectsOnPageChangedHistoryItem> objectsOnPageChangedHistoryItems, bool isAdd = true)
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
                CodedObjectAction = isAdd ? Codings.ACTION_INK_ADD : Codings.ACTION_INK_ERASE
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
                    var percentOfPreviousStrokeOverlap = PercentageOfStrokeOverPageObjectAtHistoryIndex(page, interpretationRegion, previousStroke, firstHistoryItem.HistoryIndex);
                    if (percentOfPreviousStrokeOverlap > 95.0)
                    {
                        isPreviousStrokeOverInterpretationRegion = true;
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
                        var percentOfCurrentStrokeOverlap = PercentageOfStrokeOverPageObjectAtHistoryIndex(page, interpretationRegion, currentStroke, currentHistoryItem.HistoryIndex);
                        var isCurrentStrokeOverInterpretationRegion = percentOfCurrentStrokeOverlap > 95.0;

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

        public static List<IHistoryAction> RefineSkipCountClusters(CLPPage page, List<IHistoryAction> historyActions)
        {
            /*
            Look for historyAction patterns

            ARR add, INK change, ARR remove/End/ARR move
            ARR move, INK change, ARR remove/End/ARR move
            Record list of tuples/anonymous lambdas that store startHistoryIndex and endHistoryIndex and arrayID (probably only need endHistoryIndex)

            get array dimensions/size/location at historyIndex
            get strokes on page at historyIndex
            run through IsSkipCounting()

            if false do nothing with clusters?
            else all grouped strokes in single cluster, defined as ARRskip
                 all rejected subdivided into appropriate clusters
            */

            var testData = new[] { new
                                   {
                                       ArrayID = "blah",
                                       HistoryIndex = 3
                                   },
                                   new
                                   {
                                       ArrayID = "blarg",
                                       HistoryIndex = 7
                                   } }.ToList();

            foreach (var item in testData)
            {
                var arrayID = item.ArrayID;
                var historyIndex = item.HistoryIndex;

                var array = page.GetPageObjectByIDOnPageOrInHistory(arrayID) as CLPArray;
                if (array == null)
                {
                    continue;
                }


                var strokesOnPage = page.GetStrokesOnPageAtHistoryIndex(historyIndex);
                var strokeGroupPerRow = ArrayCodedActions.GroupPossibleSkipCountStrokes(page, array, strokesOnPage, historyIndex);




                var rejectedStrokes = strokeGroupPerRow[-1].ToList();
                if (strokeGroupPerRow.ContainsKey(0))
                {
                    rejectedStrokes = rejectedStrokes.Concat(strokeGroupPerRow[0]).Distinct().ToList();
                }
                var skipStrokes = strokeGroupPerRow.Where(kv => kv.Key != 0 || kv.Key != -1).SelectMany(kv => kv.Value).ToList();
               // var
            }

            //var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
            //var historyIndex = historyItems.First().HistoryIndex;

            //var arraysOnPage = page.GetPageObjectsOnPageAtHistoryIndex(historyIndex).OfType<CLPArray>().ToList();

            //var refinedInkActions = new List<IHistoryAction>();
            //var historyItemBuffer = new List<IHistoryItem>();

            //foreach (var currentHistoryItem in historyItems)
            //{
            //    var inkDivideAction = ArrayCodedActions.InkDivide(page, currentHistoryItem);
            //    if (inkDivideAction != null)
            //    {
            //        if (historyItemBuffer.Any())
            //        {
            //            var bufferedHistoryAction = new HistoryAction(page, historyItemBuffer)
            //            {
            //                CodedObject = Codings.OBJECT_INK,
            //                CodedObjectAction = Codings.ACTION_INK_CHANGE
            //            };
            //            refinedInkActions.Add(bufferedHistoryAction);
            //            historyItemBuffer.Clear();
            //        }

            //        refinedInkActions.Add(inkDivideAction);
            //        continue;
            //    }

            //    historyItemBuffer.Add(currentHistoryItem);
            //}

            //if (historyItemBuffer.Any())
            //{
            //    var bufferedHistoryAction = new HistoryAction(page, historyItemBuffer)
            //    {
            //        CodedObject = Codings.OBJECT_INK,
            //        CodedObjectAction = Codings.ACTION_INK_CHANGE
            //    };
            //    refinedInkActions.Add(bufferedHistoryAction);
            //    historyItemBuffer.Clear();
            //}

            //return refinedInkActions;

            return null;
        }

        #endregion // Clustering

        /// <summary>Processes "INK change" action into "INK strokes (erase) [ID: location RefObject [RefObjectID]]" actions</summary>
        /// <param name="page">Parent page the history action belongs to.</param>
        /// <param name="historyAction">"INK change" history action to process</param>
        public static List<IHistoryAction> ProcessInkChangeHistoryAction(CLPPage page, IHistoryAction historyAction)
        {
            var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
            var processedInkActions = new List<IHistoryAction>();
            var pageObjectsOnPage = page.GetPageObjectsOnPageAtHistoryIndex(historyItems.First().HistoryIndex);
            // TODO: validation

            var historyItemBuffer = new List<IHistoryItem>();
            var isCurrentInkAdd = true;
            Stroke currentStrokeReference = null;
            InkCluster currentClusterReference = null;
            IPageObject currentPageObjectReference = null;
            var currentLocationReference = Codings.ACTIONID_INK_LOCATION_NONE;

            for (var i = 0; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                var currentHistoryIndex = currentHistoryItem.HistoryIndex;
                historyItemBuffer.Add(currentHistoryItem);
                if (historyItemBuffer.Count == 1)
                {
                    var strokes = currentHistoryItem.StrokesAdded;
                    isCurrentInkAdd = true;
                    if (!strokes.Any())
                    {
                        strokes = currentHistoryItem.StrokesRemoved;
                        isCurrentInkAdd = false;
                    }

                    // TODO: If strokes.count != 1, deal with point erase
                    // TODO: Validation (strokes is empty)
                    currentStrokeReference = strokes.First();
                    currentClusterReference = GetContainingCluster(currentStrokeReference);
                    currentPageObjectReference = FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, currentStrokeReference, currentHistoryItem.HistoryIndex);
                    currentLocationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                    if (currentPageObjectReference == null)
                    {
                        var currentClusterCentroid = currentClusterReference.Strokes.WeightedCentroid();

                        currentPageObjectReference = FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, currentClusterCentroid, currentHistoryItem.HistoryIndex);
                        if (currentPageObjectReference != null)
                        {
                            currentLocationReference = FindLocationReferenceAtHistoryLocation(page, currentPageObjectReference, currentClusterCentroid, currentHistoryItem.HistoryIndex);
                        }
                    }

                    if (currentPageObjectReference is CLPArray &&
                        (currentLocationReference == Codings.ACTIONID_INK_LOCATION_OVER ||
                         currentLocationReference == Codings.ACTIONID_INK_LOCATION_RIGHT) &&
                        currentClusterReference.ClusterType == InkCluster.ClusterTypes.Unknown) // TODO: Deal with skip counting on other dimensions
                    {
                        var array = currentPageObjectReference as CLPArray;
                        
                        var strokesInCluster = currentClusterReference.Strokes.ToList();
                        var strokeGroupPerRow = ArrayCodedActions.GroupPossibleSkipCountStrokes(page, array, strokesInCluster, currentHistoryIndex);
                        var possibleSkipStrokes = strokeGroupPerRow.Where(kv => kv.Key != -1).SelectMany(kv => kv.Value).ToList();
                        var rejectedStrokes = strokeGroupPerRow.Where(kv => kv.Key == -1).SelectMany(kv => kv.Value).ToList();

                        var dimensions = array.GetDimensionsAtHistoryIndex(currentHistoryItem.HistoryIndex);
                        var position = array.GetPositionAtHistoryIndex(currentHistoryItem.HistoryIndex);
                        var arrayVisualRegion = new Rect(position.X + array.LabelLength, position.Y + array.LabelLength, dimensions.X - (2 * array.LabelLength), dimensions.Y - (2 * array.LabelLength));
                        var strokesOverArray = rejectedStrokes.Where(s => arrayVisualRegion.Contains(s.WeightedCenter())).ToList();
                        var otherStrokes = rejectedStrokes.Where(s => !arrayVisualRegion.Contains(s.WeightedCenter())).ToList();

                        var interpretedRowValues = ArrayCodedActions.InterpretSkipCountGroups(page, array, strokeGroupPerRow, currentHistoryIndex);
                        var isSkipCounting = ArrayCodedActions.IsSkipCounting(interpretedRowValues);

                        if (isSkipCounting)
                        {
                            if (possibleSkipStrokes.Contains(currentStrokeReference))
                            {
                                currentClusterReference.ClusterType = InkCluster.ClusterTypes.ARRskip;

                                foreach (var stroke in strokesOverArray)
                                {
                                    currentClusterReference.Strokes.Remove(stroke);
                                }

                                var arrayCluster = new InkCluster(new StrokeCollection(strokesOverArray))
                                                   {
                                                       ClusterType = InkCluster.ClusterTypes.PossibleARReqn
                                                   };

                                InkClusters.Add(arrayCluster);

                                foreach (var stroke in otherStrokes)
                                {
                                    currentClusterReference.Strokes.Remove(stroke);
                                }

                                var otherCluster = new InkCluster(new StrokeCollection(otherStrokes))
                                                   {
                                                       ClusterType = InkCluster.ClusterTypes.Unknown
                                                   };

                                InkClusters.Add(otherCluster);
                            }
                            else if (strokesOverArray.Contains(currentStrokeReference))
                            {
                                currentClusterReference.ClusterType = InkCluster.ClusterTypes.PossibleARReqn;

                                foreach (var stroke in possibleSkipStrokes)
                                {
                                    currentClusterReference.Strokes.Remove(stroke);
                                }

                                var skipCluster = new InkCluster(new StrokeCollection(possibleSkipStrokes))
                                                   {
                                                       ClusterType = InkCluster.ClusterTypes.ARRskip
                                                   };

                                InkClusters.Add(skipCluster);

                                foreach (var stroke in otherStrokes)
                                {
                                    currentClusterReference.Strokes.Remove(stroke);
                                }

                                var otherCluster = new InkCluster(new StrokeCollection(otherStrokes))
                                {
                                    ClusterType = InkCluster.ClusterTypes.Unknown
                                };

                                InkClusters.Add(otherCluster);
                            }
                            else if (otherStrokes.Contains(currentStrokeReference))
                            {
                                currentClusterReference.ClusterType = InkCluster.ClusterTypes.Unknown;

                                foreach (var stroke in possibleSkipStrokes)
                                {
                                    currentClusterReference.Strokes.Remove(stroke);
                                }

                                var skipCluster = new InkCluster(new StrokeCollection(possibleSkipStrokes))
                                {
                                    ClusterType = InkCluster.ClusterTypes.ARRskip
                                };

                                InkClusters.Add(skipCluster);

                                foreach (var stroke in strokesOverArray)
                                {
                                    currentClusterReference.Strokes.Remove(stroke);
                                }

                                var arrayCluster = new InkCluster(new StrokeCollection(strokesOverArray))
                                                   {
                                                       ClusterType = InkCluster.ClusterTypes.PossibleARReqn
                                                   };

                                InkClusters.Add(arrayCluster);
                            }
                        }
                    }
                }

                var nextHistoryItem = i + 1 < historyItems.Count ? historyItems[i + 1] : null;
                if (nextHistoryItem != null)
                {
                    var nextStrokes = nextHistoryItem.StrokesAdded;
                    var isNextInkAdd = true;
                    if (!nextStrokes.Any())
                    {
                        nextStrokes = nextHistoryItem.StrokesRemoved;
                        isNextInkAdd = false;
                    }

                    var nextStrokeReference = nextStrokes.First();
                    var nextClusterReference = GetContainingCluster(nextStrokeReference);
                    var nextPageObjectReference = FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, nextStrokeReference, nextHistoryItem.HistoryIndex);
                    var nextLocationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                    if (nextPageObjectReference == null)
                    {
                        var nextClusterCentroid = nextClusterReference.Strokes.WeightedCentroid();

                        nextPageObjectReference = FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, nextClusterCentroid, nextHistoryItem.HistoryIndex);
                        if (nextPageObjectReference != null)
                        {
                            nextLocationReference = FindLocationReferenceAtHistoryLocation(page, nextPageObjectReference, nextClusterCentroid, nextHistoryItem.HistoryIndex);
                        }
                    }

                    var isNextInkAddCurrentInkAdd = isCurrentInkAdd == isNextInkAdd;
                    var isNextStrokeCurrentStroke = nextStrokeReference.GetStrokeID() == currentStrokeReference.GetStrokeID();
                    var isNextPartOfCurrentCluster = currentClusterReference == nextClusterReference;
                    var isNextPageObjectReferencePartOfCurrent = nextPageObjectReference == null && currentPageObjectReference == null;
                    if (nextPageObjectReference != null &&
                        currentPageObjectReference != null)
                    {
                        isNextPageObjectReferencePartOfCurrent = nextPageObjectReference.ID == currentPageObjectReference.ID;
                    }
                    var isNextLocationReferencePartOfCurrent = nextLocationReference == currentLocationReference;

                    #region Fix Later

                    // TODO: Fix this overzealous detection of INK ignore
                    //if (isNextStrokeCurrentStroke &&
                    //    !isNextInkAddCurrentInkAdd &&
                    //    currentClusterReference.ClusterName != "IGNORED" &&
                    //    !(currentPageObjectReference is InterpretationRegion))
                    //{
                    //    historyItemBuffer.Remove(currentHistoryItem);
                    //    if (historyItemBuffer.Any())
                    //    {
                    //        if (string.IsNullOrEmpty(currentClusterReference.ClusterName))
                    //        {
                    //            var numberOfNamedClusters = InkClusters.Count(c => !string.IsNullOrEmpty(c.ClusterName));
                    //            currentClusterReference.ClusterName = numberOfNamedClusters.ToLetter().ToUpper();
                    //        }

                    //        var previousInkAction = GroupAddOrErase(page, historyItemBuffer.Cast<ObjectsOnPageChangedHistoryItem>().ToList(), isCurrentInkAdd);
                    //        previousInkAction.CodedObjectID = currentClusterReference.ClusterName;
                    //        if (currentPageObjectReference != null)
                    //        {
                    //            previousInkAction.CodedObjectActionID = string.Format("{0} {1} [{2}]",
                    //                                                                     currentLocationReference,
                    //                                                                     currentPageObjectReference.CodedName,
                    //                                                                     currentPageObjectReference.GetCodedIDAtHistoryIndex(previousInkAction.HistoryItems.Last().HistoryIndex));
                    //            previousInkAction.ReferencePageObjectID = currentPageObjectReference.ID;
                    //        }

                    //        processedInkActions.Add(previousInkAction);
                    //        historyItemBuffer.Clear();
                    //    }

                    //    var ignoreItems = new List<ObjectsOnPageChangedHistoryItem> { currentHistoryItem, nextHistoryItem };
                    //    var ignoreAction = ChangeOrIgnore(page, ignoreItems, false);
                    //    if (ignoreAction != null)
                    //    {
                    //        processedInkActions.Add(ignoreAction);
                    //    }
                    //    i++;
                    //    continue;
                    //}

                    #endregion // Fix Later

                    if (isNextInkAddCurrentInkAdd &&
                        isNextPartOfCurrentCluster &&
                        isNextPageObjectReferencePartOfCurrent &&
                        isNextLocationReferencePartOfCurrent)
                    {
                        currentStrokeReference = nextStrokeReference;
                        continue;
                    }
                }

                if (currentClusterReference.ClusterName == "IGNORED")
                {
                    var ignoreAction = ChangeOrIgnore(page, historyItemBuffer.Cast<ObjectsOnPageChangedHistoryItem>().ToList(), false);
                    processedInkActions.Add(ignoreAction);
                    historyItemBuffer.Clear();
                    continue;
                }

                if (string.IsNullOrEmpty(currentClusterReference.ClusterName))
                {
                    var numberOfNamedClusters = InkClusters.Count(c => !string.IsNullOrEmpty(c.ClusterName));
                    currentClusterReference.ClusterName = numberOfNamedClusters.ToLetter().ToUpper();
                }

                var refinedHistoryAction = StrokesAddOrErase(page, historyItemBuffer.Cast<ObjectsOnPageChangedHistoryItem>().ToList(), isCurrentInkAdd);
                refinedHistoryAction.CodedObjectID = currentClusterReference.ClusterName;
                if (currentPageObjectReference != null)
                {
                    refinedHistoryAction.CodedObjectActionID = string.Format("{0} {1} [{2}]",
                                                                             currentLocationReference,
                                                                             currentPageObjectReference.CodedName,
                                                                             currentPageObjectReference.GetCodedIDAtHistoryIndex(refinedHistoryAction.HistoryItems.Last().HistoryIndex));
                    refinedHistoryAction.ReferencePageObjectID = currentPageObjectReference.ID;
                }

                processedInkActions.Add(refinedHistoryAction);
                historyItemBuffer.Clear();
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

                foreach (var stroke in strokes)
                {
                    cluster.StrokesOnPage.Add(stroke);
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

                if (!isArithAdd)
                {
                    foreach (var stroke in strokes)
                    {
                        cluster.StrokesOnPage.Remove(stroke);
                        cluster.StrokesErased.Add(stroke);
                    }
                }
                else
                {
                    foreach (var stroke in strokes)
                    {
                        cluster.StrokesOnPage.Add(stroke);
                    }
                }

                var onPageInterpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(cluster.StrokesOnPage)) ?? string.Empty;
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

                // HACK: Temporarily in place until MC Boxes are re-written and converted.
                if (pageObject is MultipleChoiceBox &&
                    percentOfStrokeOverlap > 70.0)
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
                    (pageObject is InterpretationRegion || pageObject is MultipleChoiceBox)) // HACK: Temporarily in place until MC Boxes are re-written and converted.
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
            var midX = position.X + (dimensions.X / 2.0);
            var midY = position.Y + (dimensions.Y / 2.0);

            var dx = point.X - midX;
            var dy = midY - point.Y; // swapped because page's Y coords run opposite of standard.
            var centroidArcFromMid = Math.Atan2(dy, dx);
            var topRightArc = Math.Atan2(dimensions.Y / 2, dimensions.X / 2);
            var topLeftArc = Math.Atan2(dimensions.Y / 2, -dimensions.X / 2);
            var bottomLeftArc = Math.Atan2(-dimensions.Y / 2, -dimensions.X / 2);
            var bottomRightArc = Math.Atan2(-dimensions.Y / 2, dimensions.X / 2);

            var locationReference = Codings.ACTIONID_INK_LOCATION_NONE;
            if (centroidArcFromMid >= 0 &&
                centroidArcFromMid <= topRightArc)
            {
                locationReference = Codings.ACTIONID_INK_LOCATION_RIGHT;
            }
            else if (centroidArcFromMid >= bottomRightArc)
            {
                locationReference = Codings.ACTIONID_INK_LOCATION_RIGHT;
            }
            else if (centroidArcFromMid >= topLeftArc &&
                     centroidArcFromMid <= bottomLeftArc)
            {
                locationReference = Codings.ACTIONID_INK_LOCATION_LEFT;
            }
            else if (centroidArcFromMid > topRightArc &&
                     centroidArcFromMid < topLeftArc)
            {
                locationReference = Codings.ACTIONID_INK_LOCATION_TOP;
            }
            else if (centroidArcFromMid > bottomLeftArc &&
                     centroidArcFromMid < bottomRightArc)
            {
                locationReference = Codings.ACTIONID_INK_LOCATION_BOTTOM;
            }

            return locationReference;
        }

        #endregion // Utility Static Methods
    }
}