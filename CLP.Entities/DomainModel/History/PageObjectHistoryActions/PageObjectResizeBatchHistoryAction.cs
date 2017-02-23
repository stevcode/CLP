using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class PageObjectResizeBatchHistoryAction : AHistoryActionBase, IHistoryBatch
    {
        #region Constructors

        /// <summary>Initializes <see cref="PageObjectResizeBatchHistoryAction" /> from scratch.</summary>
        public PageObjectResizeBatchHistoryAction() { }

        /// <summary>Initializes <see cref="PageObjectResizeBatchHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public PageObjectResizeBatchHistoryAction(CLPPage parentPage, Person owner, string pageObjectID, Point currentDimensions)
            : base(parentPage, owner)
        {
            PageObjectID = pageObjectID;
            StretchedDimensions = new List<Point>
                                  {
                                      currentDimensions
                                  };
        }

        #endregion // Constructors

        #region Properties

        /// <summary>The ID of the <see cref="IPageObject" /> that was moved.</summary>
        public string PageObjectID
        {
            get { return GetValue<string>(PageObjectIDProperty); }
            set { SetValue(PageObjectIDProperty, value); }
        }

        public static readonly PropertyData PageObjectIDProperty = RegisterProperty("PageObjectID", typeof(string), string.Empty);

        /// <summary>Each point in the collection represents a change in dimensions during a single resize.</summary>
        public List<Point> StretchedDimensions
        {
            get { return GetValue<List<Point>>(StretchedDimensionsProperty); }
            set { SetValue(StretchedDimensionsProperty, value); }
        }

        public static readonly PropertyData StretchedDimensionsProperty = RegisterProperty("StretchedDimensions", typeof(List<Point>), () => new List<Point>());

        public double OriginalWidth => StretchedDimensions.First().X;

        public double OriginalHeight => StretchedDimensions.First().Y;

        public double FinalWidth => StretchedDimensions.Last().X;

        public double FinalHeight => StretchedDimensions.Last().Y;

        #endregion // Properties

        #region Methods

        public void AddResizePointToBatch(string uniqueID, Point currentDimensions)
        {
            if (PageObjectID != uniqueID)
            {
                return;
            }

            StretchedDimensions.Add(currentDimensions);
        }

        #endregion // Methods

        #region AHistoryActionBase Overrides

        protected override string FormattedReport
        {
            get
            {
                var pageObject = ParentPage.GetPageObjectByIDOnPageOrInHistory(PageObjectID);

                return pageObject == null
                           ? "[ERROR] Resized PageObject not found on page or in history."
                           : $"Resized {pageObject.FormattedName}. Changed width by {FinalWidth - OriginalWidth} and height by {FinalHeight - OriginalHeight}";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            if (CurrentBatchTickIndex < 0)
            {
                return;
            }

            if (CurrentBatchTickIndex > NumberOfBatchTicks)
            {
                CurrentBatchTickIndex = NumberOfBatchTicks;
            }

            var pageObject = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID);
            if (pageObject == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Resized PageObject not found on page or in history.", HistoryActionIndex);
                CurrentBatchTickIndex = -1;
                return;
            }

            if (!StretchedDimensions.Any())
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Resized PageObject has no Streched Dimensions", HistoryActionIndex);
                CurrentBatchTickIndex = -1;
                return;
            }

            var initialWidth = pageObject.Width;
            var initialHeight = pageObject.Height;

            if (isAnimationUndo && CurrentBatchTickIndex > 0)
            {
                var stretchedDimension = StretchedDimensions[CurrentBatchTickIndex - 1];
                pageObject.Width = stretchedDimension.X;
                pageObject.Height = stretchedDimension.Y;
                CurrentBatchTickIndex--;
            }
            else
            {
                pageObject.Width = OriginalWidth;
                pageObject.Height = OriginalHeight;
                CurrentBatchTickIndex = -1;
            }

            pageObject.OnResized(initialWidth, initialHeight, true);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if (CurrentBatchTickIndex > NumberOfBatchTicks)
            {
                return;
            }

            if (CurrentBatchTickIndex < 0)
            {
                CurrentBatchTickIndex = 0;
            }

            var pageObject = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID);
            if (pageObject == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Resized PageObject not found on page or in history.", HistoryActionIndex);
                CurrentBatchTickIndex = NumberOfBatchTicks + 1;
                return;
            }

            if (!StretchedDimensions.Any())
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Resized PageObject has no Streched Dimensions", HistoryActionIndex);
                CurrentBatchTickIndex = NumberOfBatchTicks + 1;
                return;
            }

            var initialWidth = pageObject.Width;
            var initialHeight = pageObject.Height;

            if (isAnimationRedo)
            {
                var stretchedDimension = StretchedDimensions[CurrentBatchTickIndex];
                pageObject.Width = stretchedDimension.X;
                pageObject.Height = stretchedDimension.Y;
                CurrentBatchTickIndex++;
            }
            else
            {
                pageObject.Width = FinalWidth;
                pageObject.Height = FinalHeight;
                CurrentBatchTickIndex = NumberOfBatchTicks + 1;
            }

            pageObject.OnResized(initialWidth, initialHeight, true);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            if (clonedHistoryAction == null)
            {
                return null;
            }

            clonedHistoryAction.CurrentBatchTickIndex = -1;

            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return PageObjectID == id;
        }

        #endregion // AHistoryActionBase Overrides

        #region IHistoryBatch Implementation

        public int NumberOfBatchTicks => StretchedDimensions.Count - 1;

        /// <summary>Location within the Batch.</summary>
        public int CurrentBatchTickIndex
        {
            get { return GetValue<int>(CurrentBatchTickIndexProperty); }
            set { SetValue(CurrentBatchTickIndexProperty, value); }
        }

        public static readonly PropertyData CurrentBatchTickIndexProperty = RegisterProperty("CurrentBatchTickIndex", typeof(int), 0);

        public void ClearBatchAfterCurrentIndex()
        {
            var newBatch = new List<Point>();
            for (var i = 0; i < CurrentBatchTickIndex + 1; i++)
            {
                newBatch.Add(StretchedDimensions[i]);
            }
            StretchedDimensions = newBatch;
        }

        #endregion // IHistoryBatch Implementation
    }
}