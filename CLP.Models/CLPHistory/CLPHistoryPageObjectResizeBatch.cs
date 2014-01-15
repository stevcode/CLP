using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryPageObjectResizeBatch : ACLPHistoryItemBase, IHistoryBatch
    {
        #region Constructors

        public CLPHistoryPageObjectResizeBatch(ICLPPage parentPage, string uniqueID, Point currentDimensions)
            : base(parentPage)
        {
            PageObjectUniqueID = uniqueID;
            StretchedDimensions = new ObservableCollection<Point> { currentDimensions };
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryPageObjectResizeBatch(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// UniqueID of the PageObject added to the page.
        /// </summary>
        public string PageObjectUniqueID
        {
            get { return GetValue<string>(PageObjectUniqueIDProperty); }
            set { SetValue(PageObjectUniqueIDProperty, value); }
        }

        public static readonly PropertyData PageObjectUniqueIDProperty = RegisterProperty("PageObjectUniqueID", typeof(string));

        /// <summary>
        /// Each point in the collection represents a change in dimensions during a single resize.
        /// </summary>
        public ObservableCollection<Point> StretchedDimensions
        {
            get { return GetValue<ObservableCollection<Point>>(StretchedDimensionsProperty); }
            set { SetValue(StretchedDimensionsProperty, value); }
        }

        public static readonly PropertyData StretchedDimensionsProperty = RegisterProperty("StretchedDimensions", typeof(ObservableCollection<Point>));

        public int BatchDelay
        {
            get
            {
                return 100;
            }
        }

        public int NumberOfBatchTicks
        {
            get
            {
                return StretchedDimensions.Count - 1;
            }
        }

        /// <summary>
        /// Location within the Batch.
        /// </summary>
        public int CurrentBatchTickIndex
        {
            get { return GetValue<int>(CurrentBatchTickIndexProperty); }
            set { SetValue(CurrentBatchTickIndexProperty, value); }
        }

        public static readonly PropertyData CurrentBatchTickIndexProperty = RegisterProperty("CurrentBatchTickIndex", typeof(int), 0);

        #endregion //Properties

        #region Methods

        public void AddResizePointToBatch(string uniqueID, Point currentDimensions)
        {
            if(PageObjectUniqueID != uniqueID)
            {
                Logger.Instance.WriteToLog("Failed to add resize operation to Batch. Mismatching UniqueIDs.");
                return;
            }

            StretchedDimensions.Add(currentDimensions);
        }

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            if(CurrentBatchTickIndex < 0)
            {
                return;
            }
            if(CurrentBatchTickIndex > NumberOfBatchTicks)
            {
                CurrentBatchTickIndex = NumberOfBatchTicks;
            }

            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);

            if(isAnimationUndo && CurrentBatchTickIndex > 0)
            {
                var stretchedDimension = StretchedDimensions[CurrentBatchTickIndex - 1];
                pageObject.Width = stretchedDimension.X;
                pageObject.Height = stretchedDimension.Y;
                CurrentBatchTickIndex--;
            }
            else if(StretchedDimensions.Any())
            {
                var originalDimension = StretchedDimensions.First();
                pageObject.Width = originalDimension.X;
                pageObject.Height = originalDimension.Y;
                CurrentBatchTickIndex = -1;
            }
            pageObject.OnResized();
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(CurrentBatchTickIndex > NumberOfBatchTicks)
            {
                return;
            }
            if(CurrentBatchTickIndex < 0)
            {
                CurrentBatchTickIndex = 0;
            }

            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);

            if(isAnimationRedo)
            {
                var stretchedDimension = StretchedDimensions[CurrentBatchTickIndex];
                pageObject.Width = stretchedDimension.X;
                pageObject.Height = stretchedDimension.Y;
                CurrentBatchTickIndex++;
            }
            else if(StretchedDimensions.Any())
            {
                var lastDimensions = StretchedDimensions.Last();
                pageObject.Width = lastDimensions.X;
                pageObject.Height = lastDimensions.Y;
                CurrentBatchTickIndex = NumberOfBatchTicks + 1;
            }
            pageObject.OnResized();
        }

        public void ClearBatchAfterCurrentIndex()
        {
            var newBatch = new List<Point>();
            for(int i = 0; i < CurrentBatchTickIndex + 1; i++)
            {
                newBatch.Add(StretchedDimensions[i]);
            }
            StretchedDimensions = new ObservableCollection<Point>(newBatch);
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryPageObjectResizeBatch;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            clonedHistoryItem.CurrentBatchTickIndex = -1;

            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
