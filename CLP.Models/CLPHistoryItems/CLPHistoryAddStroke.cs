using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models.CLPHistoryItems
{
    [Serializable]
    public class CLPHistoryAddStroke : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryAddStroke(CLPPage page, List<byte> bytestroke) : base(HistoryItemType.AddStroke, page)
        {
            Bytestroke = bytestroke;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryAddStroke(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

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
            return new CLPHistoryRemoveStroke(Page, Bytestroke);
        }

        public override CLPHistoryItem GetRedoFingerprint()
        {
            return new CLPHistoryAddStroke(Page, Bytestroke);
        }

        override public void Undo()
        {
            Stroke strokeAdded = CLPPage.ByteToStroke(Bytestroke);
            int indexToRemove = -1;
            foreach(Stroke otherstroke in Page.InkStrokes)
            {
                if(otherstroke.GetStrokeUniqueID() == strokeAdded.GetStrokeUniqueID())
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

        override public void Redo()
        {
            Stroke s = CLPPage.ByteToStroke(Bytestroke);
            bool shouldAdd = true;
            foreach(Stroke otherstroke in Page.InkStrokes)
            {
                if(otherstroke.GetStrokeUniqueID() == s.GetStrokeUniqueID())
                {
                    shouldAdd = false;
                    break;
                }
            }
            if(shouldAdd)
            {
                Page.InkStrokes.Add(s);
            }
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryAddStroke))
            {
                return false;
            }
            Stroke thisStroke = CLPPage.ByteToStroke(Bytestroke);
            Stroke otherStroke = CLPPage.ByteToStroke((obj as CLPHistoryAddStroke).Bytestroke);
            if (thisStroke.GetStrokeUniqueID() != otherStroke.GetStrokeUniqueID()) 
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}