using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryMovePageObject : ACLPHistoryItemBase
    {
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

        override public void Undo(CLPPage page)
        {
            ICLPPageObject obj = GetPageObjectByUniqueID(page, ObjectId);
            if(obj==null){
                return;
            }
            double currentX = obj.XPosition;
            double currentY = obj.YPosition;
            obj.XPosition = OldX;
            obj.YPosition = OldY;
            if(obj.CanAcceptPageObjects)
            {
                foreach(ICLPPageObject pageObject in obj.GetPageObjectsOverPageObject())
                {
                    pageObject.XPosition = (OldX - currentX + pageObject.XPosition);
                    pageObject.YPosition = (OldY - currentY + pageObject.YPosition);
                }
            }
            if(obj.CanAcceptStrokes)
            {
                Matrix moveStroke = new Matrix();
                moveStroke.Translate(OldX - currentX, OldY - currentY);

                StrokeCollection strokesToMove = obj.GetStrokesOverPageObject();
                foreach(Stroke stroke in strokesToMove)
                {
                    stroke.Transform(moveStroke, true);
                }
            }
        }

        override public void Redo(CLPPage page)
        {
            ICLPPageObject obj = GetPageObjectByUniqueID(page, ObjectId);
            if(obj==null){
                return;
            }
            double currentX = obj.XPosition;
            double currentY = obj.YPosition;
            obj.XPosition = NewX;
            obj.YPosition = NewY;
            if(obj.CanAcceptPageObjects)
            {
                foreach(ICLPPageObject pageObject in obj.GetPageObjectsOverPageObject())
                {
                    pageObject.XPosition = (NewX - currentX + pageObject.XPosition);
                    pageObject.YPosition = (NewY - currentY + pageObject.YPosition);
                }
            }
            if(obj.CanAcceptStrokes)
            {
                Matrix moveStroke = new Matrix();
                moveStroke.Translate(NewX - currentX, NewY - currentY);

                StrokeCollection strokesToMove = obj.GetStrokesOverPageObject();
                foreach(Stroke stroke in strokesToMove)
                {
                    stroke.Transform(moveStroke, true);
                }
            }
        }

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);
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
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);
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
            }
        }

        #endregion //Methods
    }
}
