using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Ink;
using Catel;

namespace CLP.Entities
{
    public static class CLPPageExtensions
    {
        #region PageObjects

        public static IPageObject GetPageObjectByID(this CLPPage page, string id)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNullOrWhitespace("id", id);

            return !page.PageObjects.Any() ? null : page.PageObjects.FirstOrDefault(pageObject => pageObject.ID == id);
        }

        public static IPageObject GetVerifiedPageObjectOnPageByID(this CLPPage page, string id)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNullOrWhitespace("id", id);

            var pageObject = page.GetPageObjectByID(id);

            if (pageObject != null)
            {
                return pageObject;
            }

            pageObject = page.History.GetPageObjectByID(id);
            if (pageObject == null)
            {
                return null;
            }

            CLogger.AppendToLog("PageObject incorrectly existed in TrashedPageObjects.");
            page.History.TrashedPageObjects.Remove(pageObject);
            page.PageObjects.Add(pageObject);

            return pageObject;
        }

        public static IPageObject GetVerifiedPageObjectInTrashByID(this CLPPage page, string id)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNullOrWhitespace("id", id);

            var pageObject = page.History.GetPageObjectByID(id);

            if (pageObject != null)
            {
                return pageObject;
            }

            pageObject = page.GetPageObjectByID(id);
            if (pageObject == null)
            {
                return null;
            }

            CLogger.AppendToLog("PageObject incorrectly existed on Page.");
            page.History.TrashedPageObjects.Add(pageObject);
            page.PageObjects.Remove(pageObject);

            return pageObject;
        }

        public static IPageObject GetPageObjectByIDOnPageOrInHistory(this CLPPage page, string id)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNullOrWhitespace("id", id);

            var pageObject = page.History.GetPageObjectByID(id);

            if (pageObject != null)
            {
                return pageObject;
            }

            pageObject = page.GetPageObjectByID(id);
            return pageObject;
        }

        #endregion // PageObjects

        #region Strokes

        public static Stroke GetStrokeByID(this CLPPage page, string id)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNullOrWhitespace("id", id);

            return !page.InkStrokes.Any() ? null : page.InkStrokes.FirstOrDefault(stroke => stroke.GetStrokeID() == id);
        }

        public static Stroke GetVerifiedStrokeOnPageByID(this CLPPage page, string id)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNullOrWhitespace("id", id);

            var stroke = page.GetStrokeByID(id);

            if (stroke != null)
            {
                return stroke;
            }

            stroke = page.History.GetStrokeByID(id);
            if (stroke == null)
            {
                return null;
            }

            CLogger.AppendToLog("Stroke incorrectly existed in TrashedInkStrokes.");
            page.History.TrashedInkStrokes.Remove(stroke);
            page.InkStrokes.Add(stroke);

            return stroke;
        }

        public static Stroke GetVerifiedStrokeInHistoryByID(this CLPPage page, string id)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNullOrWhitespace("id", id);

            var stroke = page.History.GetStrokeByID(id);

            if (stroke != null)
            {
                return stroke;
            }

            stroke = page.GetStrokeByID(id);
            if (stroke == null)
            {
                return null;
            }

            CLogger.AppendToLog("Stroke incorrectly existed on Page.");
            page.History.TrashedInkStrokes.Add(stroke);
            page.InkStrokes.Remove(stroke);

            return stroke;
        }

        public static Stroke GetStrokeByIDOnPageOrInHistory(this CLPPage page, string id)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNullOrWhitespace("id", id);

            var stroke = page.History.GetStrokeByID(id);

            if (stroke != null)
            {
                return stroke;
            }

            stroke = page.GetStrokeByID(id);
            return stroke;
        }

        #endregion // Strokes

        #region History

        /// <summary>Gets all pageObjects that were on the page immediately after the historyAction at the given historyIndex was performed</summary>
        public static List<IPageObject> GetPageObjectsOnPageAtHistoryIndex(this CLPPage page, int historyIndex)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("historyIndex", historyIndex);

            var pageObjects = page.PageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex)).ToList();
            var trashedPageObjects = page.History.TrashedPageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex)).ToList();
            var pageObjectsOnPage = pageObjects.Concat(trashedPageObjects).Distinct().ToList();

            return pageObjectsOnPage;
        }

        /// <summary>Gets all strokes that were on the page immediately after the historyAction at the given historyIndex was performed</summary>
        public static List<Stroke> GetStrokesOnPageAtHistoryIndex<T>(this CLPPage page, int historyIndex) where T : IStrokesOnPageChangedHistoryAction
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("historyIndex", historyIndex);

            var strokes = page.InkStrokes.Where(s => s.IsOnPageAtHistoryIndex<T>(page, historyIndex)).ToList();
            var trashedStrokes = page.History.TrashedInkStrokes.Where(s => s.IsOnPageAtHistoryIndex<T>(page, historyIndex)).ToList();
            var strokesOnPage = strokes.Concat(trashedStrokes).Distinct().ToList();

            return strokesOnPage;
        }

        /// <summary>Gets all strokes that were added to the page between the given historyIndexes (including strokes that were added by the historyActions at both historyIndexes).</summary>
        public static List<Stroke> GetStrokesAddedToPageBetweenHistoryIndexes<T>(this CLPPage page, int startHistoryIndex, int endHistoryIndex) where T : IStrokesOnPageChangedHistoryAction
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("startHistoryIndex", startHistoryIndex);
            Argument.IsNotNull("endHistoryIndex", endHistoryIndex);

            var strokes = page.InkStrokes.Where(s => s.IsAddedBetweenHistoryIndexes<T>(page, startHistoryIndex, endHistoryIndex)).ToList();
            var trashedStrokes = page.History.TrashedInkStrokes.Where(s => s.IsAddedBetweenHistoryIndexes<T>(page, startHistoryIndex, endHistoryIndex)).ToList();
            var strokesAddedToPage = strokes.Concat(trashedStrokes).Distinct().ToList();

            return strokesAddedToPage;
        }

        #endregion // History

        #region Testing

        public static void ValidateStrokeIDs(this CLPPage page)
        {
            Argument.IsNotNull("page", page);

            foreach (var stroke in page.InkStrokes)
            {
                if (stroke.GetStrokeID() == "noStrokeID")
                {
                    CLogger.AppendToLog($"Stroke without ID on page {page.PageNumber}, {page.Owner.FullName}");
                }
            }

            foreach (var stroke in page.History.TrashedInkStrokes)
            {
                if (stroke.GetStrokeID() == "noStrokeID")
                {
                    CLogger.AppendToLog($"Trashed Stroke without ID on page {page.PageNumber}, {page.Owner.FullName}");
                }
            }
        }

        #endregion // Testing
    }
}