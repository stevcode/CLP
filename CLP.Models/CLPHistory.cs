using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;
using System.IO;
using System.Windows.Ink;
using CLP.Models.CLPHistoryItems;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistory : DataObjectBase<CLPHistory>
    {
        public readonly double Sample_Rate = 9;
        private bool frozen;

        #region Constructor

        public CLPHistory()
        {
            frozen = false;
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

        public void Freeze()
        {
            frozen = true;
        }

        public void Unfreeze()
        {
            frozen = false;
        }

        public Boolean push(CLPHistoryItem item)
        {
            if(frozen)
            {
                return false;
            }
            if(!isExpected(item))
            {
                Console.WriteLine("pushing a " + item.ItemType);
                Past.Push(item);
                MetaPast.Push(item);
                Future.Clear();
            }
            else
            {
                Console.WriteLine("expected " + item.ItemType);
            }
            return true;
        }
        
        public void Undo()
        {
            if(Past.Count==0){
                Console.WriteLine("told to undo, but nothing in stack");
                return;
            }
            Console.WriteLine("started undo");
            CLPHistoryItem lastAction = Past.Pop();
            CLPHistoryItem expected = lastAction.GetUndoFingerprint();
            if(expected != null)
            {
                ExpectedEvents.Add(expected);
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

        public void Redo(){
            if(Future.Count == 0)
            {
               Console.WriteLine("told to redo, but nothing in stack");
               return;
            }
            CLPHistoryItem nextAction = (CLPHistoryItem)Future.Pop();
            CLPHistoryItem expected = nextAction.GetRedoFingerprint();
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

        public void printMemStacks(String methodName, String pos)
        {
            Console.WriteLine(pos + " " + methodName + " Past");
            foreach(CLPHistoryItem item in Past)
            {
                Console.WriteLine(item.ItemType);
            }
            Console.WriteLine(pos + " " + methodName + " Future");
            foreach(CLPHistoryItem item in Future)
            {
                Console.WriteLine(item.ItemType);
            }
        }

        private bool isExpected(CLPHistoryItem item)
        {
            CLPHistoryItem match = null;
            foreach (CLPHistoryItem expected in ExpectedEvents) {
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
            else
            {
                ExpectedEvents.Remove(match);
                return true;
            }
        }
    }
}