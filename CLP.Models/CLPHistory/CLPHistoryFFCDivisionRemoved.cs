using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryFFCDivisionRemoved : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryFFCDivisionRemoved(ICLPPage parentPage, string ffcUniqueID, int divisionValue)
            : base(parentPage)
        {
            FFCUniqueID = ffcUniqueID;
            DivisionValue = divisionValue;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryFFCDivisionRemoved(SerializationInfo info, StreamingContext context)
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
            get
            {
                return GetValue<string>(FFCUniqueIDProperty);
            }
            set
            {
                SetValue(FFCUniqueIDProperty, value);
            }
        }

        public static readonly PropertyData FFCUniqueIDProperty = RegisterProperty("FFCUniqueID", typeof(string), string.Empty);

        /// <summary>
        /// Value of the label on the removed division.
        /// </summary>
        public int DivisionValue
        {
            get
            {
                return GetValue<int>(DivisionValueProperty);
            }
            set
            {
                SetValue(DivisionValueProperty, value);
            }
        }

        /// <summary>
        /// Register the value property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DivisionValueProperty = RegisterProperty("DivisionValue", typeof(int), null);

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
                ffc.SnapInArray(DivisionValue);
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
            var ffc = ParentPage.GetPageObjectByUniqueID(FFCUniqueID) as CLPFuzzyFactorCard;
            if(ffc != null)
            {
                ffc.RemoveLastDivision();
            }
            else
            {
                Logger.Instance.WriteToLog("Fuzzy Factor Card not found on page for RedoAction");
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryFFCDivisionRemoved;
            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
