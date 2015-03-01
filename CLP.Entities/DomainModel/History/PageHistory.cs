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

        /// <summary>
        /// Initializes <see cref="PageHistory" /> from scratch.
        /// </summary>
        public PageHistory()
        {
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>
        /// Initializes <see cref="PageHistory" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public PageHistory(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="PageHistory" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Unique Identifier for the <see cref="Person" /> who owns the <see cref="PageHistory" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// Also Foregin Key for <see cref="Person" /> who owns the <see cref="PageHistory" />.
        /// </remarks>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary>
        /// Version Index of the <see cref="PageHistory" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        public uint VersionIndex
        {
            get { return GetValue<uint>(VersionIndexProperty); }
            set { SetValue(VersionIndexProperty, value); }
        }

        public static readonly PropertyData VersionIndexProperty = RegisterProperty("VersionIndex", typeof(uint), 0);

        /// <summary>
        /// Version Index of the latest submission.
        /// </summary>
        public uint? LastVersionIndex
        {
            get { return GetValue<uint?>(LastVersionIndexProperty); }
            set { SetValue(LastVersionIndexProperty, value); }
        }

        public static readonly PropertyData LastVersionIndexProperty = RegisterProperty("LastVersionIndex", typeof(uint?));

        /// <summary>
        /// Differentiation Level of the <see cref="PageHistory" />.
        /// </summary>
        public string DifferentiationLevel
        {
            get { return GetValue<string>(DifferentiationLevelProperty); }
            set { SetValue(DifferentiationLevelProperty, value); }
        }

        public static readonly PropertyData DifferentiationLevelProperty = RegisterProperty("DifferentiationLevel", typeof(string), "0");

        public int CurrentAnimationDelay
        {
            get { return RedoItems.Any() ? RedoItems.First().AnimationDelay : 0; }
        }

        /// <summary>
        /// All events available for Undo.
        /// </summary>
        public ObservableCollection<IHistoryItem> UndoItems
        {
            get { return GetValue<ObservableCollection<IHistoryItem>>(UndoItemsProperty); }
            set { SetValue(UndoItemsProperty, value); }
        }

        public static readonly PropertyData UndoItemsProperty = RegisterProperty("UndoItems", typeof(ObservableCollection<IHistoryItem>), () => new ObservableCollection<IHistoryItem>());

        /// <summary>
        /// All events available for Redo.
        /// </summary>
        public ObservableCollection<IHistoryItem> RedoItems
        {
            get { return GetValue<ObservableCollection<IHistoryItem>>(RedoItemsProperty); }
            set { SetValue(RedoItemsProperty, value); }
        }

        public static readonly PropertyData RedoItemsProperty = RegisterProperty("RedoItems", typeof(ObservableCollection<IHistoryItem>), () => new ObservableCollection<IHistoryItem>());

        public bool UseHistory
        {
            get { return GetValue<bool>(UseHistoryProperty); }
            set { SetValue(UseHistoryProperty, value); }
        }

        public static readonly PropertyData UseHistoryProperty = RegisterProperty("UseHistory", typeof(bool), true);

        /// <summary>
        /// Flag to signify history is currently playing or rewinding.
        /// </summary>
        public bool IsAnimating
        {
            get { return GetValue<bool>(IsAnimatingProperty); }
            set { SetValue(IsAnimatingProperty, value); }
        }

        public static readonly PropertyData IsAnimatingProperty = RegisterProperty("IsAnimating", typeof(bool), false);

        public bool CanUndo
        {
            get
            {
                lock(_historyLock)
                {
                    return !_isUndoingOperation && UndoItems.Any() && UseHistory;
                }
            }
        }

        public bool CanRedo
        {
            get
            {
                lock(_historyLock)
                {
                    return !_isUndoingOperation && RedoItems.Any() && UseHistory;
                }
            }
        }

        /// <summary>
        /// Total Number of actions in the animation.
        /// </summary>
        public double TotalHistoryTicks
        {
            get { return GetValue<double>(TotalHistoryTicksProperty); }
            set { SetValue(TotalHistoryTicksProperty, value); }
        }

        public static readonly PropertyData TotalHistoryTicksProperty = RegisterProperty("TotalHistoryTicks", typeof(double), 0);

        /// <summary>
        /// Current position within the History.
        /// </summary>
        public double CurrentHistoryTick
        {
            get { return GetValue<double>(CurrentHistoryTickProperty); }
            set { SetValue(CurrentHistoryTickProperty, value); }
        }

        public static readonly PropertyData CurrentHistoryTickProperty = RegisterProperty("CurrentHistoryTick", typeof(double), 0);

        public bool IsAnimation
        {
            get
            {
                return UndoItems.Any(clpHistoryItem => clpHistoryItem is AnimationIndicator && (clpHistoryItem as AnimationIndicator).AnimationIndicatorType == AnimationIndicatorType.Stop) ||
                       RedoItems.Any(clpHistoryItem => clpHistoryItem is AnimationIndicator && (clpHistoryItem as AnimationIndicator).AnimationIndicatorType == AnimationIndicatorType.Stop);
            }
        }

        public int HistoryLength
        {
            get
            {
                var totalTicks = 0;
                foreach(var clpHistoryItem in UndoItems)
                {
                    if(clpHistoryItem is IHistoryBatch)
                    {
                        totalTicks += (clpHistoryItem as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        totalTicks++;
                    }
                }

                foreach(var clpHistoryItem in RedoItems)
                {
                    if(clpHistoryItem is IHistoryBatch)
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

        /// <summary>
        /// A list of all the <see cref="IPageObject" />s that have been removed from a <see cref="CLPPage" />, but are needed for the <see cref="PageHistory" />.
        /// </summary>
        public List<IPageObject> TrashedPageObjects
        {
            get { return GetValue<List<IPageObject>>(TrashedPageObjectsProperty); }
            set { SetValue(TrashedPageObjectsProperty, value); }
        }

        public static readonly PropertyData TrashedPageObjectsProperty = RegisterProperty("TrashedPageObjects", typeof(List<IPageObject>), () => new List<IPageObject>());

        /// <summary>
        /// A list of all the <see cref="Stroke" />s that have been removed from a <see cref="CLPPage" />, but are needed for the <see cref="PageHistory" />.
        /// </summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public StrokeCollection TrashedInkStrokes
        {
            get { return GetValue<StrokeCollection>(TrashedInkStrokesProperty); }
            set { SetValue(TrashedInkStrokesProperty, value); }
        }

        public static readonly PropertyData TrashedInkStrokesProperty = RegisterProperty("TrashedInkStrokes", typeof(StrokeCollection), () => new StrokeCollection());

        /// <summary>
        /// A list of all the serialized <see cref="Stroke" />s that have been removed from a <see cref="CLPPage" />, but are needed for the <see cref="PageHistory" />.
        /// </summary>
        public List<StrokeDTO> SerializedTrashedInkStrokes
        {
            get { return GetValue<List<StrokeDTO>>(SerializedTrashedInkStrokesProperty); }
            set { SetValue(SerializedTrashedInkStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedTrashedInkStrokesProperty = RegisterProperty("SerializedTrashedInkStrokes", typeof(List<StrokeDTO>), () => new List<StrokeDTO>());

        #endregion //Properties

        #region Methods

        public void RefreshHistoryIndexes()
        {
            foreach (var historyItem in UndoItems)
            {
                historyItem.HistoryIndex = UndoItems.Count - UndoItems.IndexOf(historyItem) - 1;
            }

            foreach (var historyItem in RedoItems)
            {
                historyItem.HistoryIndex = UndoItems.Count + RedoItems.IndexOf(historyItem);
            }
        }

        public void OptimizeTrashedItems()
        {
            var inkStrokesToRemove = (from trashedInkStroke in TrashedInkStrokes
                                      let isTrashedInkStrokeBeingUsed = UndoItems.Any(historyItem => historyItem.IsUsingTrashedInkStroke(trashedInkStroke.GetStrokeID(), true)) || 
                                                                        RedoItems.Any(historyItem => historyItem.IsUsingTrashedInkStroke(trashedInkStroke.GetStrokeID(), false))
                                      where !isTrashedInkStrokeBeingUsed
                                      select trashedInkStroke).ToList();
            foreach(var stroke in inkStrokesToRemove)
            {
                TrashedInkStrokes.Remove(stroke);
            }


            var pageObjectsToRemove = (from trashedPageObject in TrashedPageObjects
                                       let isTrashedPageObjectBeingUsed = UndoItems.Any(historyItem => historyItem.IsUsingTrashedPageObject(trashedPageObject.ID, true)) || 
                                                                          RedoItems.Any(historyItem => historyItem.IsUsingTrashedPageObject(trashedPageObject.ID, false))
                                       where !isTrashedPageObjectBeingUsed
                                       select trashedPageObject).ToList();
            foreach(var pageObject in pageObjectsToRemove)
            {
                TrashedPageObjects.Remove(pageObject);
            }
        }

        //Completely clear history.
        public void ClearHistory()
        {
            lock(_historyLock)
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
            lock(_historyLock)
            {
                var startAnimationIndicator =
                    UndoItems.FirstOrDefault(clpHistoryItem => clpHistoryItem is AnimationIndicator && (clpHistoryItem as AnimationIndicator).AnimationIndicatorType == AnimationIndicatorType.Record);
                var startIndex = UndoItems.IndexOf(startAnimationIndicator);
                if(startIndex > -1)
                {
                    var animationUndoItems = UndoItems.Take(startIndex + 1);
                    UndoItems = new ObservableCollection<IHistoryItem>(animationUndoItems);
                }
                else
                {
                    UndoItems.Clear();
                }

                var stopAnimationIndicator =
                    RedoItems.FirstOrDefault(clpHistoryItem => clpHistoryItem is AnimationIndicator && (clpHistoryItem as AnimationIndicator).AnimationIndicatorType == AnimationIndicatorType.Stop);
                var stopIndex = RedoItems.IndexOf(stopAnimationIndicator);
                if(stopIndex > -1)
                {
                    var animationRedoItems = RedoItems.Take(stopIndex + 1);
                    RedoItems = new ObservableCollection<IHistoryItem>(animationRedoItems);
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
            lock(_historyLock)
            {
                RaisePropertyChanged("HistoryLength");

                var totalTicks = 0;
                var currentTick = 0;

                var combinedHistory = UndoItems.Reverse().Concat(RedoItems).ToList();
                var startAnimationIndicator =
                    combinedHistory.FirstOrDefault(
                                                   clpHistoryItem =>
                                                   clpHistoryItem is AnimationIndicator && (clpHistoryItem as AnimationIndicator).AnimationIndicatorType == AnimationIndicatorType.Record);
                var stopAnimationIndicator =
                    combinedHistory.FirstOrDefault(
                                                   clpHistoryItem =>
                                                   clpHistoryItem is AnimationIndicator && (clpHistoryItem as AnimationIndicator).AnimationIndicatorType == AnimationIndicatorType.Stop);
                var startIndex = combinedHistory.IndexOf(startAnimationIndicator);
                var stopIndex = combinedHistory.IndexOf(stopAnimationIndicator);

                List<IHistoryItem> animationHistoryItems;
                if(startIndex > -1)
                {
                    animationHistoryItems = combinedHistory.Skip(startIndex).ToList();
                }
                else
                {
                    CurrentHistoryTick = 0;
                    TotalHistoryTicks = 0;
                    RaisePropertyChanged("IsAnimation");
                    return;
                }

                if(stopIndex >= -1)
                {
                    animationHistoryItems = animationHistoryItems.Take(stopIndex - startIndex + 1).ToList();
                }

                foreach(var clpHistoryItem in animationHistoryItems)
                {
                    if(clpHistoryItem is IHistoryBatch)
                    {
                        totalTicks += (clpHistoryItem as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        totalTicks++;
                    }
                }

                var animationUndoItems = UndoItems.Reverse().Skip(startIndex);
                foreach(var clpHistoryItem in animationUndoItems)
                {
                    if(clpHistoryItem is IHistoryBatch)
                    {
                        currentTick += (clpHistoryItem as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        currentTick++;
                    }
                }
                if(RedoItems.Any() &&
                   RedoItems.First() is IHistoryBatch)
                {
                    var clpHistoryItem = (RedoItems.First() as IHistoryBatch);
                    currentTick += clpHistoryItem.CurrentBatchTickIndex;
                }

                CurrentHistoryTick = currentTick;
                TotalHistoryTicks = totalTicks;
                RaisePropertyChanged("IsAnimation");
            }
        }

        public void MoveToHistoryPoint(double oldTick, double newTick)
        {
            var diff = Convert.ToInt32(newTick - oldTick);

            if(diff > 0)
            {
                while(diff > 0)
                {
                    diff--;
                    Redo(true);
                }
            }
            else if(diff < 0)
            {
                while(diff < 0)
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
            if(CurrentHistoryBatch == null)
            {
                return null;
            }

            var batch = CurrentHistoryBatch;
            batch.CurrentBatchTickIndex = batch.NumberOfBatchTicks;
            CurrentHistoryBatch = null;

            AddHistoryItem(batch);

            return batch;
        }

        public bool AddHistoryItem(IHistoryItem historyItem)
        {
            EndBatch();

            if(_isUndoingOperation || !UseHistory)
            {
                return false;
            }

            lock(_historyLock)
            {
                if(UndoItems.Any() &&
                   UndoItems.First() is AnimationIndicator &&
                   (UndoItems.First() as AnimationIndicator).AnimationIndicatorType == AnimationIndicatorType.Stop)
                {
                    return false;
                }

                historyItem.HistoryIndex = UndoItems.Count;
                UndoItems.Insert(0, historyItem);
                RedoItems.Clear();
            }

            UpdateTicks();
            return true;
        }

        public bool Undo(bool isAnimationUndo = false)
        {
            if(!CanUndo)
            {
                return false;
            }

            IHistoryItem undo = null;

            lock(_historyLock)
            {
                //This if-statement exists to correctly undo any batch items that are half-way through
                //playing, in the event an animation was paused in the middle of the batch. The historyItem
                //is already in the RedoItems list, so we don't need to move from the UndoItems list.
                //TODO: implement in Redo() if we ever want to "Play" an animation backwards.
                if(RedoItems.Any() &&
                   RedoItems.First() is IHistoryBatch)
                {
                    var clpHistoryItem = RedoItems.First() as IHistoryBatch;
                    if(clpHistoryItem.CurrentBatchTickIndex >= 0)
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
                            lock(_historyLock)
                            {
                                _isUndoingOperation = false;
                            }
                        }
                    }
                }

                if(UndoItems.Count > 0)
                {
                    _isUndoingOperation = true;

                    undo = UndoItems.First();
                }
            }

            if(undo == null)
            {
                lock(_historyLock)
                {
                    _isUndoingOperation = false;
                }
                return false;
            }

            try
            {
                undo.Undo(isAnimationUndo);

                var undoBatch = undo as IHistoryBatch;
                if((undoBatch != null && undoBatch.CurrentBatchTickIndex < 0) ||
                   undoBatch == null)
                {
                    lock(_historyLock)
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
                lock(_historyLock)
                {
                    _isUndoingOperation = false;
                }
            }
        }

        public bool Redo(bool isAnimationRedo = false)
        {
            if(!CanRedo)
            {
                return false;
            }

            IHistoryItem redo = null;

            lock(_historyLock)
            {
                if(RedoItems.Count > 0)
                {
                    _isUndoingOperation = true;

                    redo = RedoItems.First();
                }
            }

            if(redo == null)
            {
                lock(_historyLock)
                {
                    _isUndoingOperation = false;
                }
                return false;
            }

            try
            {
                redo.Redo(isAnimationRedo);

                var redoBatch = redo as IHistoryBatch;
                if((redoBatch != null && redoBatch.CurrentBatchTickIndex > redoBatch.NumberOfBatchTicks) ||
                   redoBatch == null)
                {
                    lock(_historyLock)
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
                lock(_historyLock)
                {
                    _isUndoingOperation = false;
                }
            }
        }

        public Stroke GetStrokeByID(string id) { return !TrashedInkStrokes.Any() ? null : TrashedInkStrokes.FirstOrDefault(stroke => stroke.GetStrokeID() == id); }

        public IPageObject GetPageObjectByID(string id) { return !TrashedPageObjects.Any() ? null : TrashedPageObjects.FirstOrDefault(pageObject => pageObject != null && pageObject.ID == id); }

        #endregion //Methods

        #region Static Methods

        /// <summary>
        /// Forces the UI Thread to sleep for the given number of milliseconds.
        /// </summary>
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