using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistory : DataObjectBase<CLPHistory> , ICLPHistory
    {
        public const double SAMPLE_RATE = 9;
        protected bool _frozen;
        protected bool _ingroup;
        public bool _useHistory = false;  //History currently disabled due to bugs.
        protected Stack<CLPHistoryItem> groupEvents;

        #region Constructor

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

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// The actions that have happened in the past.  "Undo" reverses the top action on the stack and pushes
        /// it to Future.
        /// </summary>
        public Stack<CLPHistoryItem> Past
        {
            get { return GetValue<Stack<CLPHistoryItem>>(PastProperty); }
            set { SetValue(PastProperty, value); }
        }

        public volatile static  PropertyData PastProperty = RegisterProperty("Past", 
            typeof(Stack<CLPHistoryItem>), () => new Stack<CLPHistoryItem>());

        /// <summary>
        /// The actions queued to happen in the future.  "Redo" performs the top action on the stack and pushes
        /// it to Past.  Taking an action other than Undo or Redo clears the Future.
        /// </summary>
        public Stack<CLPHistoryItem> Future
        {
            get { return GetValue<Stack<CLPHistoryItem>>(FutureProperty); }
            set { SetValue(FutureProperty, value); }
        }

        public volatile static  PropertyData FutureProperty = RegisterProperty("Future", 
            typeof(Stack<CLPHistoryItem>), () => new Stack<CLPHistoryItem>());

        /// <summary>
        /// The actions that have happened in the past, *including* undos and redos.
        /// </summary>
        public Stack<CLPHistoryItem> MetaPast
        {
            get { return GetValue<Stack<CLPHistoryItem>>(MetaPastProperty); }
            set { SetValue(MetaPastProperty, value); }
        }

        public volatile static  PropertyData MetaPastProperty = RegisterProperty("MetaPast",
            typeof(Stack<CLPHistoryItem>), () => new Stack<CLPHistoryItem>());


        public bool _useHistory 
        {
            get { return GetValue<bool>(_useHistoryProperty); }
            set { SetValue(_useHistoryProperty, value); }
        }

        public static readonly PropertyData _useHistoryProperty = RegisterProperty("_useHistory",
            typeof(bool), true);
        public bool singleCutting {
            get { return GetValue<bool>(singleCuttingProperty); }
            set { SetValue(singleCuttingProperty, value); }
        
        }
        public static readonly PropertyData singleCuttingProperty = RegisterProperty("singleCutting",
           typeof(bool), false);
        
        /// <summary>
        /// The events that we have triggered and should therefore ignore when we're told they've
        /// happened.
        /// </summary>
        public List<CLPHistoryItem> ExpectedEvents
        {
            get { return GetValue<List<CLPHistoryItem>>(ExpectedEventsProperty); }
            set { SetValue(ExpectedEventsProperty, value); }
        }

        public static readonly PropertyData ExpectedEventsProperty = RegisterProperty("ExpectedEvents",
            typeof(List<CLPHistoryItem>), () => new List<CLPHistoryItem>());

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

        public void Freeze()
        {
            _frozen = true;
        }

        public void Unfreeze()
        {
            _frozen = false;
        }

        public virtual void Push(CLPHistoryItem item)
        {
            if(!_useHistory || _frozen || IsExpected(item))
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
            if(!_useHistory || Past.Count==0)
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
            if(lastAction is CLPHistoryMoveObject && Past.Count > 0)
            {
                CLPHistoryItem penultimateAction = Past.Peek();
                if((lastAction as CLPHistoryMoveObject).CombinesWith(penultimateAction))
                {
                    Undo(page);
                }
            }
        }

        public virtual void Redo(CLPPage page)
        {
            if(!_useHistory || Future.Count == 0)
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
            if(nextAction is CLPHistoryMoveObject && Future.Count > 0)
            {
                CLPHistoryItem penultimateAction = Future.Peek();
                if((nextAction as CLPHistoryMoveObject).CombinesWith(penultimateAction))
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
    }
}