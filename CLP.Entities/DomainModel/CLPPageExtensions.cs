using System.Collections.Generic;
using System.Linq;
using Catel;

namespace CLP.Entities
{
    public static class CLPPageExtensions
    {
        #region History

        public static List<IPageObject> GetPageObjectsOnPageAtHistoryIndex(this CLPPage page, int historyIndex)
        {
            Argument.IsNotNull("page", page);

            return page.PageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex)).ToList();
        }

        #endregion // History
    }
}