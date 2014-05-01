using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;

namespace CLP.Entities
{
    public class PageObjectResizeBatchHistoryItem : AHistoryItemBase, IHistoryBatch
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="PageObjectResizeBatchHistoryItem" /> from scratch.
        /// </summary>
        public PageObjectResizeBatchHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="PageObjectResizeBatchHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public PageObjectResizeBatchHistoryItem(CLPPage parentPage, Person owner, string pageObjectID, Point currentDimensions)
            : base(parentPage, owner)
        {
            PageObjectID = pageObjectID;
            StretchedDimensions = new List<Point>
                                  {
                                      currentDimensions
                                  };
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected PageObjectResizeBatchHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// The ID of the <see cref="IPageObject" /> that was moved.
        /// </summary>
        public string PageObjectID
        {
            get { return GetValue<string>(PageObjectIDProperty); }
            set { SetValue(PageObjectIDProperty, value); }
        }

        public static readonly PropertyData PageObjectIDProperty = RegisterProperty("PageObjectID", typeof(string), string.Empty);

        /// <summary>
        /// Each point in the collection represents a change in dimensions during a single resize.
        /// </summary>
        public List<Point> StretchedDimensions
        {
            get { return GetValue<List<Point>>(StretchedDimensionsProperty); }
            set { SetValue(StretchedDimensionsProperty, value); }
        }

        public static readonly PropertyData StretchedDimensionsProperty = RegisterProperty("StretchedDimensions", typeof(List<Point>));

        public int NumberOfBatchTicks
        {
            get { return StretchedDimensions.Count - 1; }
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
            if(PageObjectID != uniqueID)
            {
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

            var pageObject = ParentPage.GetPageObjectByID(PageObjectID);

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

            var pageObject = ParentPage.GetPageObjectByID(PageObjectID);

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
            for(var i = 0; i < CurrentBatchTickIndex + 1; i++)
            {
                newBatch.Add(StretchedDimensions[i]);
            }
            StretchedDimensions = newBatch;
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as PageObjectResizeBatchHistoryItem;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            clonedHistoryItem.CurrentBatchTickIndex = -1;

            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}