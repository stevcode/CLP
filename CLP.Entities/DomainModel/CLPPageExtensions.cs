using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using Catel;

namespace CLP.Entities
{
    public static class CLPPageExtensions
    {
        #region History

        public static List<IPageObject> GetPageObjectsOnPageAtHistoryIndex(this CLPPage page, int historyIndex)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("historyIndex", historyIndex);

            var pageObjects = page.PageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex)).ToList();
            var trashedPageObjects = page.History.TrashedPageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex)).ToList();
            var pageObjectsOnPage = pageObjects.Concat(trashedPageObjects).Distinct().ToList();

            return pageObjectsOnPage;
        }

        public static List<Stroke> GetStrokesOnPageAtHistoryIndex(this CLPPage page, int historyIndex)
        {
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("historyIndex", historyIndex);

            var strokes = page.InkStrokes.Where(s => s.IsOnPageAtHistoryIndex(page, historyIndex)).ToList();
            var trashedStrokes = page.History.TrashedInkStrokes.Where(s => s.IsOnPageAtHistoryIndex(page, historyIndex)).ToList();
            var strokesOnPage = strokes.Concat(trashedStrokes).Distinct().ToList();

            return strokesOnPage;
        }

        #endregion // History
    }
}