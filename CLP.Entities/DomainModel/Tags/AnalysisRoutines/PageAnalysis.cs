using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CLP.Entities
{
    public static class PageAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region)
        {
            AnalyzeObjectTypesOnPage(page);
            AnalyzeHighlightingOnPage(page);
            AnalyzeObjectTypesOnInHistory(page);
        }

        public static void AnalyzeObjectTypesOnPage(CLPPage page)
        {
            var objectTypes = new List<string>();
            foreach (var pageObject in page.PageObjects.Where(pageObject => pageObject.OwnerID == page.OwnerID))
            {
                if (pageObject is FuzzyFactorCard)
                {
                    objectTypes.Add("Division Templates");
                    continue;
                }

                if (pageObject is CLPArray)
                {
                    objectTypes.Add("Arrays");
                    continue;
                }

                if (pageObject is RemainderTiles)
                {
                    objectTypes.Add("Remainder Tiles");
                    continue;
                }

                if (pageObject is NumberLine)
                {
                    objectTypes.Add("Number Line");
                    continue;
                }

                if (pageObject is Stamp)
                {
                    objectTypes.Add("Stamps");
                    continue;
                }

                if (pageObject is StampedObject)
                {
                    objectTypes.Add("Stamped Objects");
                    continue;
                }

                if (pageObject is Shape)
                {
                    objectTypes.Add("Shapes");
                    continue;
                }
            }

            objectTypes = objectTypes.Distinct().ToList();

            // HACK: Reimplement the comments below once better login is implemented to keep the correct Current User
            if (page.InkStrokes.Any()) //stroke => stroke.GetStrokeOwnerID() == page.OwnerID))
            {
                objectTypes.Add("Ink");
            }

            if (page.InkStrokes.Any(stroke => stroke.DrawingAttributes.IsHighlighter)) // && stroke.GetStrokeOwnerID() == page.OwnerID))
            {
                objectTypes.Add("Highlighter");
            }

            if (objectTypes.Any())
            {
                page.AddTag(new ObjectTypesOnPageTag(page, Origin.StudentPageGenerated, objectTypes));
            }
        }

        public static void AnalyzeHighlightingOnPage(CLPPage page)
        {
            var objectTypes = new List<string>();

            // HACK: Reimplement the comments below once better login is implemented to keep the correct Current User
            foreach (var stroke in page.InkStrokes.Where(s => s.DrawingAttributes.IsHighlighter)) // && s.GetStrokeOwnerID() == page.OwnerID)) 
            {
                foreach (var pageObject in page.PageObjects.Where(p => p.OwnerID == page.OwnerID))
                {
                    var pageObjectBoundingBox = new Rect(pageObject.XPosition, pageObject.YPosition, pageObject.Width, pageObject.Height);
                    var isOverPageObject = stroke.HitTest(pageObjectBoundingBox, 50); //Stroke must be at least 50% contained by pageObject.

                    if (!isOverPageObject)
                    {
                        continue;
                    }

                    if (pageObject is FuzzyFactorCard)
                    {
                        continue;
                    }

                    if (pageObject is CLPArray)
                    {
                        objectTypes.Add("Arrays");
                        continue;
                    }

                    if (pageObject is RemainderTiles)
                    {
                        continue;
                    }

                    if (pageObject is NumberLine)
                    {
                        objectTypes.Add("Number Line");
                        continue;
                    }

                    if (pageObject is Stamp)
                    {
                        continue;
                    }

                    if (pageObject is StampedObject)
                    {
                        objectTypes.Add("Stamped Objects");
                        continue;
                    }

                    if (pageObject is Shape)
                    {
                        objectTypes.Add("Shapes");
                        continue;
                    }
                }
            }

            objectTypes = objectTypes.Distinct().ToList();

            if (objectTypes.Any())
            {
                page.AddTag(new HighlightedObjectTypesOnPageTag(page, Origin.StudentPageGenerated, objectTypes));
            }
        }

        public static void AnalyzeObjectTypesOnInHistory(CLPPage page)
        {
            var objectTypes = new List<string>();
            foreach (
                var pageObject in
                    page.PageObjects.Where(pageObject => pageObject.OwnerID == page.OwnerID)
                        .Concat(page.History.TrashedPageObjects.Where(pageObject => pageObject.OwnerID == page.OwnerID)))
            {
                if (pageObject is FuzzyFactorCard)
                {
                    objectTypes.Add("Division Templates");
                    var divisionTemplate = pageObject as FuzzyFactorCard;
                    if (divisionTemplate.RemainderTiles != null)
                    {
                        objectTypes.Add("Remainder Tiles");
                    }
                    continue;
                }

                if (pageObject is CLPArray)
                {
                    objectTypes.Add("Arrays");
                    continue;
                }

                if (pageObject is RemainderTiles)
                {
                    objectTypes.Add("Remainder Tiles");
                    continue;
                }

                if (pageObject is Stamp)
                {
                    objectTypes.Add("Stamps");
                    continue;
                }

                if (pageObject is NumberLine)
                {
                    objectTypes.Add("Number Line");
                    continue;
                }

                if (pageObject is StampedObject)
                {
                    objectTypes.Add("Stamped Objects");
                    continue;
                }

                if (pageObject is Shape)
                {
                    objectTypes.Add("Shapes");
                    continue;
                }
            }

            objectTypes = objectTypes.Distinct().ToList();

            // HACK: Reimplement the comments below once better login is implemented to keep the correct Current User
            if (page.InkStrokes.Any() || //stroke => stroke.GetStrokeOwnerID() == page.OwnerID) ||
                page.History.TrashedInkStrokes.Any()) //stroke => stroke.GetStrokeOwnerID() == page.OwnerID))
            {
                objectTypes.Add("Ink");
            }

            if (page.InkStrokes.Any(stroke => stroke.DrawingAttributes.IsHighlighter) || // && stroke.GetStrokeOwnerID() == page.OwnerID) ||
                page.History.TrashedInkStrokes.Any(stroke => stroke.DrawingAttributes.IsHighlighter)) // && stroke.GetStrokeOwnerID() == page.OwnerID))
            {
                objectTypes.Add("Highlighter");
            }

            if (objectTypes.Any())
            {
                page.AddTag(new ObjectTypesInHistoryTag(page, Origin.StudentPageGenerated, objectTypes));
            }
        }
    }
}