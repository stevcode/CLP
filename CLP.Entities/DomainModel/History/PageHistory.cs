using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Ink;
using System.Windows.Threading;
using System.Xml.Serialization;
using Catel.Collections;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class PageHistory : AEntityBase
    {
        private readonly object _historyLock = new object();
        public const double SAMPLE_RATE = 9;
        public IHistoryBatch CurrentHistoryBatch;
        private bool _isUndoingOperation;

        #region Constructors

        /// <summary>Initializes <see cref="PageHistory" /> from scratch.</summary>
        public PageHistory()
        {
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="PageHistory" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public PageHistory(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region ID

        /// <summary>Unique Identifier for the <see cref="PageHistory" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>Version Index of the <see cref="PageHistory" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public uint VersionIndex
        {
            get { return GetValue<uint>(VersionIndexProperty); }
            set { SetValue(VersionIndexProperty, value); }
        }

        public static readonly PropertyData VersionIndexProperty = RegisterProperty("VersionIndex", typeof(uint), 0);

        /// <summary>Version Index of the latest submission.</summary>
        public uint? LastVersionIndex
        {
            get { return GetValue<uint?>(LastVersionIndexProperty); }
            set { SetValue(LastVersionIndexProperty, value); }
        }

        public static readonly PropertyData LastVersionIndexProperty = RegisterProperty("LastVersionIndex", typeof(uint?));

        #endregion //ID

        public bool UseHistory
        {
            get { return GetValue<bool>(UseHistoryProperty); }
            set { SetValue(UseHistoryProperty, value); }
        }

        public static readonly PropertyData UseHistoryProperty = RegisterProperty("UseHistory", typeof(bool), true);

        #region HistoryActions

        /// <summary>All events available for Undo.</summary>
        public ObservableCollection<IHistoryAction> UndoItems
        {
            get { return GetValue<ObservableCollection<IHistoryAction>>(UndoItemsProperty); }
            set { SetValue(UndoItemsProperty, value); }
        }

        public static readonly PropertyData UndoItemsProperty = RegisterProperty("UndoItems", typeof(ObservableCollection<IHistoryAction>), () => new ObservableCollection<IHistoryAction>());

        /// <summary>All events available for Redo.</summary>
        public ObservableCollection<IHistoryAction> RedoItems
        {
            get { return GetValue<ObservableCollection<IHistoryAction>>(RedoItemsProperty); }
            set { SetValue(RedoItemsProperty, value); }
        }

        public static readonly PropertyData RedoItemsProperty = RegisterProperty("RedoItems", typeof(ObservableCollection<IHistoryAction>), () => new ObservableCollection<IHistoryAction>());

        public List<IHistoryAction> CompleteOrderedHistoryItems
        {
            get { return UndoItems.Reverse().Concat(RedoItems).ToList(); }
        }

        public List<IHistoryAction> OrderedAnimationHistoryItems
        {
            get
            {
                var combinedHistory = CompleteOrderedHistoryItems;

                var startAnimationIndicator =
                    combinedHistory.FirstOrDefault(
                                                   clpHistoryItem =>
                                                       clpHistoryItem is AnimationIndicatorHistoryAction &&
                                                       (clpHistoryItem as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Record);
                var stopAnimationIndicator =
                    combinedHistory.FirstOrDefault(
                                                   clpHistoryItem =>
                                                       clpHistoryItem is AnimationIndicatorHistoryAction &&
                                                       (clpHistoryItem as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Stop);
                var startIndex = combinedHistory.IndexOf(startAnimationIndicator);
                var stopIndex = combinedHistory.IndexOf(stopAnimationIndicator);
                if (stopIndex > -1) //HACK: This garauntees correct historyItems in the scenario when a student hits Record, then Clear Page, does work and then hits Stop.
                {
                    startIndex = 0;
                }

                var animationHistoryItems = new List<IHistoryAction>();
                if (stopIndex == 0) //HACK: Above Hack, but where first UndoItem is a Stop.
                {
                    return animationHistoryItems;
                }

                if (startIndex > -1)
                {
                    animationHistoryItems = combinedHistory.Skip(startIndex).ToList();
                }
                else
                {
                    return animationHistoryItems;
                }

                if (stopIndex > 0)
                {
                    animationHistoryItems = animationHistoryItems.Take(stopIndex - startIndex + 1).ToList();
                }
                else //HACK: This garauntees correct historyItems in the scenario when a student hits Record, but Submits page before hitting Stop.
                {
                    animationHistoryItems = animationHistoryItems.Take(combinedHistory.Count - startIndex).ToList();
                }

                return animationHistoryItems;
            }
        }

        public bool CanUndo
        {
            get
            {
                lock (_historyLock)
                {
                    return !_isUndoingOperation && UndoItems.Any() && UseHistory;
                }
            }
        }

        public bool CanRedo
        {
            get
            {
                lock (_historyLock)
                {
                    return !_isUndoingOperation && RedoItems.Any() && UseHistory;
                }
            }
        }

        #endregion //HistoryActions

        #region SemanticEvents

        /// <summary>List of all the ISemanticEvents analysis generates.</summary>
        public ObservableCollection<ISemanticEvent> SemanticEvents
        {
            get { return GetValue<ObservableCollection<ISemanticEvent>>(SemanticEventsProperty); }
            set { SetValue(SemanticEventsProperty, value); }
        }

        public static readonly PropertyData SemanticEventsProperty = RegisterProperty("SemanticEvents", typeof(ObservableCollection<ISemanticEvent>), () => new ObservableCollection<ISemanticEvent>());

        #endregion //SemanticEvents

        #region Playback Indication

        /// <summary>Flag to signify history is currently playing or rewinding.</summary>
        public bool IsAnimating
        {
            get { return GetValue<bool>(IsAnimatingProperty); }
            set { SetValue(IsAnimatingProperty, value); }
        }

        public static readonly PropertyData IsAnimatingProperty = RegisterProperty("IsAnimating", typeof(bool), false);

        /// <summary>Current position within the History.</summary>
        public double CurrentHistoryTick
        {
            get { return GetValue<double>(CurrentHistoryTickProperty); }
            set { SetValue(CurrentHistoryTickProperty, value); }
        }

        public static readonly PropertyData CurrentHistoryTickProperty = RegisterProperty("CurrentHistoryTick", typeof(double), 0);

        public int CurrentHistoryIndex
        {
            get { return UndoItems.Any() ? UndoItems.First().HistoryActionIndex : 0; }
        }

        public int CurrentAnimationDelay
        {
            get { return RedoItems.Any() ? RedoItems.First().AnimationDelay : 0; }
        }

        public bool IsAnimation
        {
            get
            {
                return
                    UndoItems.Any(
                                  clpHistoryItem =>
                                          clpHistoryItem is AnimationIndicatorHistoryAction && (clpHistoryItem as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Stop) ||
                    RedoItems.Any(
                                  clpHistoryItem =>
                                          clpHistoryItem is AnimationIndicatorHistoryAction && (clpHistoryItem as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Stop);
            }
        }

        /// <summary>Forces playback on non-animation pages.</summary>
        [XmlIgnore] [ExcludeFromSerialization] private bool _isNonAnimationPlaybackEnabled = false;

        [XmlIgnore]
        [ExcludeFromSerialization]
        public bool IsNonAnimationPlaybackEnabled
        {
            get { return _isNonAnimationPlaybackEnabled; }
            set
            {
                if (_isNonAnimationPlaybackEnabled == value)
                {
                    return;
                }
                _isNonAnimationPlaybackEnabled = value;
                UpdateTicks();
            }
        }

        public bool IsPlaybackEnabled
        {
            get { return IsAnimation || IsNonAnimationPlaybackEnabled; }
        }

        #endregion //Playback Indication

        #region Playback Lengths

        public int HistoryLength
        {
            get
            {
                var totalTicks = 0;
                foreach (var clpHistoryItem in CompleteOrderedHistoryItems)
                {
                    if (clpHistoryItem is IHistoryBatch)
                    {
                        totalTicks += (clpHistoryItem as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        totalTicks++;
                    }
                }

                return totalTicks;
            }
        }

        public int AnimationLength
        {
            get
            {
                var totalAnimationTicks = 0;

                foreach (var clpHistoryItem in OrderedAnimationHistoryItems)
                {
                    if (clpHistoryItem is IHistoryBatch)
                    {
                        totalAnimationTicks += (clpHistoryItem as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        totalAnimationTicks++;
                    }
                }

                return totalAnimationTicks;
            }
        }

        public double PlaybackLength
        {
            get { return IsNonAnimationPlaybackEnabled ? HistoryLength : AnimationLength; }
        }

        #endregion //Playback Lengths

        #region Trashed Items

        /// <summary>A list of all the <see cref="IPageObject" />s that have been removed from a <see cref="CLPPage" />, but are needed for the <see cref="PageHistory" />.</summary>
        public List<IPageObject> TrashedPageObjects
        {
            get { return GetValue<List<IPageObject>>(TrashedPageObjectsProperty); }
            set { SetValue(TrashedPageObjectsProperty, value); }
        }

        public static readonly PropertyData TrashedPageObjectsProperty = RegisterProperty("TrashedPageObjects", typeof(List<IPageObject>), () => new List<IPageObject>());

        /// <summary>A list of all the <see cref="Stroke" />s that have been removed from a <see cref="CLPPage" />, but are needed for the <see cref="PageHistory" />.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public StrokeCollection TrashedInkStrokes
        {
            get { return GetValue<StrokeCollection>(TrashedInkStrokesProperty); }
            set { SetValue(TrashedInkStrokesProperty, value); }
        }

        public static readonly PropertyData TrashedInkStrokesProperty = RegisterProperty("TrashedInkStrokes", typeof(StrokeCollection), () => new StrokeCollection());

        /// <summary>A list of all the serialized <see cref="Stroke" />s that have been removed from a <see cref="CLPPage" />, but are needed for the <see cref="PageHistory" />.</summary>
        public List<StrokeDTO> SerializedTrashedInkStrokes
        {
            get { return GetValue<List<StrokeDTO>>(SerializedTrashedInkStrokesProperty); }
            set { SetValue(SerializedTrashedInkStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedTrashedInkStrokesProperty = RegisterProperty("SerializedTrashedInkStrokes", typeof(List<StrokeDTO>), () => new List<StrokeDTO>());

        #endregion //Trashed Items

        #endregion //Properties

        #region Methods

        public void RefreshHistoryIndexes()
        {
            foreach (var historyItem in UndoItems)
            {
                historyItem.HistoryActionIndex = UndoItems.Count - UndoItems.IndexOf(historyItem) - 1;
            }

            foreach (var historyItem in RedoItems)
            {
                historyItem.HistoryActionIndex = UndoItems.Count + RedoItems.IndexOf(historyItem);
            }
        }

        public void OptimizeTrashedItems()
        {
            var inkStrokesToRemove = (from trashedInkStroke in TrashedInkStrokes
                                      let isTrashedInkStrokeBeingUsed =
                                      UndoItems.Any(historyItem => historyItem.IsUsingTrashedInkStroke(trashedInkStroke.GetStrokeID())) ||
                                      RedoItems.Any(historyItem => historyItem.IsUsingTrashedInkStroke(trashedInkStroke.GetStrokeID()))
                                      where !isTrashedInkStrokeBeingUsed
                                      select trashedInkStroke).ToList();
            foreach (var stroke in inkStrokesToRemove)
            {
                TrashedInkStrokes.Remove(stroke);
            }

            var pageObjectsToRemove = (from trashedPageObject in TrashedPageObjects
                                       let isTrashedPageObjectBeingUsed =
                                       UndoItems.Any(historyItem => historyItem.IsUsingTrashedPageObject(trashedPageObject.ID)) ||
                                       RedoItems.Any(historyItem => historyItem.IsUsingTrashedPageObject(trashedPageObject.ID))
                                       where !isTrashedPageObjectBeingUsed
                                       select trashedPageObject).ToList();
            foreach (var pageObject in pageObjectsToRemove)
            {
                TrashedPageObjects.Remove(pageObject);
            }
        }

        //Completely clear history.
        public void ClearHistory()
        {
            lock (_historyLock)
            {
                UndoItems.Clear();
                RedoItems.Clear();
                TrashedInkStrokes.Clear();
                SerializedTrashedInkStrokes.Clear();
                TrashedPageObjects.Clear();
                _isUndoingOperation = false;
            }
            UpdateTicks();
        }

        public void ClearNonAnimationHistory()
        {
            lock (_historyLock)
            {
                var startAnimationIndicator =
                    UndoItems.FirstOrDefault(
                                             clpHistoryItem =>
                                                 clpHistoryItem is AnimationIndicatorHistoryAction &&
                                                 (clpHistoryItem as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Record);
                var startIndex = UndoItems.IndexOf(startAnimationIndicator);
                if (startIndex > -1)
                {
                    var animationUndoItems = UndoItems.Take(startIndex + 1);
                    UndoItems = new ObservableCollection<IHistoryAction>(animationUndoItems);
                }
                else
                {
                    UndoItems.Clear();
                }

                var stopAnimationIndicator =
                    RedoItems.FirstOrDefault(
                                             clpHistoryItem =>
                                                 clpHistoryItem is AnimationIndicatorHistoryAction &&
                                                 (clpHistoryItem as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Stop);
                var stopIndex = RedoItems.IndexOf(stopAnimationIndicator);
                if (stopIndex > -1)
                {
                    var animationRedoItems = RedoItems.Take(stopIndex + 1);
                    RedoItems = new ObservableCollection<IHistoryAction>(animationRedoItems);
                }
                else
                {
                    RedoItems.Clear();
                }
            }
            OptimizeTrashedItems();
            UpdateTicks();
        }

        public void UpdateTicks()
        {
            lock (_historyLock)
            {
                RaisePropertyChanged("CurrentHistoryIndex");
                RaisePropertyChanged("HistoryLength");
                RaisePropertyChanged("AnimationLength");
                RaisePropertyChanged("PlaybackLength");
                RaisePropertyChanged("IsAnimation");
                RaisePropertyChanged("IsPlaybackEnabled");

                var currentTick = 0;

                var combinedHistory = CompleteOrderedHistoryItems;

                var startAnimationIndicator =
                    combinedHistory.FirstOrDefault(
                                                   clpHistoryItem =>
                                                       clpHistoryItem is AnimationIndicatorHistoryAction &&
                                                       (clpHistoryItem as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Record);

                var startIndex = Math.Max(0, combinedHistory.IndexOf(startAnimationIndicator));
                var playbackItems = UndoItems.Reverse().Skip(startIndex);

                foreach (var clpHistoryItem in playbackItems)
                {
                    if (clpHistoryItem is IHistoryBatch)
                    {
                        currentTick += (clpHistoryItem as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        currentTick++;
                    }
                }

                if (RedoItems.Any() &&
                    RedoItems.First() is IHistoryBatch)
                {
                    var clpHistoryItem = RedoItems.First() as IHistoryBatch;
                    if (clpHistoryItem != null)
                    {
                        currentTick += clpHistoryItem.CurrentBatchTickIndex;
                    }
                }

                CurrentHistoryTick = currentTick;
            }
        }

        public void MoveToHistoryPoint(double oldTick, double newTick)
        {
            var diff = Convert.ToInt32(newTick - oldTick);

            if (diff > 0)
            {
                while (diff > 0)
                {
                    diff--;
                    Redo(true);
                }
            }
            else if (diff < 0)
            {
                while (diff < 0)
                {
                    diff++;
                    Undo(true);
                }
            }
        }

        public void BeginBatch(IHistoryBatch batch)
        {
            EndBatch();

            CurrentHistoryBatch = batch;
        }

        public IHistoryBatch EndBatch()
        {
            if (CurrentHistoryBatch == null)
            {
                return null;
            }

            var batch = CurrentHistoryBatch;
            batch.CurrentBatchTickIndex = batch.NumberOfBatchTicks;
            CurrentHistoryBatch = null;

            AddHistoryItem(batch);

            return batch;
        }

        public bool ForceIntoAnimation(CLPPage page, Person owner)
        {
            EndBatch();

            if (_isUndoingOperation || !UseHistory)
            {
                return false;
            }

            lock (_historyLock)
            {
                if (!UndoItems.Any() || IsAnimation)
                {
                    return false;
                }

                UndoItems.Insert(0, new AnimationIndicatorHistoryAction(page, owner, AnimationIndicatorType.Stop));
                UndoItems.Add(new AnimationIndicatorHistoryAction(page, owner, AnimationIndicatorType.Record));
                RedoItems.Clear();
                RefreshHistoryIndexes();
            }

            UpdateTicks();
            return true;
        }

        public bool AddHistoryItem(IHistoryAction historyAction)
        {
            EndBatch();

            if (_isUndoingOperation ||
                !UseHistory ||
                IsAnimation)
            {
                return false;
            }

            lock (_historyLock)
            {
                historyAction.HistoryActionIndex = UndoItems.Count;
                historyAction.CachedFormattedValue = historyAction.FormattedValue;
                UndoItems.Insert(0, historyAction);
                RedoItems.Clear();
            }

            UpdateTicks();
            return true;
        }

        //HACK
        public void ConversionUndo()
        {
            lock (_historyLock)
            {
                var historyItem = UndoItems.FirstOrDefault();
                if (historyItem == null)
                {
                    return;
                }
                historyItem.ConversionUndo();

                UndoItems.RemoveFirst();
                RedoItems.Insert(0, historyItem);
            }
        }

        public bool Undo(bool isAnimationUndo = false)
        {
            if (!CanUndo)
            {
                return false;
            }

            IHistoryAction undo = null;

            lock (_historyLock)
            {
                //This if-statement exists to correctly undo any batch items that are half-way through
                //playing, in the event an animation was paused in the middle of the batch. The historyAction
                //is already in the RedoItems list, so we don't need to move from the UndoItems list.
                if (RedoItems.Any() &&
                    RedoItems.First() is IHistoryBatch)
                {
                    var clpHistoryItem = RedoItems.First() as IHistoryBatch;
                    if (clpHistoryItem.CurrentBatchTickIndex >= 0)
                    {
                        _isUndoingOperation = true;
                        try
                        {
                            clpHistoryItem.Undo(isAnimationUndo);
                            UpdateTicks();
                            return true;
                        }
                        finally
                        {
                            lock (_historyLock)
                            {
                                _isUndoingOperation = false;
                            }
                        }
                    }
                }

                if (UndoItems.Count > 0)
                {
                    _isUndoingOperation = true;

                    undo = UndoItems.First();
                }
            }

            if (undo == null)
            {
                lock (_historyLock)
                {
                    _isUndoingOperation = false;
                }
                return false;
            }

            try
            {
                undo.Undo(isAnimationUndo);

                var undoBatch = undo as IHistoryBatch;
                if ((undoBatch != null && undoBatch.CurrentBatchTickIndex < 0) ||
                    undoBatch == null)
                {
                    lock (_historyLock)
                    {
                        UndoItems.RemoveFirst();
                        RedoItems.Insert(0, undo);
                    }
                }
                UpdateTicks();
                return true;
            }
            finally
            {
                lock (_historyLock)
                {
                    _isUndoingOperation = false;
                }
            }
        }

        public bool Redo(bool isAnimationRedo = false)
        {
            if (!CanRedo)
            {
                return false;
            }

            IHistoryAction redo = null;

            lock (_historyLock)
            {
                //This if-statement exists to correctly redo any batch items that are half-way through
                //playing, in the event an animation was paused in the middle of the batch. The historyAction
                //is already in the UndoItems list, so we don't need to move from the RedoItems list.
                if (UndoItems.Any() &&
                    UndoItems.First() is IHistoryBatch)
                {
                    var clpHistoryItem = UndoItems.First() as IHistoryBatch;
                    if (clpHistoryItem.CurrentBatchTickIndex < clpHistoryItem.NumberOfBatchTicks)
                    {
                        _isUndoingOperation = true;
                        try
                        {
                            clpHistoryItem.Redo(isAnimationRedo);
                            UpdateTicks();
                            return true;
                        }
                        finally
                        {
                            lock (_historyLock)
                            {
                                _isUndoingOperation = false;
                            }
                        }
                    }
                }

                if (RedoItems.Count > 0)
                {
                    _isUndoingOperation = true;

                    redo = RedoItems.First();
                }
            }

            if (redo == null)
            {
                lock (_historyLock)
                {
                    _isUndoingOperation = false;
                }
                return false;
            }

            try
            {
                redo.Redo(isAnimationRedo);

                var redoBatch = redo as IHistoryBatch;
                if ((redoBatch != null && redoBatch.CurrentBatchTickIndex > redoBatch.NumberOfBatchTicks) ||
                    redoBatch == null)
                {
                    lock (_historyLock)
                    {
                        RedoItems.RemoveFirst();
                        UndoItems.Insert(0, redo);
                    }
                }
                UpdateTicks();
                return true;
            }
            finally
            {
                lock (_historyLock)
                {
                    _isUndoingOperation = false;
                }
            }
        }

        public Stroke GetStrokeByID(string id)
        {
            return !TrashedInkStrokes.Any() ? null : TrashedInkStrokes.FirstOrDefault(stroke => stroke.GetStrokeID() == id);
        }

        public IPageObject GetPageObjectByID(string id)
        {
            return !TrashedPageObjects.Any() ? null : TrashedPageObjects.FirstOrDefault(pageObject => pageObject != null && pageObject.ID == id);
        }

        #endregion //Methods

        #region Static Methods

        /// <summary>Forces the UI Thread to sleep for the given number of milliseconds.</summary>
        /// <param name="timeToWait"></param>
        public static void UISleep(int timeToWait)
        {
            var frame = new DispatcherFrame();
            new Thread(() =>
                       {
                           Thread.Sleep(timeToWait);
                           frame.Continue = false;
                       }).Start();
            Dispatcher.PushFrame(frame);
        }

        #endregion //Static Methods
    }
}