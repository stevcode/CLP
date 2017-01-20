using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class ObjectsMovedBatchHistoryAction : AHistoryActionBase, IHistoryBatch
    {
        #region Constructors

        /// <summary>Initializes <see cref="ObjectsMovedBatchHistoryAction" /> from scratch.</summary>
        public ObjectsMovedBatchHistoryAction() { }

        /// <summary>Initializes <see cref="ObjectsMovedBatchHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        public ObjectsMovedBatchHistoryAction(CLPPage parentPage, Person owner, string pageObjectID, Point currentPosition)
            : base(parentPage, owner)
        {
            PageObjectIDs = new Dictionary<string, Point>
                            {
                                { pageObjectID, new Point(0.0, 0.0) }
                            };
            TravelledPositions = new List<Point>
                                 {
                                     currentPosition
                                 };
        }

        /// <summary>Initializes <see cref="ObjectsMovedBatchHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        public ObjectsMovedBatchHistoryAction(CLPPage parentPage, Person owner, Dictionary<string, Point> pageObjectIDs, Dictionary<string, Point> strokeIDs, Point currentPosition)
            : base(parentPage, owner)
        {
            PageObjectIDs = pageObjectIDs;
            StrokeIDs = strokeIDs;
            TravelledPositions = new List<Point>
                                 {
                                     currentPosition
                                 };
        }

        #endregion // Constructors

        #region Converter

        ///// <summary>Initializes <see cref="ObjectsMovedBatchHistoryAction" /> from <see cref="PageObjectMoveBatchHistoryItem" />.</summary>
        public ObjectsMovedBatchHistoryAction(PageObjectMoveBatchHistoryItem obsoleteHistoryItem)
        {
            ID = obsoleteHistoryItem.ID;
            OwnerID = obsoleteHistoryItem.OwnerID;
            ParentPage = obsoleteHistoryItem.ParentPage;

            CurrentBatchTickIndex = obsoleteHistoryItem.CurrentBatchTickIndex;
            PageObjectIDs = new Dictionary<string, Point>
                            {
                                { obsoleteHistoryItem.PageObjectID, new Point(0.0, 0.0) }
                            };

            TravelledPositions = obsoleteHistoryItem.TravelledPositions;
        }

        ///// <summary>Initializes <see cref="ObjectsMovedBatchHistoryAction" /> from <see cref="PageObjectsMoveBatchHistoryItem" />.</summary>
        //public ObjectsMovedBatchHistoryAction(PageObjectsMoveBatchHistoryItem obsoleteHistoryItem)
        //{
        //    ID = obsoleteHistoryItem.ID;
        //    OwnerID = obsoleteHistoryItem.OwnerID;
        //    ParentPage = obsoleteHistoryItem.ParentPage;

        //    CurrentBatchTickIndex = obsoleteHistoryItem.CurrentBatchTickIndex;
        //    var pageObjects = obsoleteHistoryItem.PageObjectIDs.Select(id => obsoleteHistoryItem.ParentPage.GetVerifiedPageObjectOnPageByID(id)).ToList();
        //    pageObjects = pageObjects.Where(p => p != null).ToList();
        //    var referencePageObject = pageObjects.First();
        //    var pageObjectIDs = pageObjects.Where(p => p != null).ToDictionary(p => p.ID, p => new Point(p.XPosition - referencePageObject.XPosition, p.YPosition - referencePageObject.YPosition));
        //    PageObjectIDs = pageObjectIDs;
        //    TravelledPositions = obsoleteHistoryItem.TravelledPositions;
        //}

        #endregion // Converter

        #region Properties

        /// <summary>List of the <see cref="IPageObject" />'s IDs that were moved and their x/y offset from the TravelledPositions Point.</summary>
        public Dictionary<string, Point> PageObjectIDs
        {
            get { return GetValue<Dictionary<string, Point>>(PageObjectIDsProperty); }
            set { SetValue(PageObjectIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsProperty = RegisterProperty("PageObjectIDs", typeof(Dictionary<string, Point>), () => new Dictionary<string, Point>());

        /// <summary>List of the Stroke's IDs that were moved and their x/y offset from the TravelledPositions Point.</summary>
        public Dictionary<string, Point> StrokeIDs
        {
            get { return GetValue<Dictionary<string, Point>>(StrokeIDsProperty); }
            set { SetValue(StrokeIDsProperty, value); }
        }

        public static readonly PropertyData StrokeIDsProperty = RegisterProperty("StrokeIDs", typeof(Dictionary<string, Point>), () => new Dictionary<string, Point>());

        /// <summary>The various points the pageObject has moved through during a single dragging operation.</summary>
        public List<Point> TravelledPositions
        {
            get { return GetValue<List<Point>>(TravelledPositionsProperty); }
            set { SetValue(TravelledPositionsProperty, value); }
        }

        public static readonly PropertyData TravelledPositionsProperty = RegisterProperty("TravelledPositions", typeof(List<Point>), () => new List<Point>());

        #endregion // Properties

        #region Methods

        public void AddPositionPointToBatch(Point currentPosition)
        {
            TravelledPositions.Add(currentPosition);
        }

        #endregion // Methods

        #region AHistoryActionBase Overrides

        protected override string FormattedReport
        {
            get
            {
                var pageObjectsMoved = PageObjectIDs.Keys.Select(id => ParentPage.GetPageObjectByIDOnPageOrInHistory(id)).Where(p => p != null).ToList();

                var objectsMoved = pageObjectsMoved.Any() ? $" Moved {string.Join(",", pageObjectsMoved.Select(p => p.FormattedName))}." : string.Empty;
                var strokesMoved = StrokeIDs.Keys.Any() ? StrokeIDs.Keys.Count == 1 ? " Moved 1 stroke." : $" Moved {StrokeIDs.Keys.Count} strokes." : string.Empty;

                return $"{objectsMoved}{strokesMoved}";
            }
        }

        protected override void ConversionUndoAction() { }

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

            if (!PageObjectIDs.Any())
            {
                Debug.WriteLine("ERROR: PageObjectIDs is empty on ObjectsMovedBatch.");
                CurrentBatchTickIndex = -1;
                return;
            }

            foreach (var pageObjectID in PageObjectIDs)
            {
                var pageObject = ParentPage.GetVerifiedPageObjectOnPageByID(pageObjectID.Key);

                if (pageObject == null)
                {
                    continue;
                }

                var initialX = pageObject.XPosition;
                var initialY = pageObject.YPosition;

                if (isAnimationUndo && CurrentBatchTickIndex > 0)
                {
                    var travelledPosition = TravelledPositions[CurrentBatchTickIndex - 1];

                    pageObject.XPosition = travelledPosition.X + pageObjectID.Value.X;
                    pageObject.YPosition = travelledPosition.Y + pageObjectID.Value.Y;
                    CurrentBatchTickIndex--;
                }
                else if (TravelledPositions.Any())
                {
                    var originalPosition = TravelledPositions.First();

                    pageObject.XPosition = originalPosition.X + pageObjectID.Value.X;
                    pageObject.YPosition = originalPosition.Y + pageObjectID.Value.Y;
                    CurrentBatchTickIndex = -1;
                }
                pageObject.OnMoved(initialX, initialY, true);
            }
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

            if (!PageObjectIDs.Any())
            {
                Debug.WriteLine("ERROR: PageObjectIDs is empty on ObjectsMovedBatch.");
                CurrentBatchTickIndex = NumberOfBatchTicks + 1;
                return;
            }

            foreach (var pageObjectID in PageObjectIDs)
            {
                var pageObject = ParentPage.GetVerifiedPageObjectOnPageByID(pageObjectID.Key);

                if (pageObject == null)
                {
                    continue;
                }
                var initialX = pageObject.XPosition;
                var initialY = pageObject.YPosition;

                if (isAnimationRedo && CurrentBatchTickIndex < TravelledPositions.Count)
                {
                    var travelledPosition = TravelledPositions[CurrentBatchTickIndex];

                    pageObject.XPosition = travelledPosition.X + pageObjectID.Value.X;
                    pageObject.YPosition = travelledPosition.Y + pageObjectID.Value.Y;

                    CurrentBatchTickIndex++;
                }
                else if (TravelledPositions.Any())
                {
                    var lastPosition = TravelledPositions.Last();

                    pageObject.XPosition = lastPosition.X + pageObjectID.Value.X;
                    pageObject.YPosition = lastPosition.Y + pageObjectID.Value.Y;
                    CurrentBatchTickIndex = NumberOfBatchTicks + 1;
                }
                pageObject.OnMoved(initialX, initialY, true);
            }
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
            return PageObjectIDs.Keys.Contains(id);
        }

        public override bool IsUsingTrashedInkStroke(string id)
        {
            return StrokeIDs.Keys.Contains(id);
        }

        #endregion // AHistoryActionBase Overrides

        #region IHistoryBatch Implementation

        public int NumberOfBatchTicks => TravelledPositions.Count - 1;

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
                newBatch.Add(TravelledPositions[i]);
            }
            TravelledPositions = new List<Point>(newBatch);
        }

        #endregion // IHistoryBatch Implementation
    }
}