using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using CLP.InkInterpretation;

namespace CLP.Entities
{
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

        public static IHistoryAction Arithmetic(CLPPage page, IHistoryAction inkAction)
        {
            const double INTERPRET_AS_ARITH_DIGIT_PERCENTAGE_THRESHOLD = 5.0;
            const string MULTIPLICATION_SYMBOL = "×";
            const string ADDITION_SYMBOL = "+";
            const string EQUALS_SYMBOL = "=";

            if (page == null ||
                inkAction == null ||
                inkAction.CodedObject != Codings.OBJECT_INK ||
                !(inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ||
                  inkAction.CodedObjectAction == Codings.ACTION_INK_ERASE))
            {
                return null;
            }

            var strokes = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD
                              ? inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesRemoved).ToList();

            var interpretation = InkInterpreter.StrokesToBestGuessText(new StrokeCollection(strokes));

            var definitelyInArith = new List<string> { MULTIPLICATION_SYMBOL, ADDITION_SYMBOL, EQUALS_SYMBOL };
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
                CodedObjectActionID = string.Format("\"{0}\"", interpretation)
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
            var answer = relationDefinitionTag == null ? "UNDEFINED" : relationDefinitionTag is IRelationPart ? (relationDefinitionTag as IRelationPart).RelationPartAnswerValue.ToString() : (relationDefinitionTag as DivisionRelationDefinitionTag).Quotient.ToString();
            if (answer == "UNDEFINED")
            {
                interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
            }
            else
            {
                int expectedValue;
                interpretation = int.TryParse(answer, out expectedValue) ? InkInterpreter.MatchInterpretationToExpectedInt(interpretations, expectedValue) : InkInterpreter.InterpretationClosestToANumber(interpretations);
            }

            var correctness =  answer == "UNDEFINED" ? "unknown" : answer == interpretation ? "COR" : "INC";

            var historyAction = new HistoryAction(page, inkAction)
            {
                CodedObject = Codings.OBJECT_FILL_IN,
                CodedObjectAction = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ? Codings.ACTION_FILL_IN_ADD : Codings.ACTION_FILL_IN_ERASE,
                IsObjectActionVisible = inkAction.CodedObjectAction != Codings.ACTION_INK_ADD,
                CodedObjectID = answer,
                CodedObjectActionID = string.Format("\"{0}\", {1}", interpretation, correctness)
            };

            return historyAction;
        }

        #endregion // Verify And Generate Methods

        #region Utility Static Methods

        public static double GetPercentageOfDigits(string s)
        {
            var numberOfDigits = s.Where(char.IsDigit).Count();
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
                    (pageObject is InterpretationRegion ||
                     pageObject is MultipleChoiceBox))  // HACK: Temporarily in place until MC Boxes are re-written and converted.
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