using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models.CLPHistoryItems
{
    [Serializable]
    public class CLPHistoryRemoveStroke : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryRemoveStroke(CLPPage page, List<byte> bytestroke) : base(HistoryItemType.RemoveStroke, page)
        {
            Bytestroke = bytestroke;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryRemoveStroke(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Bytestroke corresponding to the stroke added
        /// </summary>
        public List<byte> Bytestroke
        {
            get { return GetValue<List<byte>>(BytestrokeProperty); }
            set { SetValue(BytestrokeProperty, value); }
        }

        public static readonly PropertyData BytestrokeProperty = RegisterProperty("Bytestroke", typeof(List<byte>), null);

        #endregion //Properties

        #region Methods

        public override CLPHistoryItem GetUndoFingerprint()
        {
            return new CLPHistoryAddStroke(Page, Bytestroke);
        }

        public override CLPHistoryItem GetRedoFingerprint()
        {
            return new CLPHistoryRemoveStroke(Page, Bytestroke);
        }

        override public void Undo()
        {
            Stroke strokeRemoved = CLPPage.ByteToStroke(Bytestroke);
            bool shouldAdd = true;
            foreach(Stroke otherstroke in Page.InkStrokes)
            {
                if(otherstroke.GetStrokeUniqueID() == strokeRemoved.GetStrokeUniqueID())
                {
                    shouldAdd = false;
                    break;
                }
            }
            if(shouldAdd)
            {
                Page.InkStrokes.Add(strokeRemoved);
            }
        }

        override public void Redo()
        {
            Stroke s = CLPPage.ByteToStroke(Bytestroke);
            int indexToRemove = -1;
            foreach(Stroke otherstroke in Page.InkStrokes)
            {
                if(otherstroke.GetStrokeUniqueID() == s.GetStrokeUniqueID())
                {
                    indexToRemove = Page.InkStrokes.IndexOf(otherstroke);
                    break;
                }
            }
            if(indexToRemove != -1)
            {
                Page.InkStrokes.RemoveAt(indexToRemove);
            }
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryRemoveStroke))
            {
                return false;
            }
            Stroke thisStroke = CLPPage.ByteToStroke(Bytestroke);
            Stroke otherStroke = CLPPage.ByteToStroke((obj as CLPHistoryRemoveStroke).Bytestroke);
            if(thisStroke.GetStrokeUniqueID() != otherStroke.GetStrokeUniqueID())
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}