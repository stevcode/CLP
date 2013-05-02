using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistory : DataObjectBase<CLPHistory>
    {
        public const double SAMPLE_RATE = 9;
        private bool _frozen;
        private bool _ingroup;
        private Stack<CLPHistoryItem> groupEvents;

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

        public static readonly PropertyData PastProperty = RegisterProperty("Past", 
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

        public static readonly PropertyData FutureProperty = RegisterProperty("Future", 
            typeof(Stack<CLPHistoryItem>), () => new Stack<CLPHistoryItem>());

        /// <summary>
        /// The actions that have happened in the past, *including* undos and redos.
        /// </summary>
        public Stack<CLPHistoryItem> MetaPast
        {
            get { return GetValue<Stack<CLPHistoryItem>>(MetaPastProperty); }
            set { SetValue(MetaPastProperty, value); }
        }

        public static readonly PropertyData MetaPastProperty = RegisterProperty("MetaPast",
            typeof(Stack<CLPHistoryItem>), () => new Stack<CLPHistoryItem>());

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

        public void Push(CLPHistoryItem item)
        {
            if(_frozen || IsExpected(item))
            {
                return;
            }
            if(_ingroup)
            {
                Console.WriteLine("pushing a " + item.ItemType + " to group");
                groupEvents.Push(item);
            }
            else
            {
                Console.WriteLine("pushing a " + item.ItemType);
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

        public void EndEventGroup()
        {
            _ingroup = false;
            CLPHistoryItem group = AggregateItems(groupEvents);
            if(group != null)
            {
                Past.Push(group);
                MetaPast.Push(group);
                Future.Clear();
            }
        }

        public CLPHistoryItem AggregateItems(Stack<CLPHistoryItem> itemStack)
        {
            List<CLPHistoryItem> itemList = new List<CLPHistoryItem>();
            List<bool> deleted = new List<bool>();
            while(itemStack.Count > 0){
                CLPHistoryItem item = itemStack.Pop();
                itemList.Add(item);
                deleted.Add(false);
            }

            //search for removes followed by adds, since we're going backwards in time, and
            //eliminate such redundancies
            for(int i = 0; i < itemList.Count; i++)
            {
                if(!deleted[i] && itemList[i] is CLPHistoryRemoveStroke)
                {
                    for(int j = i; j < itemList.Count; j++)
                    {
                        if(!deleted[j] && itemList[j] is CLPHistoryAddStroke &&
                            CLPPage.ByteToStroke((itemList[i] as CLPHistoryRemoveStroke).Bytestroke).GetStrokeUniqueID() ==
                            CLPPage.ByteToStroke((itemList[j] as CLPHistoryAddStroke).Bytestroke).GetStrokeUniqueID())
                        {
                            deleted[i] = true;
                            deleted[j] = true;
                        }
                    }
                }
            }

            List<CLPHistoryItem> finalItemList = new List<CLPHistoryItem>();
            for(int i = 0; i < itemList.Count; i++)
            {
                if(!deleted[i])
                {
                    finalItemList.Add(itemList[i]);
                }
            }

            if(finalItemList.Count == 0)
            {
                return null;
            }
            else if(finalItemList.Count == 1)
            {
                return finalItemList[0];
            }

            return new CLPHistoryAggregation(finalItemList[0].Page, finalItemList);
        }

        public void Undo()
        {
            if(Past.Count==0)
            {
                return;
            }

            CLPHistoryItem lastAction = Past.Pop();
            CLPHistoryItem expected = lastAction.GetUndoFingerprint();
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
            lastAction.Undo();
            MetaPast.Push(expected);
            Future.Push(lastAction);
            if(lastAction is CLPHistoryMoveObject && Past.Count > 0)
            {
                CLPHistoryItem penultimateAction = Past.Peek();
                if((lastAction as CLPHistoryMoveObject).CombinesWith(penultimateAction))
                {
                    Undo();
                }
            }
        }

        public void Redo()
        {
            if(Future.Count == 0)
            {
               return;
            }
            var nextAction = Future.Pop();
            var expected = nextAction.GetRedoFingerprint();
            if(expected != null)
            {
                ExpectedEvents.Add(expected);
            }
            nextAction.Redo();
            MetaPast.Push(expected);
            Past.Push(nextAction);
            if(nextAction is CLPHistoryMoveObject && Future.Count > 0)
            {
                CLPHistoryItem penultimateAction = Future.Peek();
                if((nextAction as CLPHistoryMoveObject).CombinesWith(penultimateAction))
                {
                    Redo();
                }
            }
        }

        public void ReplaceHistoricalRecords(ICLPPageObject oldObject, ICLPPageObject newObject) 
        {
            foreach(CLPHistoryItem item in Past)
            {
                item.ReplaceHistoricalRecords(oldObject, newObject);
            }
        }   

        private bool IsExpected(CLPHistoryItem item)
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