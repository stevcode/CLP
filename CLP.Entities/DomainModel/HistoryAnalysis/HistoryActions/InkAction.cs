using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Entities
{
    public static class InkCodedActions
    {
        #region Static Methods

        public static HistoryAction ChangeOrIgnore(CLPPage page, List<ObjectsOnPageChangedHistoryItem> objectsOnPageChangedHistoryItems, bool isChange = true)
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

        public static List<HistoryAction> GroupAddAndErase(CLPPage page, IHistoryAction inkAction)
        {
            if (page == null ||
                inkAction == null ||
                !inkAction.HistoryItemIDs.Any() ||
                inkAction.CodedObjectAction != Codings.ACTION_INK_CHANGE)
            {
                return null;
            }

            var orderedHistoryItems = inkAction.HistoryItems.OrderBy(h => h.HistoryIndex).ToList();


            // TODO
            return null;
        }

        //public static double DistanceBetweenStrokes(Stroke stroke1, Stroke stroke2)
        //{
        //    stroke1.GetBounds().
        //}

        #region Utility Static Methods

        public static Rect GetStrokeBoundsAtHistoryIndex(Stroke stroke, int historyIndex) { return stroke.GetBounds(); }

        #endregion // Utility Static Methods

        #endregion // Static Methods

    }
}
