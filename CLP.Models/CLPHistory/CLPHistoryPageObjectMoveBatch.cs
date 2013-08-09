﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryPageObjectMoveBatch : ACLPHistoryItemBase, IHistoryBatch
    {
        #region Constructors

        public CLPHistoryPageObjectMoveBatch(ICLPPage parentPage, string uniqueID, Point currentPosition)
            : base(parentPage)
        {
            PageObjectUniqueID = uniqueID;
            TravelledPositions = new List<Point> { currentPosition };
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryPageObjectMoveBatch(SerializationInfo info, StreamingContext context)
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
        /// The various points the pageObject has moved through during a single dragging operation.
        /// </summary>
        public List<Point> TravelledPositions
        {
            get { return GetValue<List<Point>>(TravelledPositionsProperty); }
            set { SetValue(TravelledPositionsProperty, value); }
        }

        public static readonly PropertyData TravelledPositionsProperty = RegisterProperty("TravelledPositions", typeof(List<Point>));

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
                return TravelledPositions.Count - 1;
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

        public void AddPositionPointToBatch(string uniqueID, Point currentPosition)
        {
            if(PageObjectUniqueID != uniqueID)
            {
                Logger.Instance.WriteToLog("Failed to add move operation to Batch. Mismatching UniqueIDs.");
                return;
            }

            TravelledPositions.Add(currentPosition);
        }

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            if(CurrentBatchTickIndex <= 0)
            {
                CurrentBatchTickIndex = 0;
                return;
            }

            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);

            if(isAnimationUndo)
            {
                var travelledPosition = TravelledPositions[CurrentBatchTickIndex - 1];

                if(pageObject.CanAcceptStrokes)
                {
                    var xDiff = travelledPosition.X - pageObject.XPosition;
                    var yDiff = travelledPosition.Y - pageObject.YPosition;
                    var moveStroke = new Matrix();
                    moveStroke.Translate(xDiff, yDiff);

                    var strokesToMove = pageObject.GetStrokesOverPageObject();
                    foreach(var stroke in strokesToMove)
                    {
                        stroke.Transform(moveStroke, true);
                    }
                }

                pageObject.XPosition = travelledPosition.X;
                pageObject.YPosition = travelledPosition.Y;
                CurrentBatchTickIndex--;
            }
            else if(TravelledPositions.Any())
            {
                var originalPosition = TravelledPositions.First();

                if(pageObject.CanAcceptStrokes) //TODO: move children pageObjects if pageObject.CannAcceptPageObjects
                {
                    var xDiff = originalPosition.X - pageObject.XPosition;
                    var yDiff = originalPosition.Y - pageObject.YPosition;
                    var moveStroke = new Matrix();
                    moveStroke.Translate(xDiff, yDiff);

                    var strokesToMove = pageObject.GetStrokesOverPageObject();
                    foreach(var stroke in strokesToMove)
                    {
                        stroke.Transform(moveStroke, true);
                    }
                }

                pageObject.XPosition = originalPosition.X;
                pageObject.YPosition = originalPosition.Y;
                CurrentBatchTickIndex = 0;
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
            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);

            if(isAnimationRedo)
            {
                var travelledPosition = TravelledPositions[CurrentBatchTickIndex];

                if(pageObject.CanAcceptStrokes)
                {
                    var xDiff = travelledPosition.X - pageObject.XPosition;
                    var yDiff = travelledPosition.Y - pageObject.YPosition;
                    var moveStroke = new Matrix();
                    moveStroke.Translate(xDiff, yDiff);

                    var strokesToMove = pageObject.GetStrokesOverPageObject();
                    foreach(var stroke in strokesToMove)
                    {
                        stroke.Transform(moveStroke, true);
                    }
                }

                pageObject.XPosition = travelledPosition.X;
                pageObject.YPosition = travelledPosition.Y;
                CurrentBatchTickIndex++;
            }
            else if(TravelledPositions.Any())
            {
                var lastPosition = TravelledPositions.Last();

                if(pageObject.CanAcceptStrokes)
                {
                    var xDiff = lastPosition.X - pageObject.XPosition;
                    var yDiff = lastPosition.Y - pageObject.YPosition;
                    var moveStroke = new Matrix();
                    moveStroke.Translate(xDiff, yDiff);

                    var strokesToMove = pageObject.GetStrokesOverPageObject();
                    foreach(var stroke in strokesToMove)
                    {
                        stroke.Transform(moveStroke, true);
                    }
                }

                pageObject.XPosition = lastPosition.X;
                pageObject.YPosition = lastPosition.Y;
                CurrentBatchTickIndex = NumberOfBatchTicks;
            }
        }

        public void ClearBatchAfterCurrentIndex()
        {
            var newBatch = new List<Point>();
            for(int i = 0; i < CurrentBatchTickIndex + 1; i++)
            {
                newBatch.Add(TravelledPositions[i]);
            }
            TravelledPositions = new List<Point>(newBatch);
        }

        #endregion //Methods
    }
}