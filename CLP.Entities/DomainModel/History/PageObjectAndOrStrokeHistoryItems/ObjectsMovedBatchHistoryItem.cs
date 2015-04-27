﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class ObjectsMovedBatchHistoryItem : AHistoryItemBase, IHistoryBatch
    {
        #region Constructors

        /// <summary>Initializes <see cref="ObjectsMovedBatchHistoryItem" /> from scratch.</summary>
        public ObjectsMovedBatchHistoryItem() { }

        /// <summary>Initializes <see cref="ObjectsMovedBatchHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public ObjectsMovedBatchHistoryItem(CLPPage parentPage, Person owner, string pageObjectID, Point currentPosition)
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

        /// <summary>Initializes <see cref="ObjectsMovedBatchHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public ObjectsMovedBatchHistoryItem(CLPPage parentPage, Person owner, Dictionary<string, Point> pageObjectIDs, Dictionary<string, Point> strokeIDs, Point currentPosition)
            : base(parentPage, owner)
        {
            PageObjectIDs = pageObjectIDs;
            StrokeIDs = strokeIDs;
            TravelledPositions = new List<Point>
                                 {
                                     currentPosition
                                 };
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected ObjectsMovedBatchHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Converter

        /// <summary>Initializes <see cref="ObjectsMovedBatchHistoryItem" /> from <see cref="PageObjectMoveBatchHistoryItem" />.</summary>
        public ObjectsMovedBatchHistoryItem(PageObjectMoveBatchHistoryItem obsoleteHistoryItem)
        {
            ID = obsoleteHistoryItem.ID;
            OwnerID = obsoleteHistoryItem.OwnerID;
            VersionIndex = obsoleteHistoryItem.VersionIndex;
            LastVersionIndex = obsoleteHistoryItem.LastVersionIndex;
            DifferentiationGroup = obsoleteHistoryItem.DifferentiationGroup;
            ParentPage = obsoleteHistoryItem.ParentPage;

            CurrentBatchTickIndex = obsoleteHistoryItem.CurrentBatchTickIndex;
            PageObjectIDs = new Dictionary<string, Point>
                            {
                                { obsoleteHistoryItem.PageObjectID, new Point(0.0, 0.0) }
                            };

            TravelledPositions = obsoleteHistoryItem.TravelledPositions;
        }

        /// <summary>Initializes <see cref="ObjectsMovedBatchHistoryItem" /> from <see cref="PageObjectsMoveBatchHistoryItem" />.</summary>
        public ObjectsMovedBatchHistoryItem(PageObjectsMoveBatchHistoryItem obsoleteHistoryItem)
        {
            ID = obsoleteHistoryItem.ID;
            OwnerID = obsoleteHistoryItem.OwnerID;
            VersionIndex = obsoleteHistoryItem.VersionIndex;
            LastVersionIndex = obsoleteHistoryItem.LastVersionIndex;
            DifferentiationGroup = obsoleteHistoryItem.DifferentiationGroup;
            ParentPage = obsoleteHistoryItem.ParentPage;

            CurrentBatchTickIndex = obsoleteHistoryItem.CurrentBatchTickIndex;
            var pageObjects = obsoleteHistoryItem.PageObjectIDs.Select(id => obsoleteHistoryItem.ParentPage.GetVerifiedPageObjectOnPageByID(id)).ToList();
            pageObjects = pageObjects.Where(p => p != null).ToList();
            var referencePageObject = pageObjects.First();
            var pageObjectIDs = pageObjects.Where(p => p != null).ToDictionary(p => p.ID, p => new Point(p.XPosition - referencePageObject.XPosition, p.YPosition - referencePageObject.YPosition));
            PageObjectIDs = pageObjectIDs;
            TravelledPositions = obsoleteHistoryItem.TravelledPositions;
        }

        #endregion //Converter

        #region Properties

        /// <summary>List of the <see cref="IPageObject" />'s IDs that were moved and their x/y offset from the TravelledPositions Point.</summary>
        public Dictionary<string, Point> PageObjectIDs
        {
            get { return GetValue<Dictionary<string, Point>>(PageObjectIDsProperty); }
            set { SetValue(PageObjectIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsProperty = RegisterProperty("PageObjectIDs", typeof (Dictionary<string, Point>), () => new Dictionary<string, Point>());

        /// <summary>List of the Stroke's IDs that were moved and their x/y offset from the TravelledPositions Point.</summary>
        public Dictionary<string, Point> StrokeIDs
        {
            get { return GetValue<Dictionary<string, Point>>(StrokeIDsProperty); }
            set { SetValue(StrokeIDsProperty, value); }
        }

        public static readonly PropertyData StrokeIDsProperty = RegisterProperty("StrokeIDs", typeof (Dictionary<string, Point>), () => new Dictionary<string, Point>());

        /// <summary>The various points the pageObject has moved through during a single dragging operation.</summary>
        public List<Point> TravelledPositions
        {
            get { return GetValue<List<Point>>(TravelledPositionsProperty); }
            set { SetValue(TravelledPositionsProperty, value); }
        }

        public static readonly PropertyData TravelledPositionsProperty = RegisterProperty("TravelledPositions", typeof (List<Point>));

        public int NumberOfBatchTicks
        {
            get { return TravelledPositions.Count - 1; }
        }

        /// <summary>Location within the Batch.</summary>
        public int CurrentBatchTickIndex
        {
            get { return GetValue<int>(CurrentBatchTickIndexProperty); }
            set { SetValue(CurrentBatchTickIndexProperty, value); }
        }

        public static readonly PropertyData CurrentBatchTickIndexProperty = RegisterProperty("CurrentBatchTickIndex", typeof (int), 0);

        public override string FormattedValue
        {
            get
            {
                var PageObjectTypes = new List<string>();

                //TODO: fix formatting to use offsets
                //foreach (var pageObject in PageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByIDOnPageOrInHistory(pageObjectID)))
                //{
                //    if (pageObject == null)
                //    {
                //        continue;
                //    }
                //    PageObjectTypes.Add(pageObject.GetType().Name);
                //}

                //var objectsMoved = string.Empty;
                //if (PageObjectTypes.Any())
                //{
                //    objectsMoved = string.Format("Moved {0} on page. ", string.Join(", ", PageObjectTypes));
                //    if (PageObjectTypes.Count == 1 &&
                //        PageObjectTypes[0] == "CLPArray")
                //    {
                //        var movedArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(PageObjectIDs[0]) as CLPArray;
                //        objectsMoved = string.Format("Moved CLPArray [{0} x {1}] on page. ", movedArray.Rows, movedArray.Columns);
                //    }
                //}

                var strokesMoved = string.Empty;
                if (StrokeIDs.Any())
                {
                    return "Strokes Moved";
                    //strokesMoved = "Moved strokes on page.";
                }

                return "PageObjects Moved";
                //var formattedValue = string.Format("Index # {0}, {1} {2}", HistoryIndex, objectsMoved, strokesMoved);
                //return formattedValue;
            }
        }

        #endregion //Properties

        #region Methods

        public void AddPositionPointToBatch(Point currentPosition) { TravelledPositions.Add(currentPosition); }

        protected override void ConversionUndoAction()
        {
            UndoAction(false);
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

            if (!PageObjectIDs.Any())
            {
                Console.WriteLine("ERROR: PageObjectIDs is empty on ObjectsMovedBatch.");
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
                pageObject.OnMoved(initialX, initialY);
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
                Console.WriteLine("ERROR: PageObjectIDs is empty on ObjectsMovedBatch.");
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

                if (isAnimationRedo)
                {
                    var travelledPosition = TravelledPositions[CurrentBatchTickIndex];

                    pageObject.XPosition = travelledPosition.X + pageObjectID.Value.X;
                    pageObject.YPosition = travelledPosition.Y + pageObjectID.Value.Y;
                    if (CurrentBatchTickIndex == NumberOfBatchTicks)
                    {
                        pageObject.YPosition = travelledPosition.Y;
                    }
                    CurrentBatchTickIndex++;
                }
                else if (TravelledPositions.Any())
                {
                    var lastPosition = TravelledPositions.Last();

                    pageObject.XPosition = lastPosition.X + pageObjectID.Value.X;
                    pageObject.YPosition = lastPosition.Y + pageObjectID.Value.Y;
                    CurrentBatchTickIndex = NumberOfBatchTicks + 1;
                }
                pageObject.OnMoved(initialX, initialY);
            }
        }

        public void ClearBatchAfterCurrentIndex()
        {
            var newBatch = new List<Point>();
            for (var i = 0; i < CurrentBatchTickIndex + 1; i++)
            {
                newBatch.Add(TravelledPositions[i]);
            }
            TravelledPositions = new List<Point>(newBatch);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as ObjectsMovedBatchHistoryItem;
            if (clonedHistoryItem == null)
            {
                return null;
            }

            clonedHistoryItem.CurrentBatchTickIndex = -1;

            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}