using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryAggregation : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryAggregation(CLPPage page, List<CLPHistoryItem> events) : base(HistoryItemType.Aggregation, page)
        {
            Events = events;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryAggregation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Events part of this grouping
        /// </summary>
        public List<CLPHistoryItem> Events
        {
            get { return GetValue<List<CLPHistoryItem>>(EventsProperty); }
            set { SetValue(EventsProperty, value); }
        }

        public static readonly PropertyData EventsProperty = RegisterProperty("Events", typeof(List<CLPHistoryItem>), null);

        #endregion //Properties

        #region Methods

        public override CLPHistoryItem GetUndoFingerprint()
        {
            List<CLPHistoryItem> inverse = new List<CLPHistoryItem>();
            foreach(CLPHistoryItem item in Events) 
            {
                inverse.Add(item.GetUndoFingerprint());
            }
            inverse.Reverse();
            return new CLPHistoryAggregation(Page, inverse);		  
        }

        public override CLPHistoryItem GetRedoFingerprint()
        {
            return this;
        }

        override public void Undo()
        {
            Console.WriteLine("Undoing an aggregation");
            foreach(CLPHistoryItem item in Events) 
            {
                Console.WriteLine("Undoing a " + item.ItemType);
                item.Undo();
            }
        }

        override public void Redo()
        {
            foreach(CLPHistoryItem item in Events)
            {
                item.Redo();
            }
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        #endregion //Methods
    }
}