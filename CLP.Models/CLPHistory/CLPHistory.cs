using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Collections;
using Catel.Data;
using Microsoft.Ink;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistory : ModelBase
    {
        private readonly object _historyLock = new object();
        public const double SAMPLE_RATE = 9;
        public IHistoryBatch CurrentHistoryBatch;

        private bool _isUndoingOperation; //don't serialize?

        #region Constructors

        public CLPHistory()
        {
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        public int CurrentAnimationDelay
        {
            get
            {
                return RedoItems.First().AnimationDelay;
            }
        }

        /// <summary>
        /// All events available for Undo.
        /// </summary>
        public ObservableCollection<ICLPHistoryItem> UndoItems
        {
            get { return GetValue<ObservableCollection<ICLPHistoryItem>>(UndoItemsProperty); }
            set { SetValue(UndoItemsProperty, value); }
        }

        public static readonly PropertyData UndoItemsProperty = RegisterProperty("UndoItems", typeof(ObservableCollection<ICLPHistoryItem>), () => new ObservableCollection<ICLPHistoryItem>());

        /// <summary>
        /// All events available for Redo.
        /// </summary>
        public ObservableCollection<ICLPHistoryItem> RedoItems
        {
            get { return GetValue<ObservableCollection<ICLPHistoryItem>>(RedoItemsProperty); }
            set { SetValue(RedoItemsProperty, value); }
        }

        public static readonly PropertyData RedoItemsProperty = RegisterProperty("RedoItems", typeof(ObservableCollection<ICLPHistoryItem>), () => new ObservableCollection<ICLPHistoryItem>());

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

        #endregion //Properties

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

        public void UpdateTicks()
        {
            lock(_historyLock)
            {
                var totalTicks = 0;
                var currentTick = 0;
                foreach(var clpHistoryItem in UndoItems)
                {
                    if(clpHistoryItem is IHistoryBatch)
                    {
                        totalTicks += (clpHistoryItem as IHistoryBatch).NumberOfBatchTicks;
                        currentTick += (clpHistoryItem as IHistoryBatch).NumberOfBatchTicks;
                    }
                    else
                    {
                        totalTicks++;
                        currentTick++;
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
                if(RedoItems.Any() && RedoItems.First() is IHistoryBatch)
                {
                    var clpHistoryItem = (RedoItems.First() as IHistoryBatch);
                    currentTick += clpHistoryItem.CurrentBatchTickIndex;
                }
                CurrentHistoryTick = currentTick;
                TotalHistoryTicks = totalTicks;
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

        public bool AddHistoryItem(ICLPHistoryItem historyItem)
        {
            EndBatch();

            if(_isUndoingOperation || !UseHistory)
            {
                return false;
            }

            lock(_historyLock)
            {
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

            ICLPHistoryItem undo = null;

            lock(_historyLock)
            {
                //This if-statement exists to correctly undo any batch items that are half-way through
                //playing, in the event an animation was paused in the middle of the batch. The historyItem
                //is already in the RedoItems list, so we don't need to move from the UndoItems list.
                //TODO: implement in Redo() if we ever want to "Play" an animation backwards.
                if(RedoItems.Any() && RedoItems.First() is IHistoryBatch)
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
                if((undoBatch != null && undoBatch.CurrentBatchTickIndex < 0) || undoBatch == null)
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

            ICLPHistoryItem redo = null;

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
                if((redoBatch != null && redoBatch.CurrentBatchTickIndex > redoBatch.NumberOfBatchTicks) || redoBatch == null)
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
    }
}