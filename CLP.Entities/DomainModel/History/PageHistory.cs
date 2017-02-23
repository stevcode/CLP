using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Ink;
using System.Windows.Threading;
using System.Xml.Serialization;
using Catel.Collections;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    [Serializable]
    public class PageHistory : AEntityBase
    {
        #region Constants

        public const double SAMPLE_RATE = 9;

        #endregion // Constants

        #region Fields

        private readonly object _historyLock = new object();
        private bool _isUndoingOperation;

        public IHistoryBatch CurrentHistoryBatch;

        #endregion // Fields

        #region Constructors

        public PageHistory() { }

        #endregion //Constructors

        #region Properties

        public bool IsHistoryEnabled
        {
            get { return GetValue<bool>(IsHistoryEnabledProperty); }
            set { SetValue(IsHistoryEnabledProperty, value); }
        }

        public static readonly PropertyData IsHistoryEnabledProperty = RegisterProperty("IsHistoryEnabled", typeof(bool), true);

        #region HistoryActions

        /// <summary>All actions available for Undo.</summary>
        public ObservableCollection<IHistoryAction> UndoActions
        {
            get { return GetValue<ObservableCollection<IHistoryAction>>(UndoActionsProperty); }
            set { SetValue(UndoActionsProperty, value); }
        }

        public static readonly PropertyData UndoActionsProperty = RegisterProperty("UndoActions", typeof(ObservableCollection<IHistoryAction>), () => new ObservableCollection<IHistoryAction>());

        /// <summary>All actions available for Redo.</summary>
        public ObservableCollection<IHistoryAction> RedoActions
        {
            get { return GetValue<ObservableCollection<IHistoryAction>>(RedoActionsProperty); }
            set { SetValue(RedoActionsProperty, value); }
        }

        public static readonly PropertyData RedoActionsProperty = RegisterProperty("RedoActions", typeof(ObservableCollection<IHistoryAction>), () => new ObservableCollection<IHistoryAction>());

        public List<IHistoryAction> CompleteOrderedHistoryActions => UndoActions.Reverse().Concat(RedoActions).ToList();

        public List<IHistoryAction> OrderedAnimationHistoryActions
        {
            get
            {
                var combinedHistory = CompleteOrderedHistoryActions;

                var startAnimationIndicator =
                    combinedHistory.FirstOrDefault(h => h is AnimationIndicatorHistoryAction && (h as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Record);
                var stopAnimationIndicator =
                    combinedHistory.FirstOrDefault(h => h is AnimationIndicatorHistoryAction && (h as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Stop);
                var startIndex = combinedHistory.IndexOf(startAnimationIndicator);
                var stopIndex = combinedHistory.IndexOf(stopAnimationIndicator);
                if (stopIndex > -1) //HACK: This garauntees correct historyActions in the scenario when a student hits Record, then Clear Page, does work and then hits Stop.
                {
                    startIndex = 0;
                }

                var animationHistoryActions = new List<IHistoryAction>();
                if (stopIndex == 0) //HACK: Above Hack, but where first UndoAction is a Stop.
                {
                    return animationHistoryActions;
                }

                if (startIndex > -1)
                {
                    animationHistoryActions = combinedHistory.Skip(startIndex).ToList();
                }
                else
                {
                    return animationHistoryActions;
                }

                if (stopIndex > 0)
                {
                    animationHistoryActions = animationHistoryActions.Take(stopIndex - startIndex + 1).ToList();
                }
                else //HACK: This garauntees correct historyActions in the scenario when a student hits Record, but Submits page before hitting Stop.
                {
                    animationHistoryActions = animationHistoryActions.Take(combinedHistory.Count - startIndex).ToList();
                }

                return animationHistoryActions;
            }
        }

        public bool CanUndo
        {
            get
            {
                lock (_historyLock)
                {
                    return !_isUndoingOperation && UndoActions.Any() && IsHistoryEnabled;
                }
            }
        }

        public bool CanRedo
        {
            get
            {
                lock (_historyLock)
                {
                    return !_isUndoingOperation && RedoActions.Any() && IsHistoryEnabled;
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

        public static readonly PropertyData CurrentHistoryTickProperty = RegisterProperty("CurrentHistoryTick", typeof(double), 0.0);

        public int CurrentHistoryIndex => UndoActions.Any() ? UndoActions.First().HistoryActionIndex : 0;

        public IHistoryAction CurrentHistoryAction => UndoActions.FirstOrDefault();

        public int CurrentAnimationDelay => RedoActions.Any() ? RedoActions.First().AnimationDelay : 0;

        public bool IsAnimation
        {
            get
            {
                return UndoActions.Any(h => h is AnimationIndicatorHistoryAction && (h as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Stop) ||
                       RedoActions.Any(h => h is AnimationIndicatorHistoryAction && (h as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Stop);
            }
        }

        /// <summary>Forces playback on non-animation pages.</summary>
        [XmlIgnore] [JsonIgnore] [ExcludeFromSerialization] private bool _isNonAnimationPlaybackEnabled = false;

        [XmlIgnore]
        [JsonIgnore]
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

        public bool IsPlaybackEnabled => IsAnimation || IsNonAnimationPlaybackEnabled;

        #endregion //Playback Indication

        #region Playback Lengths

        public int HistoryLength
        {
            get
            {
                var totalTicks = 0;
                foreach (var historyAction in CompleteOrderedHistoryActions)
                {
                    if (historyAction is IHistoryBatch)
                    {
                        totalTicks += (historyAction as IHistoryBatch).NumberOfBatchTicks;
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

                foreach (var historyAction in OrderedAnimationHistoryActions)
                {
                    if (historyAction is IHistoryBatch)
                    {
                        totalAnimationTicks += (historyAction as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        totalAnimationTicks++;
                    }
                }

                return totalAnimationTicks;
            }
        }

        public double PlaybackLength => IsNonAnimationPlaybackEnabled ? HistoryLength : AnimationLength;

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
        [JsonIgnore]
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
            foreach (var historyAction in UndoActions)
            {
                historyAction.HistoryActionIndex = UndoActions.Count - UndoActions.IndexOf(historyAction) - 1;
            }

            foreach (var historyAction in RedoActions)
            {
                historyAction.HistoryActionIndex = UndoActions.Count + RedoActions.IndexOf(historyAction);
            }
        }

        public void RefreshCachedFormattedValues()
        {
            foreach (var historyAction in UndoActions)
            {
                historyAction.CachedFormattedValue = historyAction.FormattedValue;
            }

            foreach (var historyAction in RedoActions)
            {
                historyAction.CachedFormattedValue = historyAction.FormattedValue;
            }
        }

        public void OptimizeTrashedItems()
        {
            var inkStrokesToRemove = (from trashedInkStroke in TrashedInkStrokes
                                      let isTrashedInkStrokeBeingUsed =
                                      UndoActions.Any(h => h.IsUsingTrashedInkStroke(trashedInkStroke.GetStrokeID())) || RedoActions.Any(h => h.IsUsingTrashedInkStroke(trashedInkStroke.GetStrokeID()))
                                      where !isTrashedInkStrokeBeingUsed
                                      select trashedInkStroke).ToList();
            foreach (var stroke in inkStrokesToRemove)
            {
                TrashedInkStrokes.Remove(stroke);
            }

            var pageObjectsToRemove = (from trashedPageObject in TrashedPageObjects
                                       let isTrashedPageObjectBeingUsed =
                                       UndoActions.Any(h => h.IsUsingTrashedPageObject(trashedPageObject.ID)) || RedoActions.Any(h => h.IsUsingTrashedPageObject(trashedPageObject.ID))
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
                UndoActions.Clear();
                RedoActions.Clear();
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
                    UndoActions.FirstOrDefault(h => h is AnimationIndicatorHistoryAction && (h as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Record);
                var startIndex = UndoActions.IndexOf(startAnimationIndicator);
                if (startIndex > -1)
                {
                    var animationUndoActions = UndoActions.Take(startIndex + 1);
                    UndoActions = new ObservableCollection<IHistoryAction>(animationUndoActions);
                }
                else
                {
                    UndoActions.Clear();
                }

                var stopAnimationIndicator =
                    RedoActions.FirstOrDefault(h => h is AnimationIndicatorHistoryAction && (h as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Stop);
                var stopIndex = RedoActions.IndexOf(stopAnimationIndicator);
                if (stopIndex > -1)
                {
                    var animationRedoActions = RedoActions.Take(stopIndex + 1);
                    RedoActions = new ObservableCollection<IHistoryAction>(animationRedoActions);
                }
                else
                {
                    RedoActions.Clear();
                }
            }
            OptimizeTrashedItems();
            UpdateTicks();
        }

        public void UpdateTicks()
        {
            lock (_historyLock)
            {
                RaisePropertyChanged(nameof(CurrentHistoryIndex));
                RaisePropertyChanged(nameof(CurrentHistoryAction));
                RaisePropertyChanged(nameof(HistoryLength));
                RaisePropertyChanged(nameof(AnimationLength));
                RaisePropertyChanged(nameof(PlaybackLength));
                RaisePropertyChanged(nameof(IsAnimation));
                RaisePropertyChanged(nameof(IsPlaybackEnabled));

                var currentTick = 0;

                var combinedHistory = CompleteOrderedHistoryActions;

                var startAnimationIndicator =
                    combinedHistory.FirstOrDefault(h => h is AnimationIndicatorHistoryAction && (h as AnimationIndicatorHistoryAction).AnimationIndicatorType == AnimationIndicatorType.Record);

                var startIndex = Math.Max(0, combinedHistory.IndexOf(startAnimationIndicator));
                var playbackActions = UndoActions.Reverse().Skip(startIndex);

                foreach (var historyAction in playbackActions)
                {
                    if (historyAction is IHistoryBatch)
                    {
                        currentTick += (historyAction as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        currentTick++;
                    }
                }

                if (RedoActions.Any() &&
                    RedoActions.First() is IHistoryBatch)
                {
                    var historyAction = RedoActions.First() as IHistoryBatch;
                    if (historyAction != null)
                    {
                        currentTick += historyAction.CurrentBatchTickIndex;
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

            AddHistoryAction(batch);

            return batch;
        }

        public bool ForceIntoAnimation(CLPPage page, Person owner)
        {
            EndBatch();

            if (_isUndoingOperation || !IsHistoryEnabled)
            {
                return false;
            }

            lock (_historyLock)
            {
                if (!UndoActions.Any() || IsAnimation)
                {
                    return false;
                }

                UndoActions.Insert(0, new AnimationIndicatorHistoryAction(page, owner, AnimationIndicatorType.Stop));
                UndoActions.Add(new AnimationIndicatorHistoryAction(page, owner, AnimationIndicatorType.Record));
                RedoActions.Clear();
                RefreshHistoryIndexes();
            }

            UpdateTicks();
            return true;
        }

        public bool AddHistoryAction(IHistoryAction historyAction)
        {
            EndBatch();

            if (_isUndoingOperation ||
                !IsHistoryEnabled ||
                IsAnimation)
            {
                return false;
            }

            lock (_historyLock)
            {
                historyAction.HistoryActionIndex = UndoActions.Count;
                historyAction.CachedFormattedValue = historyAction.FormattedValue;
                UndoActions.Insert(0, historyAction);
                RedoActions.Clear();
            }

            UpdateTicks();
            return true;
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
                //is already in the RedoActions list, so we don't need to move from the UndoActions list.
                if (RedoActions.Any() &&
                    RedoActions.First() is IHistoryBatch)
                {
                    var historyAction = RedoActions.First() as IHistoryBatch;
                    if (historyAction.CurrentBatchTickIndex >= 0)
                    {
                        _isUndoingOperation = true;
                        try
                        {
                            historyAction.Undo(isAnimationUndo);
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

                if (UndoActions.Count > 0)
                {
                    _isUndoingOperation = true;

                    undo = UndoActions.First();
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
                        UndoActions.RemoveFirst();
                        RedoActions.Insert(0, undo);
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
                //is already in the UndoActions list, so we don't need to move from the RedoActions list.
                if (UndoActions.Any() &&
                    UndoActions.First() is IHistoryBatch)
                {
                    var historyAction = UndoActions.First() as IHistoryBatch;
                    if (historyAction.CurrentBatchTickIndex < historyAction.NumberOfBatchTicks)
                    {
                        _isUndoingOperation = true;
                        try
                        {
                            historyAction.Redo(isAnimationRedo);
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

                if (RedoActions.Count > 0)
                {
                    _isUndoingOperation = true;

                    redo = RedoActions.First();
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
                        RedoActions.RemoveFirst();
                        UndoActions.Insert(0, redo);
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