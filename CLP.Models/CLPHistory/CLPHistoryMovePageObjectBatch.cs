using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryMovePageObjectBatch : ACLPHistoryBatchBase
    {
        #region Constructors

        public CLPHistoryMovePageObjectBatch(ICLPPage parentPage)
            : base(parentPage)
        {
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryMovePageObjectBatch(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        public override void AddToBatch(object target, object tag = null)
        {
            var pageObject = target as ICLPPageObject;
            if(pageObject == null)
            {
                Logger.Instance.WriteToLog("Failed to pass ICLPPageObject as target in MovePageObjectBatch.");
                return;
            }

            var movePageObjectHistoryItem = HistoryItems.Where(clpHistoryItem =>
                                                                   {
                                                                       var clpHistoryMovePageObject =
                                                                           clpHistoryItem as CLPHistoryMovePageObject;
                                                                       return clpHistoryMovePageObject != null &&
                                                                              clpHistoryMovePageObject
                                                                                  .PageObjectUniqueID ==
                                                                              pageObject.UniqueID;
                                                                   }).FirstOrDefault();

            if(movePageObjectHistoryItem == null)
            {
                var newMovePageObjectHistoryItem = new CLPHistoryMovePageObject(ParentPage, pageObject.UniqueID,
                                                                             new Point(pageObject.XPosition,
                                                                                       pageObject.YPosition));
                HistoryItems.Add(newMovePageObjectHistoryItem);
            }
            else
            {
                var clpHistoryMovePageObject = movePageObjectHistoryItem as CLPHistoryMovePageObject;
                if(clpHistoryMovePageObject != null)
                {
                    clpHistoryMovePageObject.TravelledPositions.Add(new Point(pageObject.XPosition, pageObject.YPosition));
                }
            }
        }
    }
}
