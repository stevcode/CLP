using System;
using System.Collections;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryRemoveArrayLine : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryRemoveArrayLine(CLPPage page, CLPArray array, CLPArrayDivision oldDiv,
            CLPArrayDivision newTopDiv, CLPArrayDivision newBottomDiv)
            : base(HistoryItemType.RemoveArrayLine, page)
        {
            Array = array;
            OldDivision = oldDiv;
            NewTopDivision = newTopDiv;
            NewBottomDivision = newBottomDiv;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryRemoveArrayLine(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Array to which a line was added
        /// </summary>
        public CLPArray Array
        {
            get { return GetValue<CLPArray>(ArrayProperty); }
            set { SetValue(ArrayProperty, value); }
        }

        public static readonly PropertyData ArrayProperty =
            RegisterProperty("Array", typeof(CLPArray), null);

        /// <summary>
        /// Division replaced by two new ones
        /// </summary>
        public CLPArrayDivision OldDivision
        {
            get { return GetValue<CLPArrayDivision>(OldDivisionProperty); }
            set { SetValue(OldDivisionProperty, value); }
        }

        public static readonly PropertyData OldDivisionProperty =
            RegisterProperty("OldDivision", typeof(CLPArrayDivision), null);

        /// <summary>
        /// New division replacing the top portion of the old one
        /// </summary>
        public CLPArrayDivision NewTopDivision
        {
            get { return GetValue<CLPArrayDivision>(NewTopDivisionProperty); }
            set { SetValue(NewTopDivisionProperty, value); }
        }

        public static readonly PropertyData NewTopDivisionProperty =
            RegisterProperty("NewTopDivision", typeof(CLPArrayDivision), null);

        /// <summary>
        /// Division added to the array
        /// </summary>
        public CLPArrayDivision NewBottomDivision
        {
            get { return GetValue<CLPArrayDivision>(NewBottomDivisionProperty); }
            set { SetValue(NewBottomDivisionProperty, value); }
        }

        public static readonly PropertyData NewBottomDivisionProperty =
            RegisterProperty("NewBottomDivision", typeof(CLPArrayDivision), null);

        #endregion //Properties

        #region Methods

        public override void Undo()
        {
            if(NewTopDivision.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                Array.HorizontalDivisions.Add(NewTopDivision);
                Array.HorizontalDivisions.Add(NewBottomDivision);
                if(OldDivision != null)
                {
                    Array.HorizontalDivisions.Remove(OldDivision);
                }
            }
            else
            {
                Array.VerticalDivisions.Add(NewTopDivision);
                Array.VerticalDivisions.Add(NewBottomDivision);
                if(OldDivision != null)
                {
                    Array.VerticalDivisions.Remove(OldDivision);
                }
            }
        }

        override public void Redo()
        {
            if(NewTopDivision.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                Array.HorizontalDivisions.Remove(NewTopDivision);
                Array.HorizontalDivisions.Remove(NewBottomDivision);
                if(OldDivision != null)
                {
                    Array.HorizontalDivisions.Add(OldDivision);
                }
            }
            else
            {
                Array.VerticalDivisions.Remove(NewTopDivision);
                Array.VerticalDivisions.Remove(NewBottomDivision);
                if(OldDivision != null)
                {
                    Array.VerticalDivisions.Add(OldDivision);
                }
            }
        }

        override public CLPHistoryItem GetUndoFingerprint()
        {
            return null;
        }

        override public CLPHistoryItem GetRedoFingerprint()
        {
            return null;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryRemoveArrayLine) ||
                (obj as CLPHistoryRemoveArrayLine).Array.UniqueID != Array.UniqueID ||
                (obj as CLPHistoryRemoveArrayLine).OldDivision != OldDivision ||
                (obj as CLPHistoryRemoveArrayLine).NewTopDivision != NewTopDivision ||
                (obj as CLPHistoryRemoveArrayLine).NewBottomDivision != NewBottomDivision)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}
