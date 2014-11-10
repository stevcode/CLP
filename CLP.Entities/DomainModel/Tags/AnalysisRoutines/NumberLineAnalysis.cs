using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CLP.Entities
{
    public static class NumberLineAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region) { }

        public static List<string> GetListOfNumberLineIDsInHistory(CLPPage page)
        {
            var completeOrderedHistory = page.History.UndoItems.Reverse().Concat(page.History.RedoItems).ToList();

            var numberLineIDsInHistory = new List<string>();
            foreach (var pageObjectsAddedHistoryItem in completeOrderedHistory.OfType<PageObjectsAddedHistoryItem>())
            {
                numberLineIDsInHistory.AddRange(from pageObjectID in pageObjectsAddedHistoryItem.PageObjectIDs
                                                      let numberLine =
                                                          page.GetPageObjectByID(pageObjectID) as NumberLine ?? page.History.GetPageObjectByID(pageObjectID) as NumberLine
                                                      where numberLine != null
                                                      select pageObjectID);
            }

            return numberLineIDsInHistory;
        }

    }
}