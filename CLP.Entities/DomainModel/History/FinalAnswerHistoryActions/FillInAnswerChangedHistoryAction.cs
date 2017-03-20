using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    public class FillInAnswerChangedHistoryAction : AHistoryActionBase, IStrokesOnPageChangedHistoryAction
    {
        #region Constructors

        /// <summary>Initializes <see cref="FillInAnswerChangedHistoryAction" /> from scratch.</summary>
        public FillInAnswerChangedHistoryAction() { }

        /// <summary>Initializes <see cref="FillInAnswerChangedHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        public FillInAnswerChangedHistoryAction(CLPPage parentPage, Person owner, InterpretationRegion interpretationRegion, IEnumerable<Stroke> strokesAdded, IEnumerable<Stroke> strokesRemoved)
            : base(parentPage, owner)
        {
            InterpretationRegionID = interpretationRegion.ID;

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

        #endregion //Constructors

        #region Properties

        /// <summary>ID of the affected Interpretation Region</summary>
        public string InterpretationRegionID
        {
            get { return GetValue<string>(InterpretationRegionIDProperty); }
            set { SetValue(InterpretationRegionIDProperty, value); }
        }

        public static readonly PropertyData InterpretationRegionIDProperty = RegisterProperty("InterpretationRegionID", typeof(string), string.Empty);

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

        /// <summary>List of serialized <see cref="Stroke" />s to be used on another machine when <see cref="MultipleChoiceBubbleStatusChangedHistoryAction" /> is unpacked.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public List<StrokeDTO> PackagedSerializedStrokes
        {
            get { return GetValue<List<StrokeDTO>>(PackagedSerializedStrokesProperty); }
            set { SetValue(PackagedSerializedStrokesProperty, value); }
        }

        public static readonly PropertyData PackagedSerializedStrokesProperty = RegisterProperty("PackagedSerializedStrokes", typeof(List<StrokeDTO>), () => new List<StrokeDTO>());

        #region Calculated Properties

        public InterpretationRegion PageObject => ParentPage.GetPageObjectByIDOnPageOrInHistory(InterpretationRegionID) as InterpretationRegion;

        public bool IsIntermediaryFillIn => PageObject?.IsIntermediary == true;

        public List<Stroke> StrokesAdded
        {
            get { return StrokeIDsAdded.Select(id => ParentPage.GetStrokeByIDOnPageOrInHistory(id)).Where(s => s != null).ToList(); }
        }

        public List<Stroke> StrokesRemoved
        {
            get { return StrokeIDsRemoved.Select(id => ParentPage.GetStrokeByIDOnPageOrInHistory(id)).Where(s => s != null).ToList(); }
        }

        #endregion // Calculated Properties

        #endregion // Properties

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 100;

        protected override string FormattedReport
        {
            get
            {
                var changeType = StrokeIDsRemoved.Any() ? "erasing" : "adding";
                var intermediarySignifier = IsIntermediaryFillIn ? "Intermediary" : "Fill-In";
                return $"Changed {intermediarySignifier} Answer by {changeType} a stroke.";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var addedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Debug.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsAdded in FillInAnswerChangedHistoryAction.", HistoryActionIndex);
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
                    Debug.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsRemoved in FillInAnswerChangedHistoryAction.", HistoryActionIndex);
                    continue;
                }
                removedStrokes.Add(stroke);
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
            }

            PageObject.ChangeAcceptedStrokes(removedStrokes, addedStrokes);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var removedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsRemoved.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Debug.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsRemoved in FillInAnswerChangedHistoryAction.", HistoryActionIndex);
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
                    Debug.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsAdded in FillInAnswerChangedHistoryAction.", HistoryActionIndex);
                    continue;
                }
                addedStrokes.Add(stroke);
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
            }

            PageObject.ChangeAcceptedStrokes(addedStrokes, removedStrokes);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            PackagedSerializedStrokes.Clear();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                PackagedSerializedStrokes.Add(stroke.ToStrokeDTO());
            }
            return this;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction()
        {
            foreach (var stroke in PackagedSerializedStrokes.Select(serializedStroke => serializedStroke.ToStroke()))
            {
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }
        }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return InterpretationRegionID == id;
        }

        public override bool IsUsingTrashedInkStroke(string id)
        {
            return StrokeIDsRemoved.Contains(id) || StrokeIDsAdded.Contains(id);
        }

        #endregion // AHistoryActionBase Overrides
    }
}