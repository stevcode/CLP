using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class PageObjectsMoveBatchHistoryItem : AHistoryItemBase, IHistoryBatch
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="PageObjectsMoveBatchHistoryItem" /> from scratch.
        /// </summary>
        public PageObjectsMoveBatchHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="PageObjectsMoveBatchHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        public PageObjectsMoveBatchHistoryItem(CLPPage parentPage, Person owner, List<string> pageObjectIDs, Point currentPosition)
            : base(parentPage, owner)
        {
            PageObjectIDs = pageObjectIDs;
            TravelledPositions = new List<Point>
                                 {
                                     currentPosition
                                 };
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected PageObjectsMoveBatchHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// List of the <see cref="IPageObject" />'s IDs that were moved.
        /// </summary>
        public List<string> PageObjectIDs
        {
            get { return GetValue<List<string>>(PageObjectIDsProperty); }
            set { SetValue(PageObjectIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsProperty = RegisterProperty("PageObjectIDs", typeof(List<string>), () => new List<string>());

         /// <summary>
        /// The various points the pageObject has moved through during a single dragging operation.
        /// </summary>
        public List<Point> TravelledPositions
        {
            get { return GetValue<List<Point>>(TravelledPositionsProperty); }
            set { SetValue(TravelledPositionsProperty, value); }
        }

        public static readonly PropertyData TravelledPositionsProperty = RegisterProperty("TravelledPositions", typeof(List<Point>));

        public int NumberOfBatchTicks
        {
            get { return TravelledPositions.Count - 1; }
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

        public void AddPositionPointToBatch(Point currentPosition)
        {
            TravelledPositions.Add(currentPosition);
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

            foreach(var pageObject in PageObjectIDs.Select(id => ParentPage.GetPageObjectByID(ID)))
            {
                if(pageObject == null)
                {
                    Console.WriteLine("ERROR: PageObject not  found on page for UNDO of PageObjectsMoveBatch.");
                    continue;
                }

                var initialX = pageObject.XPosition;
                var initialY = pageObject.YPosition;

                if(isAnimationUndo && CurrentBatchTickIndex > 0)
                {
                    var travelledPosition = TravelledPositions[CurrentBatchTickIndex - 1];

                    //if(pageObject.CanAcceptStrokes)
                    //{
                    //    var xDiff = travelledPosition.X - pageObject.XPosition;
                    //    var yDiff = travelledPosition.Y - pageObject.YPosition;
                    //    var moveStroke = new Matrix();
                    //    moveStroke.Translate(xDiff, yDiff);

                    //    var strokesToMove = pageObject.GetStrokesOverPageObject();
                    //    foreach(var stroke in strokesToMove)
                    //    {
                    //        stroke.Transform(moveStroke, true);
                    //    }
                    //}

                    //TODO: move child pageObjects as well, also for REDO

                    pageObject.XPosition = travelledPosition.X;
                    pageObject.YPosition = travelledPosition.Y;
                    CurrentBatchTickIndex--;
                }
                else if(TravelledPositions.Any())
                {
                    var originalPosition = TravelledPositions.First();

                    //if(pageObject.CanAcceptStrokes) //TODO: move children pageObjects if pageObject.CannAcceptPageObjects
                    //{
                    //    var xDiff = originalPosition.X - pageObject.XPosition;
                    //    var yDiff = originalPosition.Y - pageObject.YPosition;
                    //    var moveStroke = new Matrix();
                    //    moveStroke.Translate(xDiff, yDiff);

                    //    var strokesToMove = pageObject.GetStrokesOverPageObject();
                    //    foreach(var stroke in strokesToMove)
                    //    {
                    //        stroke.Transform(moveStroke, true);
                    //    }
                    //}

                    pageObject.XPosition = originalPosition.X;
                    pageObject.YPosition = originalPosition.Y;
                    CurrentBatchTickIndex = -1;
                }
                pageObject.OnMoved(initialX, initialY);
            }
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

            foreach(var pageObject in PageObjectIDs.Select(id => ParentPage.GetPageObjectByID(ID)))
            {
                if(pageObject == null)
                {
                    Console.WriteLine("ERROR: PageObject not  found on page for REDO of PageObjectMoveBatch.");
                    continue;
                }

                var initialX = pageObject.XPosition;
                var initialY = pageObject.YPosition;

                if(isAnimationRedo)
                {
                    var travelledPosition = TravelledPositions[CurrentBatchTickIndex];

                    //if(pageObject.CanAcceptStrokes)
                    //{
                    //    var xDiff = travelledPosition.X - pageObject.XPosition;
                    //    var yDiff = travelledPosition.Y - pageObject.YPosition;
                    //    var moveStroke = new Matrix();
                    //    moveStroke.Translate(xDiff, yDiff);

                    //    var strokesToMove = pageObject.GetStrokesOverPageObject();
                    //    foreach(var stroke in strokesToMove)
                    //    {
                    //        stroke.Transform(moveStroke, true);
                    //    }
                    //}

                    pageObject.XPosition = travelledPosition.X;
                    pageObject.YPosition = travelledPosition.Y;
                    CurrentBatchTickIndex++;
                }
                else if(TravelledPositions.Any())
                {
                    var lastPosition = TravelledPositions.Last();

                    //if(pageObject.CanAcceptStrokes)
                    //{
                    //    var xDiff = lastPosition.X - pageObject.XPosition;
                    //    var yDiff = lastPosition.Y - pageObject.YPosition;
                    //    var moveStroke = new Matrix();
                    //    moveStroke.Translate(xDiff, yDiff);

                    //    var strokesToMove = pageObject.GetStrokesOverPageObject();
                    //    foreach(var stroke in strokesToMove)
                    //    {
                    //        stroke.Transform(moveStroke, true);
                    //    }
                    //}

                    pageObject.XPosition = lastPosition.X;
                    pageObject.YPosition = lastPosition.Y;
                    CurrentBatchTickIndex = NumberOfBatchTicks + 1;
                }
                pageObject.OnMoved(initialX, initialY);
            }
        }

        public void ClearBatchAfterCurrentIndex()
        {
            var newBatch = new List<Point>();
            for(var i = 0; i < CurrentBatchTickIndex + 1; i++)
            {
                newBatch.Add(TravelledPositions[i]);
            }
            TravelledPositions = new List<Point>(newBatch);
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as PageObjectsMoveBatchHistoryItem;
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