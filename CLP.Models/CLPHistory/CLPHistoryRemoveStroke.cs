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

        public CLPHistoryRemoveStroke(StrokeDTO serializedStroke) : base(HistoryItemType.RemoveStroke)
        {
            SerializedStroke = serializedStroke;
            StrokeId = serializedStroke.StrokeID;
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
        /// Ink Stroke serialized via Data Transfer Object, StrokeDTO.
        /// Corresponds to the stroke removed (null if this event is in the Future)
        /// </summary>
        public StrokeDTO SerializedStroke
        {
            get { return GetValue<StrokeDTO>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof(StrokeDTO));


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
            return new CLPHistoryRemoveStroke(GetSerializedStrokeByUniqueID(page, StrokeId));
        }

        override public void Undo(CLPPage page)
        {
            if(SerializedStroke == null)
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
                page.InkStrokes.Add(SerializedStroke.ToStroke());
            }
            SerializedStroke = null;
        }

        override public void Redo(CLPPage page)
        {
            SerializedStroke = GetSerializedStrokeByUniqueID(page, StrokeId);
            if(SerializedStroke == null)
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
            if(!(obj is CLPHistoryRemoveStroke) || SerializedStroke == null || (obj as CLPHistoryRemoveStroke).SerializedStroke == null)
            {
                return false;
            }
            Stroke thisStroke = SerializedStroke.ToStroke();
            Stroke otherStroke = (obj as CLPHistoryRemoveStroke).SerializedStroke.ToStroke();
            if(thisStroke.GetStrokeUniqueID() != otherStroke.GetStrokeUniqueID())
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}