using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryMoveObject : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryMoveObject(String objectID, double oldX,
                double oldY, double newX, double newY) : base(HistoryItemType.MoveObject)
        {
            ObjectId = objectID;
            OldX = oldX;
            OldY = oldY;
            NewX = newX;
            NewY = newY;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryMoveObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Page object moved in this historical event
        /// </summary>
        public String ObjectId
        {
            get { return GetValue<String>(ObjectIdProperty); }
            set { SetValue(ObjectIdProperty, value); }
        }

        public static readonly PropertyData ObjectIdProperty = RegisterProperty("ObjectId", typeof(String), null);
        
        /// <summary>
        /// The page object's X position before the resizing
        /// </summary>
        public double OldX
        {
            get { return GetValue<double>(OldXProperty); }
            set { SetValue(OldXProperty, value); }
        }

        public static readonly PropertyData OldXProperty = RegisterProperty("OldX", typeof(double), null);

        /// <summary>
        /// The page object's Y position before the resizing
        /// </summary>
        public double OldY
        {
            get { return GetValue<double>(OldYProperty); }
            set { SetValue(OldYProperty, value); }
        }

        public static readonly PropertyData OldYProperty = RegisterProperty("OldY", typeof(double), null);

        /// <summary>
        /// The page object's X position after the resizing
        /// </summary>
        public double NewX
        {
            get { return GetValue<double>(NewXProperty); }
            set { SetValue(NewXProperty, value); }
        }

        public static readonly PropertyData NewXProperty = RegisterProperty("NewX", typeof(double), null);

        /// <summary>
        /// The page object's Y position after the resizing
        /// </summary>
        public double NewY
        {
            get { return GetValue<double>(NewYProperty); }
            set { SetValue(NewYProperty, value); }
        }

        public static readonly PropertyData NewYProperty = RegisterProperty("NewY", typeof(double), null);

        #endregion //Properties

        #region Methods

        public override CLPHistoryItem GetUndoFingerprint(CLPPage page)
        {
            return new CLPHistoryMoveObject(ObjectId, NewX, NewY, OldX, OldY);
        }

        public override CLPHistoryItem GetRedoFingerprint(CLPPage page)
        {
            return new CLPHistoryMoveObject(ObjectId, OldX, OldY, NewX, NewY);
        }

        override public void Undo(CLPPage page)
        {
            ICLPPageObject obj = GetPageObjectByUniqueID(page, ObjectId);
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

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryMoveObject)) 
            {
                return false;
            }
            CLPHistoryMoveObject other = obj as CLPHistoryMoveObject;
            if (other.ObjectId != ObjectId ||
                other.OldX != OldX ||
                other.OldY != OldY ||
                other.NewX != NewX ||
                other.NewY != NewY)
            {
                return false;
            }
            return base.Equals(obj);
        }

        public bool CombinesWith(CLPHistoryItem other)
        {
            if(other is CLPHistoryMoveObject)
            {
                if((other as CLPHistoryMoveObject).ObjectId == ObjectId)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion //Methods
    }
}
