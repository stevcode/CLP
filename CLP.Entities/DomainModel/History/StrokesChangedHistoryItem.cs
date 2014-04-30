using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class StrokesChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="StrokesChangedHistoryItem" /> from scratch.
        /// </summary>
        public StrokesChangedHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="StrokesChangedHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        public StrokesChangedHistoryItem(CLPPage parentPage, List<string> strokesIDsAdded, IEnumerable<Stroke> strokesRemoved)
            : base(parentPage)
        {
            StrokeIDsAdded = strokesIDsAdded;
            foreach(var stroke in strokesRemoved)
            {
                StrokeIDsRemoved.Add(stroke.GetStrokeID());
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected StrokesChangedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 100; }
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
        /// Unique IDs of the strokes removed
        /// </summary>
        public List<string> StrokeIDsRemoved
        {
            get { return GetValue<List<string>>(StrokeIDsRemovedProperty); }
            set { SetValue(StrokeIDsRemovedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsRemovedProperty = RegisterProperty("StrokeIDsRemoved", typeof(List<string>), () => new List<string>());

        /// <summary>
        /// List of serialized <see cref="Stroke" />s to be used on another machine when <see cref="StrokesChangedHistoryItem" /> is unpacked.
        /// </summary>
        [XmlIgnore]
        public List<StrokeDTO> PackagedSerializedStrokes
        {
            get { return GetValue<List<StrokeDTO>>(PackagedSerializedStrokesProperty); }
            set { SetValue(PackagedSerializedStrokesProperty, value); }
        }

        public static readonly PropertyData PackagedSerializedStrokesProperty = RegisterProperty("PackagedSerializedStrokes", typeof(List<StrokeDTO>), () => new List<StrokeDTO>());

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            foreach(var stroke in StrokeIDsAdded.Select(id => ParentPage.GetStrokeByID(id))) 
            {
                try
                {
                    ParentPage.InkStrokes.Remove(stroke);
                    ParentPage.History.TrashedInkStrokes.Add(stroke);
                }
                catch(Exception ex)
                {
                }
            }

            foreach(var stroke in StrokeIDsRemoved.Select(id => ParentPage.History.GetStrokeByID(id))) 
            {
                try
                {
                    ParentPage.History.TrashedInkStrokes.Remove(stroke);
                    ParentPage.InkStrokes.Add(stroke);
                }
                catch(Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            foreach(var stroke in StrokeIDsRemoved.Select(id => ParentPage.GetStrokeByID(id))) 
            {
                try
                {
                    ParentPage.InkStrokes.Remove(stroke);
                    ParentPage.History.TrashedInkStrokes.Add(stroke);
                }
                catch(Exception ex)
                {
                }
            }

            foreach(var stroke in StrokeIDsAdded.Select(id => ParentPage.History.GetStrokeByID(id))) 
            {
                try
                {
                    ParentPage.History.TrashedInkStrokes.Remove(stroke);
                    ParentPage.InkStrokes.Add(stroke);
                }
                catch(Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that is can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as StrokesChangedHistoryItem;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            clonedHistoryItem.PackagedSerializedStrokes.Clear();
            foreach(var stroke in StrokeIDsAdded.Select(id => ParentPage.GetStrokeByID(id)))
            {
                clonedHistoryItem.PackagedSerializedStrokes.Add(stroke.ToStrokeDTO());
            }

            return clonedHistoryItem;
        }

        #region Overrides of AHistoryItemBase

        public override void UnpackHistoryItem()
        {
            foreach(var stroke in PackagedSerializedStrokes.Select(serializedStroke => serializedStroke.ToStroke())) 
            {
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }
        }

        #endregion

        #endregion //Methods
    }
}