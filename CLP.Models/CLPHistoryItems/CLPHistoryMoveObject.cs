using System;
using System.Collections;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryMoveObject : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryMoveObject(CLPPage page, ICLPPageObject pageObject, double oldX,
                double oldY, double newX, double newY) : base(HistoryItemType.MoveObject, page)
        {
            PageObject = pageObject;
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
        /// Page objects resized in this historical event
        /// </summary>
        public ICLPPageObject PageObject
        {
            get { return GetValue<ICLPPageObject>(PageObjectProperty); }
            set { SetValue(PageObjectProperty, value); }
        }

        public static readonly PropertyData PageObjectProperty = RegisterProperty("PageObject", typeof(ICLPPageObject), null);

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

        public override CLPHistoryItem GetUndoFingerprint()
        {
            return new CLPHistoryMoveObject(Page, PageObject, NewX, NewY, OldX, OldY);
        }

        public override CLPHistoryItem GetRedoFingerprint()
        {
            return new CLPHistoryMoveObject(Page, PageObject, OldX, OldY, NewX, NewY);
        }

        override public void Undo()
        {
            PageObject.XPosition = OldX;
            PageObject.YPosition = OldY;
        }

        override public void Redo()
        {
            PageObject.XPosition = NewX;
            PageObject.YPosition = NewY;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryMoveObject)) 
            {
                return false;
            }
            CLPHistoryMoveObject other = obj as CLPHistoryMoveObject;
            if (other.PageObject.UniqueID != PageObject.UniqueID ||
                other.OldX != OldX ||
                other.OldY != OldY ||
                other.NewX != NewX ||
                other.NewY != NewY)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}
