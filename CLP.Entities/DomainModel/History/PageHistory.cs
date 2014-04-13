﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Collections;
using Catel.Data;

namespace CLP.Entities
{
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
        public PageHistory() { }

        /// <summary>
        /// Initializes <see cref="PageHistory" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public PageHistory(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

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
        /// Total Number of actions in the entire history.
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

        #endregion //Properties

        #region Methods

        //Completely clear history.
        public void ClearHistory()
        {
            lock(_historyLock)
            {
                UndoItems.Clear();
                RedoItems.Clear();
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
            UpdateTicks();
        }

        public void UpdateTicks()
        {
            lock(_historyLock)
            {
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

        #endregion //Methods
    }
}