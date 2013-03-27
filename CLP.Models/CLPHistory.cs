using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;
using System.IO;
using System.Windows.Ink;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistory : DataObjectBase<CLPHistory>
    {
        private bool memEnabled;
        private string closed = null;
        public readonly double Sample_Rate = 9;

        #region Constructor

        public CLPHistory()
        {
            memEnabled = true;
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

        public CLPHistory GetMemento(){
            var clpHistory = new CLPHistory
                {
                    Past = new Stack<CLPHistoryItem>(new Stack<CLPHistoryItem>(Past)),
                    Future = new Stack<CLPHistoryItem>(new Stack<CLPHistoryItem>(Future))
                };
            clpHistory.close();
            return clpHistory;
        }

        public void disableMem()
        {
            memEnabled = false;
        }

        public void enableMem()
        {
            memEnabled = true;
        }

        public void close()
        {
            closed = "closed";
        }

        public Boolean push(CLPHistoryItem item)
        {
            if(closed == null && memEnabled)
            {
                if(!isExpected(item))
                {
                    Console.WriteLine("pushing a " + item.ItemType);
                    Past.Push(item);
                    Future.Clear();
                }
                else
                {
                    Console.WriteLine("expected " + item.ItemType);
                }
                return true;
            }
            return false;
        }

        public CLPHistory getInitialHistory(){
            CLPHistory clpHistory = new CLPHistory();
            clpHistory.Past = new Stack<CLPHistoryItem>();
            clpHistory.Future = new Stack<CLPHistoryItem>();
            clpHistory.close();
            return clpHistory;
        }
       
        public void undo(){
            if(Past.Count==0){
                Console.WriteLine("told to undo, but nothing in stack");
                return;
            }
            Console.WriteLine("started undo");
            CLPHistoryItem lastAction = (CLPHistoryItem)Past.Pop();
            CLPHistoryItem expected = lastAction.GetUndoFingerprint();
            if(expected != null)
            {
                ExpectedEvents.Add(expected);
            }
            lastAction.Undo();
            Future.Push(lastAction);
            printMemStacks("undo", "after");
        }

        public void redo(){
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
            Past.Push(nextAction);
            printMemStacks("redo", "after");
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