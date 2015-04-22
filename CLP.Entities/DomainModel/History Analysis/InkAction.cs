using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public class InkAction : AHistoryActionBase
    {
        public enum InkActions
        {
            Change,
            Add,
            Erase,
            Ignore
        }

        public enum InkLocations
        {
            None,
            Over,
            Left,
            Right,
            Top,
            Bottom
        }

        #region Constructors

        /// <summary>Initializes <see cref="InkAction" /> using <see cref="CLPPage" />.</summary>
        public InkAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            : base(parentPage)
        {
            var addStrokes = false;
            var removeStrokes = false;
            foreach (var historyItem in historyItems)
            {
                if (!(historyItem is ObjectsOnPageChangedHistoryItem))
                {
                    //throw error
                }
                var pageChangedHistoryItem = (ObjectsOnPageChangedHistoryItem)historyItem;
                if (!pageChangedHistoryItem.StrokeIDsAdded.Any() && 
                    !pageChangedHistoryItem.StrokeIDsRemoved.Any()) 
                {
                    //throw error
                }
                
                if (pageChangedHistoryItem.StrokeIDsAdded.Any())
                {
                    addStrokes = true;
                }

                if (pageChangedHistoryItem.StrokeIDsRemoved.Any())
                {
                    removeStrokes = true;
                }

            }

            if (addStrokes && removeStrokes)
            {
                //throw error, can't have both
            }

            //validate

            //PageObjectStructuredID.ID = pageObject.ID;
            //PageObjectStructuredID.CodedID = pageObject.ID;
        }

        /// <summary>Initializes <see cref="InkAction" /> using <see cref="CLPPage" />.</summary>
        public InkAction(CLPPage parentPage, List<InkAction> inkAction)
            : base(parentPage)
        {
            //validate

            //PageObjectStructuredID.ID = pageObject.ID;
            //PageObjectStructuredID.CodedID = pageObject.ID;
        }

        /// <summary>Initializes <see cref="InkAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public InkAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        //Make string CollectionID

        /// <summary>
        /// The type of Ink Action this HistoryAction represents.
        /// </summary>
        public InkActions InkActionType
        {
            get { return GetValue<InkActions>(InkActionTypeProperty); }
            set { SetValue(InkActionTypeProperty, value); }
        }

        public static readonly PropertyData InkActionTypeProperty = RegisterProperty("InkActionType", typeof(InkActions));

        /// <summary>
        /// Location the Ink Action occurs.
        /// </summary>
        public InkLocations InkLocation
        {
            get { return GetValue<InkLocations>(InkLocationProperty); }
            set { SetValue(InkLocationProperty, value); }
        }

        public static readonly PropertyData InkLocationProperty = RegisterProperty("InkLocation", typeof (InkLocations));

        public override string CodedValue
        {
            get
            {
                var codedActionType = InkActionType.ToString().ToLower();
                return string.Format("INK {1} {2}", codedActionType, "[A]");
            }
        } 

        #endregion //Properties
    }
}
