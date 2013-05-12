using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryRemoveStroke : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryRemoveStroke(List<byte> bytestroke) : base(HistoryItemType.RemoveStroke)
        {
            Bytestroke = bytestroke;
            StrokeId = CLPPage.ByteToStroke(bytestroke).GetStrokeUniqueID();
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
        /// Bytestroke corresponding to the stroke removed (null if this event is in the Future)
        /// </summary>
        public List<byte> Bytestroke
        {
            get { return GetValue<List<byte>>(BytestrokeProperty); }
            set { SetValue(BytestrokeProperty, value); }
        }

        public static readonly PropertyData BytestrokeProperty = RegisterProperty("Bytestroke", typeof(List<byte>), null);

        /// <summary>
        /// Unique ID of the stroke removed
        /// </summary>
        public String StrokeId
        {
            get { return GetValue<String>(StrokeIdProperty); }
            set { SetValue(StrokeIdProperty, value); }
        }

        public static readonly PropertyData StrokeIdProperty = RegisterProperty("StrokeId", typeof(String), null);


        #endregion //Properties

        #region Methods

        public override CLPHistoryItem GetUndoFingerprint(CLPPage page)
        {
            return new CLPHistoryAddStroke(StrokeId);
        }

        public override CLPHistoryItem GetRedoFingerprint(CLPPage page)
        {
            return new CLPHistoryRemoveStroke(GetBytestrokeByUniqueID(page, StrokeId));
        }

        override public void Undo(CLPPage page)
        {
            if(Bytestroke == null)
            {
                Console.WriteLine("RemoveStroke undo failure: No stroke to add.");
                return;
            }
            bool shouldAdd = true;
            foreach(Stroke otherstroke in page.InkStrokes)
            {
                if(otherstroke.GetStrokeUniqueID() == StrokeId)
                {
                    shouldAdd = false;
                    break;
                }
            }
            if(shouldAdd)
            {
                page.InkStrokes.Add(CLPPage.ByteToStroke(Bytestroke));
            }
            Bytestroke = null;
        }

        override public void Redo(CLPPage page)
        {
            Bytestroke = GetBytestrokeByUniqueID(page, StrokeId);
            if(Bytestroke == null)
            {
                Console.WriteLine("RemoveStroke redo failure: No stroke to remove.");
                return;
            }
            int indexToRemove = -1;
            foreach(Stroke otherstroke in page.InkStrokes)
            {
                if(otherstroke.GetStrokeUniqueID() == StrokeId)
                {
                    indexToRemove = page.InkStrokes.IndexOf(otherstroke);
                    break;
                }
            }
            if(indexToRemove != -1)
            {
                page.InkStrokes.RemoveAt(indexToRemove);
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