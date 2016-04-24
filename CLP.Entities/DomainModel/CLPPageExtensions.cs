using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using Catel;

namespace CLP.Entities
{
    public static class CLPPageExtensions
    {
        #region History

        /// <summary>
        /// Gets all pageObjects that were on the page immediately after the historyItem at the given historyIndex was performed
        /// </summary>
        public static List<IPageObject> GetPageObjectsOnPageAtHistoryIndex(this CLPPage page, int historyIndex)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("historyIndex", historyIndex);

            var pageObjects = page.PageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex)).ToList();
            var trashedPageObjects = page.History.TrashedPageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex)).ToList();
            var pageObjectsOnPage = pageObjects.Concat(trashedPageObjects).Distinct().ToList();

            return pageObjectsOnPage;
        }

        /// <summary>
        /// Gets all strokes that were on the page immediately after the historyItem at the given historyIndex was performed
        /// </summary>
        public static List<Stroke> GetStrokesOnPageAtHistoryIndex(this CLPPage page, int historyIndex)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("historyIndex", historyIndex);

            var strokes = page.InkStrokes.Where(s => s.IsOnPageAtHistoryIndex(page, historyIndex)).ToList();
            var trashedStrokes = page.History.TrashedInkStrokes.Where(s => s.IsOnPageAtHistoryIndex(page, historyIndex)).ToList();
            var strokesOnPage = strokes.Concat(trashedStrokes).Distinct().ToList();

            return strokesOnPage;
        }

        /// <summary>
        /// Gets all strokes that were added to the page between the given historyIndexes (including strokes that were added by the
        /// historyItems at both historyIndexes).
        /// </summary>
        public static List<Stroke> GetStrokesAddedToPageBetweenHistoryIndexes(this CLPPage page, int startHistoryIndex, int endHistoryIndex)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("startHistoryIndex", startHistoryIndex);
            Argument.IsNotNull("endHistoryIndex", endHistoryIndex);

            var strokes = page.InkStrokes.Where(s => s.IsAddedBetweenHistoryIndexes(page, startHistoryIndex, endHistoryIndex)).ToList();
            var trashedStrokes = page.History.TrashedInkStrokes.Where(s => s.IsAddedBetweenHistoryIndexes(page, startHistoryIndex, endHistoryIndex)).ToList();
            var strokesAddedToPage = strokes.Concat(trashedStrokes).Distinct().ToList();

            return strokesAddedToPage;
        }

        #endregion // History
    }
}