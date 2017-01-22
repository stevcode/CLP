using System;
using System.Diagnostics;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArrayRotateHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayRotateHistoryAction" /> from scratch.</summary>
        public CLPArrayRotateHistoryAction() { }

        /// <summary>Initializes <see cref="CLPArrayRotateHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        public CLPArrayRotateHistoryAction(CLPPage parentPage,
                                           Person owner,
                                           string arrayID,
                                           double oldXPosition,
                                           double oldYPosition,
                                           double newXPosition,
                                           double newYPosition,
                                           double oldWidth,
                                           double oldHeight,
                                           double newWidth,
                                           double newHeight,
                                           int oldRows,
                                           int oldColumns)
            : base(parentPage, owner)
        {
            ArrayID = arrayID;
            OldXPosition = oldXPosition;
            OldYPosition = oldYPosition;
            NewXPosition = newXPosition;
            NewYPosition = newYPosition;
            OldWidth = oldWidth;
            OldHeight = oldHeight;
            NewWidth = newWidth;
            NewHeight = newHeight;
            OldRows = oldRows;
            OldColumns = oldColumns;
        }

        #endregion // Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="ACLPArrayBase" /> this <see cref="IHistoryAction" /> modifies.</summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof(string), string.Empty);

        /// <summary>X Position before rotate.</summary>
        public double OldXPosition
        {
            get { return GetValue<double>(OldXPositionProperty); }
            set { SetValue(OldXPositionProperty, value); }
        }

        public static readonly PropertyData OldXPositionProperty = RegisterProperty("OldXPosition", typeof(double), 0.0);

        /// <summary>Y Position before rotate.</summary>
        public double OldYPosition
        {
            get { return GetValue<double>(OldYPositionProperty); }
            set { SetValue(OldYPositionProperty, value); }
        }

        public static readonly PropertyData OldYPositionProperty = RegisterProperty("OldYPosition", typeof(double), 0.0);

        /// <summary>X Position after rotate.</summary>
        public double NewXPosition
        {
            get { return GetValue<double>(NewXPositionProperty); }
            set { SetValue(NewXPositionProperty, value); }
        }

        public static readonly PropertyData NewXPositionProperty = RegisterProperty("NewXPosition", typeof(double), 0.0);

        /// <summary>Y Position after rotate.</summary>
        public double NewYPosition
        {
            get { return GetValue<double>(NewYPositionProperty); }
            set { SetValue(NewYPositionProperty, value); }
        }

        public static readonly PropertyData NewYPositionProperty = RegisterProperty("NewYPosition", typeof(double), 0.0);

        /// <summary>Width before rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public double OldWidth
        {
            get { return GetValue<double>(OldWidthProperty); }
            set { SetValue(OldWidthProperty, value); }
        }

        public static readonly PropertyData OldWidthProperty = RegisterProperty("OldWidth", typeof(double), 0.0);

        /// <summary>Height before rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public double OldHeight
        {
            get { return GetValue<double>(OldHeightProperty); }
            set { SetValue(OldHeightProperty, value); }
        }

        public static readonly PropertyData OldHeightProperty = RegisterProperty("OldHeight", typeof(double), 0.0);

        /// <summary>Width after rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public double NewWidth
        {
            get { return GetValue<double>(NewWidthProperty); }
            set { SetValue(NewWidthProperty, value); }
        }

        public static readonly PropertyData NewWidthProperty = RegisterProperty("NewWidth", typeof(double), 0.0);

        /// <summary>Height after rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public double NewHeight
        {
            get { return GetValue<double>(NewHeightProperty); }
            set { SetValue(NewHeightProperty, value); }
        }

        public static readonly PropertyData NewHeightProperty = RegisterProperty("NewHeight", typeof(double), 0.0);

        /// <summary>Rows value before rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public int OldRows
        {
            get { return GetValue<int>(OldRowsProperty); }
            set { SetValue(OldRowsProperty, value); }
        }

        public static readonly PropertyData OldRowsProperty = RegisterProperty("OldRows", typeof(int), 0);

        /// <summary>Columns value before rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public int OldColumns
        {
            get { return GetValue<int>(OldColumnsProperty); }
            set { SetValue(OldColumnsProperty, value); }
        }

        public static readonly PropertyData OldColumnsProperty = RegisterProperty("OldColumns", typeof(int), 0);

        #region Calculated Properties

        /// <summary>Rows value after rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public int NewRows => OldColumns;

        /// <summary>Columns value after rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public int NewColumns => OldRows;

        #endregion // Calculated Properties

        #endregion // Properties

        #region Methods

        private void RotateArray(bool isUndo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Array for Rotate not found on page or in history.", HistoryActionIndex);
                return;
            }

            array.RotateArray();
            array.XPosition = isUndo ? OldXPosition : NewXPosition;
            array.YPosition = isUndo ? OldYPosition : NewYPosition;
        }

        #endregion // Methods

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 600;

        protected override string FormattedReport
        {
            get
            {
                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(ArrayID) as ACLPArrayBase;
                return array == null ? "[ERROR] Array for Rotate not found on page or in history." : $"Array rotated from [{OldRows}x{OldColumns}] to [{OldColumns}x{OldRows}]";
            }
        }

        protected override void ConversionUndoAction() { }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            RotateArray(true);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            RotateArray(false);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            if (clonedHistoryAction == null)
            {
                return null;
            }

            var array = ParentPage.GetPageObjectByID(ArrayID) as ACLPArrayBase;
            clonedHistoryAction.OldXPosition = array.XPosition;
            clonedHistoryAction.OldYPosition = array.YPosition;

            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return ArrayID == id;
        }

        #endregion // AHistoryActionBase Overrides
    }
}