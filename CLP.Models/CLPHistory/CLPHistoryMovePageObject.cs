using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryMovePageObject : ACLPHistoryItemBase
    {
        private const int MOVE_PAGE_OBJECT_DELAY = 100;

        #region Constructors

        public CLPHistoryMovePageObject(ICLPPage parentPage, string uniqueID, Point currentPosition)
            : base(parentPage)
        {
            PageObjectUniqueID = uniqueID;
            TravelledPositions = new ObservableCollection<Point> {currentPosition};
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryMovePageObject(SerializationInfo info, StreamingContext context)
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
        public ObservableCollection<Point> TravelledPositions
        {
            get { return GetValue<ObservableCollection<Point>>(TravelledPositionsProperty); }
            set { SetValue(TravelledPositionsProperty, value); }
        }

        public static readonly PropertyData TravelledPositionsProperty = RegisterProperty("TravelledPositions", typeof(ObservableCollection<Point>));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);

            if(isAnimationUndo)
            {
                foreach(var travelledPosition in TravelledPositions.Reverse())
                {
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
                    Thread.Sleep(MOVE_PAGE_OBJECT_DELAY);
                }
            }
            else if(TravelledPositions.Any())
            {
                var originalPosition = TravelledPositions.FirstOrDefault();

                if(pageObject.CanAcceptStrokes)
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
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);

            if(isAnimationRedo)
            {
                foreach(var travelledPosition in TravelledPositions)
                {
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
                    Thread.Sleep(MOVE_PAGE_OBJECT_DELAY);
                }
            }
            else if(TravelledPositions.Any())
            {
                var lastPosition = TravelledPositions.Reverse().FirstOrDefault();

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
            }
        }

        #endregion //Methods
    }
}
