using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryArrayGridToggle : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryArrayGridToggle(ICLPPage parentPage, string arrayUniqueID)
            : base(parentPage)
        {
            ArrayUniqueID = arrayUniqueID;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryArrayGridToggle(SerializationInfo info, StreamingContext context)
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
        /// UniqueID of the Array whose divisions have been modified
        /// </summary>
        public string ArrayUniqueID
        {
            get { return GetValue<string>(ArrayUniqueIDProperty); }
            set { SetValue(ArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData ArrayUniqueIDProperty = RegisterProperty("ArrayUniqueID", typeof(string), string.Empty);

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            ToggleGrid();
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            ToggleGrid();
        }

        private void ToggleGrid()
        {
            var array = ParentPage.GetPageObjectByUniqueID(ArrayUniqueID) as CLPArray;
            if(array != null)
            {
                array.IsGridOn = !array.IsGridOn;
            }
            else
            {
                Logger.Instance.WriteToLog("Array not found on page for UndoAction");
            }
        }

        #endregion //Methods
    }
}

