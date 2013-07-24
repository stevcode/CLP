using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistory : ModelBase
    {
        private readonly object _historyLock = new object();
        public const double SAMPLE_RATE = 9;

        private bool _isUndoingOperation;

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

        /// <summary>
        /// All events available for Undo.
        /// </summary>
        public ObservableCollection<IHistoryBatch> UndoBatches
        {
            get { return GetValue<ObservableCollection<IHistoryBatch>>(UndoBatchesProperty); }
            set { SetValue(UndoBatchesProperty, value); }
        }

        public static readonly PropertyData UndoBatchesProperty = RegisterProperty("UndoBatches", typeof(ObservableCollection<IHistoryBatch>), () => new ObservableCollection<IHistoryBatch>());

        /// <summary>
        /// All events available for Redo.
        /// </summary>
        public ObservableCollection<IHistoryBatch> RedoBatches
        {
            get { return GetValue<ObservableCollection<IHistoryBatch>>(RedoBatchesProperty); }
            set { SetValue(RedoBatchesProperty, value); }
        }

        public static readonly PropertyData RedoBatchesProperty = RegisterProperty("RedoBatches", typeof(ObservableCollection<IHistoryBatch>), () => new ObservableCollection<IHistoryBatch>());


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
                    return !_isUndoingOperation && UndoBatches.Any() && UseHistory;
                }
            }
        }

        public bool CanRedo
        {
            get
            {
                lock(_historyLock)
                {
                    return !_isUndoingOperation && RedoBatches.Any() && UseHistory;
                }
            }
        }

        #endregion //Properties

        //Forget everything that happened; lock the current state as the starting state.
        public void ClearHistory()
        {
            Past.Clear();
            MetaPast.Clear();
            Future.Clear();
            ExpectedEvents.Clear();
            if(groupEvents != null)
            {
                groupEvents.Clear();
            }
        }

        public virtual void Push(CLPHistoryItem item)
        {
            if(!UseHistory || _frozen || IsExpected(item))
            {
                return;
            }
            if(_ingroup)
            {
                //Console.WriteLine("pushing a " + item.ItemType + " to group");
                groupEvents.Push(item);
            }
            else
            {
                //Console.WriteLine("pushing a " + item.ItemType);
                //if(item is CLPHistoryAddStroke)
                //{
                //    Console.WriteLine((item as CLPHistoryAddStroke).StrokeId);
                //}
                //if(item is CLPHistoryRemoveStroke)
                //{
                //    Console.WriteLine((item as CLPHistoryRemoveStroke).StrokeId);
                //}
                Past.Push(item);
                MetaPast.Push(item);
                Future.Clear();
            }
        }

        public void BeginEventGroup()
        {
            _ingroup = true;
            groupEvents = new Stack<CLPHistoryItem>();
        }

        public virtual void EndEventGroup()
        {
            _ingroup = false;
            if(groupEvents.Count > 0)
            {
                CLPHistoryItem group = AggregateItems(groupEvents);
                Past.Push(group);
                MetaPast.Push(group);
                Future.Clear();
            }
        }

        public CLPHistoryItem AggregateItems(Stack<CLPHistoryItem> itemStack)
        {
            List<CLPHistoryItem> itemList = new List<CLPHistoryItem>();
            while(itemStack.Count > 0)
            {
                itemList.Add(itemStack.Pop());
            }
            return new CLPHistoryAggregation(itemList);
        }

        public virtual void Undo(CLPPage page)
        {
            if(!UseHistory || Past.Count==0)
            {
                return;
            }

            CLPHistoryItem lastAction = Past.Pop();
            CLPHistoryItem expected = lastAction.GetUndoFingerprint(page);
            if(expected != null)
            {
                if(expected is CLPHistoryAggregation)
                {
                    foreach(CLPHistoryItem item in (expected as CLPHistoryAggregation).Events)
                    {
                        ExpectedEvents.Add(item);
                    }
                }
                else
                {
                    ExpectedEvents.Add(expected);
                }
            }
            lastAction.Undo(page);
            MetaPast.Push(expected);
            Future.Push(lastAction);
            if(lastAction is CLPHistoryMovePageObject && Past.Count > 0)
            {
                CLPHistoryItem penultimateAction = Past.Peek();
                if((lastAction as CLPHistoryMovePageObject).CombinesWith(penultimateAction))
                {
                    Undo(page);
                }
            }
        }

        public virtual void Redo(CLPPage page)
        {
            if(!UseHistory || Future.Count == 0)
            {
               return;
            }
            var nextAction = Future.Pop();
            var expected = nextAction.GetRedoFingerprint(page);
            if(expected != null)
            {
                ExpectedEvents.Add(expected);
            }
            nextAction.Redo(page);
            MetaPast.Push(expected);
            Past.Push(nextAction);
            if(nextAction is CLPHistoryMovePageObject && Future.Count > 0)
            {
                CLPHistoryItem penultimateAction = Future.Peek();
                if((nextAction as CLPHistoryMovePageObject).CombinesWith(penultimateAction))
                {
                    Redo(page);
                }
            }
        }

        protected bool IsExpected(CLPHistoryItem item)
        {
            CLPHistoryItem match = null;
            foreach (CLPHistoryItem expected in ExpectedEvents) 
            {
                if(item.ItemType == expected.ItemType && expected.Equals(item))
                {
                    match = expected;
                    break;
                }
            }
            if(match == null)
            {
                return false;
            }
            
            ExpectedEvents.Remove(match);
            return true;
        }




        protected override void OnDeserialized()
        {
            base.OnDeserialized();
            
            //foreach IHistoryItem, ParentHistory = this;
        }
    }
}