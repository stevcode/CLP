﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum ChoiceBubbleStatuses
    {
        PartiallyFilledIn,
        FilledIn,
        AdditionalFilledIn,
        ErasedPartiallyFilledIn,
        IncompletelyErased,
        CompletelyErased
    }

    [Serializable]
    public class MultipleChoiceBubbleStatusChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryItem" /> from scratch.</summary>
        public MultipleChoiceBubbleStatusChangedHistoryItem()
        { }

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public MultipleChoiceBubbleStatusChangedHistoryItem(CLPPage parentPage,
                                               Person owner,
                                               MultipleChoice multipleChoice,
                                               int choiceBubbleIndex,
                                               ChoiceBubbleStatuses status,
                                               IEnumerable<Stroke> strokesAdded,
                                               IEnumerable<Stroke> strokesRemoved)
            : base(parentPage, owner)
        {
            MultipleChoiceID = multipleChoice.ID;
            ChoiceBubbleIndex = choiceBubbleIndex;
            ChoiceBubbleStatus = status;

            StrokeIDsAdded = strokesAdded.Select(s => s.GetStrokeID()).ToList();
            foreach (var stroke in strokesRemoved)
            {
                StrokeIDsRemoved.Add(stroke.GetStrokeID());
                if (!ParentPage.History.TrashedInkStrokes.Contains(stroke))
                {
                    ParentPage.History.TrashedInkStrokes.Add(stroke);
                }
            }
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected MultipleChoiceBubbleStatusChangedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 200; }
        }

        /// <summary>ID of the affected Multiple Choice.</summary>
        public string MultipleChoiceID
        {
            get { return GetValue<string>(MultipleChoiceIDProperty); }
            set { SetValue(MultipleChoiceIDProperty, value); }
        }

        public static readonly PropertyData MultipleChoiceIDProperty = RegisterProperty("MultipleChoiceID", typeof (string), string.Empty);

        /// <summary>Index of the affected choice bubble.</summary>
        public int ChoiceBubbleIndex
        {
            get { return GetValue<int>(ChoiceBubbleIndexProperty); }
            set { SetValue(ChoiceBubbleIndexProperty, value); }
        }

        public static readonly PropertyData ChoiceBubbleIndexProperty = RegisterProperty("ChoiceBubbleIndex", typeof (int), -1);
        

        /// <summary>Status of the choice bubble after ink change occurs.</summary>
        public ChoiceBubbleStatuses ChoiceBubbleStatus
        {
            get { return GetValue<ChoiceBubbleStatuses>(ChoiceBubbleStatusProperty); }
            set { SetValue(ChoiceBubbleStatusProperty, value); }
        }

        public static readonly PropertyData ChoiceBubbleStatusProperty = RegisterProperty("ChoiceBubbleStatus", typeof (ChoiceBubbleStatuses), ChoiceBubbleStatuses.PartiallyFilledIn);
        
        
        #region Strokes

        /// <summary>Unique IDs of the strokes added.</summary>
        public List<string> StrokeIDsAdded
        {
            get { return GetValue<List<string>>(StrokeIDsAddedProperty); }
            set { SetValue(StrokeIDsAddedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsAddedProperty = RegisterProperty("StrokeIDsAdded", typeof(List<string>), () => new List<string>());

        /// <summary>Unique IDs of the strokes removed.</summary>
        public List<string> StrokeIDsRemoved
        {
            get { return GetValue<List<string>>(StrokeIDsRemovedProperty); }
            set { SetValue(StrokeIDsRemovedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsRemovedProperty = RegisterProperty("StrokeIDsRemoved", typeof(List<string>), () => new List<string>());

        /// <summary>List of serialized <see cref="Stroke" />s to be used on another machine when <see cref="StrokesChangedHistoryItem" /> is unpacked.</summary>
        [XmlIgnore]
        public List<StrokeDTO> PackagedSerializedStrokes
        {
            get { return GetValue<List<StrokeDTO>>(PackagedSerializedStrokesProperty); }
            set { SetValue(PackagedSerializedStrokesProperty, value); }
        }

        public static readonly PropertyData PackagedSerializedStrokesProperty = RegisterProperty("PackagedSerializedStrokes", typeof(List<StrokeDTO>), () => new List<StrokeDTO>());

        #endregion //Strokes

        public override string FormattedValue
        {
            get
            {
                var bubble = Bubble;
                var formattedBubble = string.Format("{0} \"{1} {2}\", {3}", bubble.BubbleContent, bubble.Answer, bubble.AnswerLabel, bubble.IsACorrectValue ? "Correct" : "Incorrect");
                return string.Format("Index #{0}, {1} Bubble {2}", HistoryIndex, ChoiceBubbleStatus, formattedBubble);
            }
        }

        public MultipleChoice PageObject
        {
            get { return ParentPage.GetPageObjectByIDOnPageOrInHistory(MultipleChoiceID) as MultipleChoice; }
        }

        public ChoiceBubble Bubble
        {
            get { return PageObject.ChoiceBubbles[ChoiceBubbleIndex]; }
        }

        public List<Stroke> StrokesAdded
        {
            get
            {
                return StrokeIDsAdded.Select(id => ParentPage.GetStrokeByIDOnPageOrInHistory(id)).Where(s => s != null).ToList();
            }
        }

        public List<Stroke> StrokesRemoved
        {
            get
            {
                return StrokeIDsRemoved.Select(id => ParentPage.GetStrokeByIDOnPageOrInHistory(id)).Where(s => s != null).ToList();
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        { UndoAction(false); }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {

            var addedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsAdded in MultipleChoiceBubbleStatusChangedHistoryItem.", HistoryIndex);
                    continue;
                }
                addedStrokes.Add(stroke);
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }

            var removedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsRemoved.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsRemoved in MultipleChoiceBubbleStatusChangedHistoryItem.", HistoryIndex);
                    continue;
                }
                removedStrokes.Add(stroke);
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
            }

            PageObject.ChangeAcceptedStrokes(removedStrokes, addedStrokes);

            switch (ChoiceBubbleStatus)
            {
                case ChoiceBubbleStatuses.CompletelyErased:
                    Bubble.IsFilledIn = true;
                    break;
                case ChoiceBubbleStatuses.FilledIn:
                    Bubble.IsFilledIn = false;
                    break;
            }
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var removedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsRemoved.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsRemoved in MultipleChoiceBubbleStatusChangedHistoryItem.", HistoryIndex);
                    continue;
                }
                removedStrokes.Add(stroke);
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }

            var addedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsAdded in MultipleChoiceBubbleStatusChangedHistoryItem.", HistoryIndex);
                    continue;
                }
                addedStrokes.Add(stroke);
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
            }

            PageObject.ChangeAcceptedStrokes(addedStrokes, removedStrokes);

            switch (ChoiceBubbleStatus)
            {
                case ChoiceBubbleStatuses.CompletelyErased:
                    Bubble.IsFilledIn = false;
                    break;
                case ChoiceBubbleStatuses.FilledIn:
                    Bubble.IsFilledIn = true;
                    break;
            }
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            PackagedSerializedStrokes.Clear();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                PackagedSerializedStrokes.Add(stroke.ToStrokeDTO());
            }
            return this;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem()
        {
            foreach (var stroke in PackagedSerializedStrokes.Select(serializedStroke => serializedStroke.ToStroke()))
            {
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }
        }

        public override bool IsUsingTrashedPageObject(string id)
        { return MultipleChoiceID == id; }

        public override bool IsUsingTrashedInkStroke(string id)
        { return StrokeIDsRemoved.Contains(id) || StrokeIDsAdded.Contains(id); }

        #endregion //Methods
    }
}