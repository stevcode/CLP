using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryStrokesChanged : ACLPHistoryItemBase
    {
        #region Constructors

        public CLPHistoryStrokesChanged(ICLPPage parentPage, List<string> strokeIdsAdded, List<Stroke> strokesRemoved)
            : base(parentPage)
        {
            StrokeIDsAdded = strokeIdsAdded;
            StrokeIDsRemoved = new List<string>();
            SerializedStrokesRemoved = new List<StrokeDTO>();
            foreach(var s in strokesRemoved)
            {
                StrokeIDsRemoved.Add(s.GetStrokeUniqueID());
                SerializedStrokesRemoved.Add(new StrokeDTO(s));
            }
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryStrokesChanged(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// Unique IDs of the strokes added
        /// </summary>
        public List<string> StrokeIDsAdded
        {
            get { return GetValue<List<string>>(StrokeIDsAddedProperty); }
            set { SetValue(StrokeIDsAddedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsAddedProperty = RegisterProperty("StrokeIDsAdded", typeof(List<string>));

        /// <summary>
        /// Added Ink Strokes serialized via Data Transfer Object, StrokeDTO. 
        /// Null unless removed from page via Undo.
        /// </summary>
        public List<StrokeDTO> SerializedStrokesAdded
        {
            get { return GetValue<List<StrokeDTO>>(SerializedStrokesAddedProperty); }
            set { SetValue(SerializedStrokesAddedProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesAddedProperty = RegisterProperty("SerializedStrokesAdded", typeof(List<StrokeDTO>));

        /// <summary>
        /// Unique IDs of the strokes removed
        /// </summary>
        public List<string> StrokeIDsRemoved
        {
            get { return GetValue<List<string>>(StrokeIDsRemovedProperty); }
            set { SetValue(StrokeIDsRemovedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsRemovedProperty = RegisterProperty("StrokeIDsRemoved", typeof(List<string>));

        /// <summary>
        /// Removed Ink Strokes serialized via Data Transfer Object, StrokeDTO. 
        /// Null after re-added page via Undo.
        /// </summary>
        public List<StrokeDTO> SerializedStrokesRemoved
        {
            get { return GetValue<List<StrokeDTO>>(SerializedStrokesRemovedProperty); }
            set { SetValue(SerializedStrokesRemovedProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesRemovedProperty = RegisterProperty("SerializedStrokesRemoved", typeof(List<StrokeDTO>));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            SerializedStrokesAdded = new List<StrokeDTO>();
            foreach(var stroke in StrokeIDsAdded.Select(id => ParentPage.GetStrokeByStrokeID(id))) 
            {
                SerializedStrokesAdded.Add(new StrokeDTO(stroke));
                try
                {
                    ParentPage.InkStrokes.Remove(stroke);
                }
                catch(Exception ex)
                {
                    Logger.Instance.WriteErrorToLog("StrokesChanged Undo Failure. ", ex);
                }
            }

            if(SerializedStrokesRemoved != null)
            {
                foreach(var serializedStroke in SerializedStrokesRemoved)
                {
                    ParentPage.InkStrokes.Add(serializedStroke.ToStroke());
                }
                SerializedStrokesRemoved = null;
            }
            else
            {
                Logger.Instance.WriteToLog("StrokesChanged Undo Failure: Null SerializedStrokesRemoved");
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            SerializedStrokesRemoved = new List<StrokeDTO>();
            foreach(string id in StrokeIDsRemoved)
            {
                var stroke = ParentPage.GetStrokeByStrokeID(id);
                SerializedStrokesRemoved.Add(new StrokeDTO(stroke));
                try
                {
                    ParentPage.InkStrokes.Remove(stroke);
                }
                catch(Exception ex)
                {
                    Logger.Instance.WriteErrorToLog("StrokesChanged Redo Failure. ", ex);
                }
            }

            if(SerializedStrokesAdded != null)
            {
                foreach(StrokeDTO serializedStroke in SerializedStrokesAdded)
                {
                    ParentPage.InkStrokes.Add(serializedStroke.ToStroke());
                }
                SerializedStrokesAdded = null;
            }
            else
            {
                Logger.Instance.WriteToLog("StrokesChanged Redo Failure: Null SerializedStrokesAdded");
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryStrokesChanged;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            if(clonedHistoryItem.SerializedStrokesAdded == null)
            {
                clonedHistoryItem.SerializedStrokesAdded = new List<StrokeDTO>();
                foreach(var stroke in StrokeIDsAdded.Select(id => ParentPage.GetStrokeByStrokeID(id)))
                {
                    clonedHistoryItem.SerializedStrokesAdded.Add(new StrokeDTO(stroke));
                }
            }

            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}