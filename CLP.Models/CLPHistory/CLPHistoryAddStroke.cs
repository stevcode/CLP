using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryAddStroke : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryAddStroke(String strokeId) : base(HistoryItemType.AddStroke)
        {
            StrokeId = strokeId;
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
        /// Unique ID of the stroke added
        /// </summary>
        public String StrokeId
        {
            get { return GetValue<String>(StrokeIdProperty); }
            set { SetValue(StrokeIdProperty, value); }
        }

        public static readonly PropertyData StrokeIdProperty = RegisterProperty("StrokeId", typeof(String), null);

        /// <summary>
        /// Bytestroke corresponding to the stroke added (null unless this event is in the Future)
        /// </summary>
        public List<byte> Bytestroke
        {
            get { return GetValue<List<byte>>(BytestrokeProperty); }
            set { SetValue(BytestrokeProperty, value); }
        }

        public static readonly PropertyData BytestrokeProperty = RegisterProperty("Bytestroke", typeof(List<byte>), null);

        /// <summary>
        /// Ink Stroke serialized via Data Transfer Object, StrokeDTO.
        /// </summary>
        public StrokeDTO SerializedStroke
        {
            get { return GetValue<StrokeDTO>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof(StrokeDTO));


        #endregion //Properties

        #region Methods

        public override CLPHistoryItem GetUndoFingerprint(CLPPage page)
        {
            return new CLPHistoryRemoveStroke(GetSerializedStrokeByUniqueID(page, StrokeId));
        }

        public override CLPHistoryItem GetRedoFingerprint(CLPPage page)
        {
            return new CLPHistoryAddStroke(StrokeId);
        }

        override public void Undo(CLPPage page)
        {
            SerializedStroke = GetSerializedStrokeByUniqueID(page, StrokeId); // remember in case we put it back
            if(SerializedStroke == null)
            {
                Console.WriteLine("AddStroke undo failure: No stroke to remove");
                return;
            }
            int indexToRemove = -1;
            foreach(Stroke otherstroke in page.InkStrokes)
            {
                if(otherstroke.GetStrokeUniqueID() == StrokeId)
                {
                    indexToRemove = page.InkStrokes.IndexOf(otherstroke);
                    SerializedStroke = new StrokeDTO(otherstroke);
                    break;
                }
            }
            if(indexToRemove != -1)
            {
                page.InkStrokes.RemoveAt(indexToRemove);
            }
        }

        override public void Redo(CLPPage page)
        {
            if(SerializedStroke == null)
            {
                Console.WriteLine("AddStroke redo failure: No stroke to add");
                return;
            }
            Stroke s = SerializedStroke.ToStroke();
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
                page.InkStrokes.Add(s);
            }
            SerializedStroke = null; //don't need to remember anymore
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryAddStroke))
            {
                return false;
            }

            if (StrokeId != (obj as CLPHistoryAddStroke).StrokeId) 
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}