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
        public InkCluster(StrokeCollection strokes)
        {
            Strokes = strokes;
            ClusterNumber = NumberOfClusters;
            NumberOfClusters++;
        }

        public StrokeCollection Strokes { get; set; }
        public int ClusterNumber { get; set; }
        public string ClusterName { get; set; }

        public static int NumberOfClusters = 0;
    }

    public static class InkCodedActions
    {
        #region Verify And Generate Methods

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

        public static IHistoryAction GroupAddOrErase(CLPPage page, List<ObjectsOnPageChangedHistoryItem> objectsOnPageChangedHistoryItems, bool isAdd = true)
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

        public static readonly List<InkCluster> InkClusters = new List<InkCluster>(); 

        public static void GenerateInitialClusterings(CLPPage page, List<IHistoryAction> historyActions)
        {
            InkClusters.Clear();
            InkCluster.NumberOfClusters = 0;
            var inkActions = historyActions.Where(h => h.CodedObjectAction == Codings.OBJECT_INK && h.CodedObjectAction == Codings.ACTION_INK_CHANGE).ToList();
            var historyItems = inkActions.SelectMany(h => h.HistoryItems).OfType<ObjectsOnPageChangedHistoryItem>().ToList();
            var strokesAdded = historyItems.SelectMany(i => i.StrokesAdded).ToList();

            var maxEpsilon = 1000;
            var minimumStrokesInCluster = 2;
            Func<Stroke, Stroke, double> distanceEquation = (s1, s2) => Math.Sqrt(s1.DistanceSquaredByClosestPoint(s2));
            var optics = new OPTICS<Stroke>(maxEpsilon, minimumStrokesInCluster, strokesAdded, distanceEquation);
            optics.BuildReachability();
            var reachabilityDistances = optics.ReachabilityDistances().ToList();

            var normalizedReachabilityPlot = reachabilityDistances.Select(i => new Point(0, i.ReachabilityDistance)).Skip(1).ToList();
            var rawData = new double[normalizedReachabilityPlot.Count][];
            for (var i = 0; i < rawData.Length; i++)
            {
                rawData[i] = new[] { 0.0, normalizedReachabilityPlot[i].Y };
            }

            var clustering = InkClustering.K_MEANS_Clustering(rawData, 2);

            var zeroCount = 0;
            var zeroTotal = 0.0;
            var oneCount = 0;
            var oneTotal = 0.0;
            for (var i = 0; i < clustering.Length; i++)
            {
                if (clustering[i] == 0)
                {
                    zeroCount++;
                    zeroTotal += normalizedReachabilityPlot[i].Y;
                }
                if (clustering[i] == 1)
                {
                    oneCount++;
                    oneTotal += normalizedReachabilityPlot[i].Y;
                }
            }
            var zeroMean = zeroTotal / zeroCount;
            var oneMean = oneTotal / oneCount;
            var clusterWithHighestMean = zeroMean > oneMean ? 0 : 1;
            var currentCluster = new StrokeCollection();
            var allClusteredStrokes = new List<Stroke>();
            var firstStrokeIndex = (int)reachabilityDistances[0].OriginalIndex;
            var firstStroke = strokesAdded[firstStrokeIndex];
            currentCluster.Add(firstStroke);
            var strokeClusters = new List<StrokeCollection>();
            for (var i = 1; i < reachabilityDistances.Count(); i++)
            {
                var strokeIndex = (int)reachabilityDistances[i].OriginalIndex;
                var stroke = strokesAdded[strokeIndex];

                if (clustering[i - 1] != clusterWithHighestMean)
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

            var unclusteredStrokes = new StrokeCollection();
            foreach (var stroke in strokesAdded)
            {
                if (allClusteredStrokes.Contains(stroke))
                {
                    continue;
                }
                unclusteredStrokes.Add(stroke);
            }

            var ignoredCluster = new InkCluster(unclusteredStrokes);
            ignoredCluster.ClusterName = "IGNORED";

            InkClusters.Add(ignoredCluster);
            foreach (var strokeCluster in strokeClusters)
            {
                InkClusters.Add(new InkCluster(strokeCluster));
            }
        }

        /// <summary>Processes "INK change" action into "INK strokes (erase) [ID: location RefObject [RefObjectID]]" actions</summary>
        /// <param name="page">Parent page the history action belongs to.</param>
        /// <param name="historyAction">"INK change" history action to process</param>
        public static List<IHistoryAction> ProcessInkChangeHistoryAction(CLPPage page, IHistoryAction historyAction)
        {
            var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
            var processedInkActions = new List<IHistoryAction>();
            var pageObjectsOnPage = ObjectCodedActions.GetPageObjectsOnPageAtHistoryIndex(page, historyItems.First().HistoryIndex);
            // TODO: validation

            var averageStrokeDimensions = GetAverageStrokeDimensions(page);
            var averageClosestStrokeDistance = GetAverageClosestStrokeDistance(page);
            var closestStrokeDistanceStandardDeviation = GetStandardDeviationOfClosestStrokeDistance(page);
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
                    // First see if single stroke was an Ink Divide, if so, remove from clusters and create separate history event.
                    var inkDivideAction = ArrayCodedActions.AttemptInkDivide(page, currentHistoryItem);
                    if (inkDivideAction != null)
                    {
                        processedInkActions.Add(inkDivideAction);
                        historyItemBuffer.Clear();
                        continue;
                    }

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
                    currentPageObjectReference = FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, currentStrokeReference, currentHistoryItem.HistoryIndex);
                    currentLocationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                    isMatchingAgainstCluster = false;
                    if (currentPageObjectReference == null)
                    {
                        isMatchingAgainstCluster = true;

                        var strokeCopy = currentStrokeReference.GetStrokeCopyAtHistoryIndex(page, currentHistoryItem.HistoryIndex);
                        currentClusterCentroid = strokeCopy.WeightedCenter();
                        currentClusterWeight = strokeCopy.StrokeWeight();

                        currentPageObjectReference = FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, currentClusterCentroid, currentHistoryItem.HistoryIndex);
                        if (currentPageObjectReference != null)
                        {
                            currentLocationReference = FindLocationReferenceAtHistoryLocation(page, currentPageObjectReference, currentClusterCentroid, currentHistoryItem.HistoryIndex);
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
                    var nextPageObjectReference = FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, nextStroke, nextHistoryItem.HistoryIndex);
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
                            var distanceFromLastStroke = Math.Sqrt(DistanceSquaredBetweenPoints(currentCentroid, nextCentroid));
                            var lastStrokeDistanceZScore = (distanceFromLastStroke - averageClosestStrokeDistance) / closestStrokeDistanceStandardDeviation;
                            var isCloseToLastStroke = lastStrokeDistanceZScore <= HistoryAnalysis.MAX_DISTANCE_Z_SCORE ||
                                                      distanceFromLastStroke <= averageStrokeDimensions.X * HistoryAnalysis.DIMENSION_MULTIPLIER_THRESHOLD ||
                                                      distanceFromLastStroke <= averageStrokeDimensions.Y * HistoryAnalysis.DIMENSION_MULTIPLIER_THRESHOLD;

                            bool isCloseToCluster;
                            var distanceFromCluster = Math.Sqrt(DistanceSquaredBetweenPoints(currentClusterCentroid, nextCentroid));
                            if (historyItemBuffer.Count == 1)
                            {
                                isCloseToCluster = isCloseToLastStroke;
                            }
                            else
                            {
                                var clusterDistanceZScore = (distanceFromCluster - rollingStatsForDistanceFromCluster.Mean) / rollingStatsForDistanceFromCluster.StandardDeviation;
                                isCloseToCluster = clusterDistanceZScore <= HistoryAnalysis.MAX_DISTANCE_Z_SCORE;
                            }

                            if (isCloseToLastStroke || isCloseToCluster)
                            {
                                isNextPartOfCurrentCluster = true;

                                nextPageObjectReference = FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, currentClusterCentroid, nextHistoryItem.HistoryIndex);
                                if (nextPageObjectReference != null)
                                {
                                    nextLocationReference = FindLocationReferenceAtHistoryLocation(page, nextPageObjectReference, currentClusterCentroid, nextHistoryItem.HistoryIndex);
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
                    if (isNextInkPartOfCurrent && (isNextPartOfCurrentCluster || (currentLocationReference == Codings.ACTIONID_INK_LOCATION_OVER && isNextPageObjectReferencePartOfCurrent)))
                    {
                        continue;
                    }
                }

                var refinedHistoryAction = GroupAddOrErase(page, historyItemBuffer.Cast<ObjectsOnPageChangedHistoryItem>().ToList(), isInkAdd);
                refinedHistoryAction.CodedObjectID = "A";
                if (currentPageObjectReference != null)
                {
                    //var locationHack = currentPageObjectReference.CodedName == Codings.OBJECT_TEXT ? Codings.ACTIONID_INK_LOCATION_NEAR : currentLocationReference;

                    refinedHistoryAction.CodedObjectActionID = String.Format("{0} {1} [{2}]",
                                                                             currentLocationReference,
                                                                             currentPageObjectReference.CodedName,
                                                                             currentPageObjectReference.GetCodedIDAtHistoryIndex(refinedHistoryAction.HistoryItems.Last().HistoryIndex));
                }
                refinedHistoryAction.ReferencePageObjectID = currentPageObjectReference.ID;

                processedInkActions.Add(refinedHistoryAction);
                historyItemBuffer.Clear();
            }

            return processedInkActions;
        }

        public static IHistoryAction Arithmetic(CLPPage page, IHistoryAction inkAction)
        {
            const double INTERPRET_AS_ARITH_DIGIT_PERCENTAGE_THRESHOLD = 5.0;
            const string MULTIPLICATION_SYMBOL = "×";
            const string ADDITION_SYMBOL = "+";
            const string EQUALS_SYMBOL = "=";

            if (page == null ||
                inkAction == null ||
                inkAction.CodedObject != Codings.OBJECT_INK ||
                !(inkAction.CodedObjectAction == Codings.ACTION_INK_ADD || inkAction.CodedObjectAction == Codings.ACTION_INK_ERASE))
            {
                return null;
            }

            var strokes = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD
                              ? inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesRemoved).ToList();

            var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(strokes));
            var interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);

            var definitelyInArith = new List<string>
                                    {
                                        MULTIPLICATION_SYMBOL,
                                        ADDITION_SYMBOL,
                                        EQUALS_SYMBOL
                                    };
            var percentageOfDigits = GetPercentageOfDigits(interpretation);
            var isDefinitelyArith = definitelyInArith.Any(s => interpretation.Contains(s));

            if (percentageOfDigits < INTERPRET_AS_ARITH_DIGIT_PERCENTAGE_THRESHOLD &&
                !isDefinitelyArith)
            {
                return null;
            }

            var historyAction = new HistoryAction(page, inkAction)
                                {
                                    CodedObject = Codings.OBJECT_ARITH,
                                    CodedObjectAction = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ? Codings.ACTION_ARITH_ADD : Codings.ACTION_ARITH_ERASE,
                                    IsObjectActionVisible = inkAction.CodedObjectAction != Codings.ACTION_INK_ADD,
                                    CodedObjectID = "A",
                                    CodedObjectActionID = String.Format("\"{0}\"", interpretation)
                                };

            return historyAction;
        }

        public static IHistoryAction FillInInterpretation(CLPPage page, IHistoryAction inkAction)
        {
            if (page == null ||
                inkAction == null ||
                inkAction.CodedObject != Codings.OBJECT_INK ||
                !(inkAction.CodedObjectAction == Codings.ACTION_INK_ADD || inkAction.CodedObjectAction == Codings.ACTION_INK_ERASE))
            {
                return null;
            }

            var referenceRegionID = inkAction.ReferencePageObjectID;
            if (referenceRegionID == null)
            {
                return null;
            }
            var region = page.GetPageObjectByIDOnPageOrInHistory(referenceRegionID) as InterpretationRegion;
            if (region == null)
            {
                return null;
            }

            var strokes = inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList();
            var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(strokes));
            string interpretation;

            var relationDefinitionTag = page.Tags.FirstOrDefault(t => t is IRelationPart || t is DivisionRelationDefinitionTag);
            var answer = relationDefinitionTag == null
                             ? "UNDEFINED"
                             : relationDefinitionTag is IRelationPart
                                   ? (relationDefinitionTag as IRelationPart).RelationPartAnswerValue.ToString()
                                   : (relationDefinitionTag as DivisionRelationDefinitionTag).Quotient.ToString();
            if (answer == "UNDEFINED")
            {
                interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
            }
            else
            {
                int expectedValue;
                interpretation = Int32.TryParse(answer, out expectedValue)
                                     ? InkInterpreter.MatchInterpretationToExpectedInt(interpretations, expectedValue)
                                     : InkInterpreter.InterpretationClosestToANumber(interpretations);
            }

            var correctness = answer == "UNDEFINED" ? "unknown" : answer == interpretation ? "COR" : "INC";

            var historyAction = new HistoryAction(page, inkAction)
                                {
                                    CodedObject = Codings.OBJECT_FILL_IN,
                                    CodedObjectAction = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ? Codings.ACTION_FILL_IN_ADD : Codings.ACTION_FILL_IN_ERASE,
                                    IsObjectActionVisible = inkAction.CodedObjectAction != Codings.ACTION_INK_ADD,
                                    CodedObjectID = answer,
                                    CodedObjectActionID = String.Format("\"{0}\", {1}", interpretation, correctness)
                                };

            return historyAction;
        }

        #endregion // Verify And Generate Methods

        #region Utility Static Methods

        public static double GetPercentageOfDigits(string s)
        {
            var numberOfDigits = s.Where(Char.IsDigit).Count();
            return numberOfDigits * 100.0 / s.Length;
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
            var dy = point.Y - midY;
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