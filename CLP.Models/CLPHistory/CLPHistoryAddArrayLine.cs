using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryArrayAddLine : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryAddArrayLine(String arrayId, CLPArrayDivision oldDiv, 
            CLPArrayDivision newTopDiv, CLPArrayDivision newBottomDiv)
            : base(HistoryItemType.AddArrayLine)
        {
            ArrayId = arrayId;
            OldDivision = oldDiv;
            NewTopDivision = newTopDiv;
            NewBottomDivision = newBottomDiv;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryAddArrayLine(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Array (by unique ID) to which a line was added
        /// </summary>
        public String ArrayId
        {
            get { return GetValue<String>(ArrayIdProperty); }
            set { SetValue(ArrayIdProperty, value); }
        }

        public static readonly PropertyData ArrayIdProperty =
            RegisterProperty("ArrayId", typeof(String), null);

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

        public override void Undo(CLPPage page)
        {
            CLPArray array = (GetPageObjectByUniqueID(page, ArrayId) as CLPArray);
            if(NewTopDivision.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                array.HorizontalDivisions.Remove(NewTopDivision);
                array.HorizontalDivisions.Remove(NewBottomDivision);
                if(OldDivision != null)
                {
                    array.HorizontalDivisions.Add(OldDivision);
                }
            }
            else
            {
                array.VerticalDivisions.Remove(NewTopDivision);
                array.VerticalDivisions.Remove(NewBottomDivision);
                if(OldDivision != null)
                {
                    array.VerticalDivisions.Add(OldDivision);
                }
            }
        }

        override public void Redo(CLPPage page)
        {
            CLPArray array = (GetPageObjectByUniqueID(page, ArrayId) as CLPArray);
            if(NewTopDivision.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                array.HorizontalDivisions.Add(NewTopDivision);
                array.HorizontalDivisions.Add(NewBottomDivision);
                if(OldDivision != null)
                {
                    array.HorizontalDivisions.Remove(OldDivision);
                }
            }
            else
            {
                array.VerticalDivisions.Add(NewTopDivision);
                array.VerticalDivisions.Add(NewBottomDivision);
                if(OldDivision != null)
                {
                    array.VerticalDivisions.Remove(OldDivision);
                }
            }
        }

        override public CLPHistoryItem GetUndoFingerprint(CLPPage page)
        {
            return null;
        }

        override public CLPHistoryItem GetRedoFingerprint(CLPPage page)
        {
            return null;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryAddArrayLine) ||
                (obj as CLPHistoryAddArrayLine).ArrayId != ArrayId ||
                (obj as CLPHistoryAddArrayLine).OldDivision != OldDivision ||
                (obj as CLPHistoryAddArrayLine).NewTopDivision != NewTopDivision ||
                (obj as CLPHistoryAddArrayLine).NewBottomDivision != NewBottomDivision)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}
