using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryFFCArraySnappedIn : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryFFCArraySnappedIn(ICLPPage parentPage, string ffcUniqueID, CLPArray snappedInArray)
            : base(parentPage)
        {
            FFCUniqueID = ffcUniqueID;
            SnappedInArray = snappedInArray;
            SnappedInArrayUniqueID = snappedInArray.UniqueID;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryFFCArraySnappedIn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 600;
            }
        }

        /// <summary>
        /// UniqueID of the FFC which had an array snapped inside
        /// </summary>
        public string FFCUniqueID
        {
            get { return GetValue<string>(FFCUniqueIDProperty); }
            set { SetValue(FFCUniqueIDProperty, value); }
        }

        public static readonly PropertyData FFCUniqueIDProperty = RegisterProperty("FFCUniqueID", typeof(string), string.Empty);

        /// <summary>
        /// Array which was snapped into the FFC. Null if it's currently on the page.
        /// </summary>
        public CLPArray SnappedInArray
        {
            get { return GetValue<CLPArray>(SnappedInArrayProperty); }
            set { SetValue(SnappedInArrayProperty, value); }
        }

        public static readonly PropertyData SnappedInArrayProperty = RegisterProperty("SnappedInArray", typeof(CLPArray));

        /// <summary>
        /// UniqueID of the array that wass snapped in and then deleted. 
        /// </summary>
        public string SnappedInArrayUniqueID
        {
            get { return GetValue<string>(SnappedInArrayUniqueIDProperty); }
            set { SetValue(SnappedInArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData SnappedInArrayUniqueIDProperty = RegisterProperty("SnappedInArrayUniqueID", typeof(string), string.Empty);

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var ffc = ParentPage.GetPageObjectByUniqueID(FFCUniqueID) as CLPFuzzyFactorCard;
            if(ffc != null)
            {
                ffc.RemoveLastDivision();
                SnappedInArray.ParentPage = ParentPage;
                ParentPage.PageObjects.Add(SnappedInArray);
                SnappedInArray = null;
            }
            else
            {
                Logger.Instance.WriteToLog("Fuzzy Factor Card not found on page for UndoAction");
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            SnappedInArray = ParentPage.GetPageObjectByUniqueID(SnappedInArrayUniqueID) as CLPArray;
            var ffc = ParentPage.GetPageObjectByUniqueID(FFCUniqueID) as CLPFuzzyFactorCard;
            if(ffc != null)
            {
                if(ffc.IsHorizontallyAligned)
                {
                    ffc.SnapInArray(SnappedInArray.Columns);
                }
                else
                {
                    ffc.SnapInArray(SnappedInArray.Rows);
                }
                ParentPage.PageObjects.Remove(SnappedInArray);
            }
            else
            {
                Logger.Instance.WriteToLog("Fuzzy Factor Card not found on page for RedoAction");
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryFFCArraySnappedIn;

            var snappedInArray = ParentPage.GetPageObjectByUniqueID(SnappedInArrayUniqueID);
            if(snappedInArray == null)
            {
                Logger.Instance.WriteToLog("Failed to get snappedInArray by ID during UndoRedoComplete in HistoryFFCArraySnappedIn.");
                return null;
            }
            clonedHistoryItem.SnappedInArray = snappedInArray as CLPArray;

            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
