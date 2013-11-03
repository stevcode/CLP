﻿using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryStrokeAdd : ACLPHistoryItemBase
    {

        #region Constructors

        public CLPHistoryStrokeAdd(ICLPPage parentPage, string strokeId) 
            : base(parentPage)
        {
            StrokeID = strokeId;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryStrokeAdd(SerializationInfo info, StreamingContext context)
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
        /// Unique ID of the stroke added
        /// </summary>
        public string StrokeID
        {
            get { return GetValue<string>(StrokeIDProperty); }
            set { SetValue(StrokeIDProperty, value); }
        }

        public static readonly PropertyData StrokeIDProperty = RegisterProperty("StrokeID", typeof(string));

        /// <summary>
        /// Ink Stroke serialized via Data Transfer Object, StrokeDTO. Null unless removed from page via Undo.
        /// </summary>
        public StrokeDTO SerializedStroke
        {
            get { return GetValue<StrokeDTO>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof(StrokeDTO));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var stroke = ParentPage.GetStrokeByStrokeID(StrokeID);
            SerializedStroke = new StrokeDTO(stroke);
            try
            {
                ParentPage.InkStrokes.Remove(stroke);
            }
            catch(Exception ex)
            {
                Logger.Instance.WriteErrorToLog("Undo AddStroke Error.", ex);
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(SerializedStroke != null)
            {
                var stroke = SerializedStroke.ToStroke();
                if(stroke == null)
                {
                    Logger.Instance.WriteToLog("Skipped Loading of corrupted stroke");
                    return;
                }
                ParentPage.InkStrokes.Add(stroke);
                SerializedStroke = null; //on Page again, don't want to reserialize.
            }
            else
            {
                Logger.Instance.WriteToLog("AddStroke Redo Failure: No stroke to add.");
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryStrokeAdd;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            var stroke = ParentPage.GetStrokeByStrokeID(StrokeID);
            if(stroke == null)
            {
                Logger.Instance.WriteToLog("Failed to get stroke by ID during UndoRedoComplete in HistoryStrokeAdd.");
                return null;
            }
            clonedHistoryItem.SerializedStroke = new StrokeDTO(stroke);

            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}