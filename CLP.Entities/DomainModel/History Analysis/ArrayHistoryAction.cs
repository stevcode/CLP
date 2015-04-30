using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public class ArrayHistoryAction : AHistoryActionBase
    {
        public enum ArrayActions
        {
            Cut,
            Divide,
            InkDivide,
            Rotate,
            Snap
        }
        
        #region Constructors
        public ArrayHistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            :base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
        }

        /// <summary>Initializes <see cref="ArrayHistoryAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayHistoryAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion //Constructors

        #region Properties
        /// <summary>
        /// The type of Array Action this HistoryAction represents.
        /// </summary>
        public ArrayActions ArrayAction
        {
            get { return GetValue<ArrayActions>(ArrayActionProperty); }
            set { SetValue(ArrayActionProperty, value); }
        }

        public static readonly PropertyData ArrayActionProperty = RegisterProperty("ArrayAction", typeof(ArrayActions));

        public override string CodedValue
        {
            get 
            {
                switch (ArrayAction)
                {
                    case ArrayActions.Cut:
                        return string.Format("ARR cut[{0}x{1}: {2}x{3}, {4}x{5}]", 6, 7, 2, 7, 4, 7); //array rows/columns, smaller arrays
                    case ArrayActions.Divide:
                        return string.Format("ARR divide[{0}x{1}: {2}x{3}, {4}x{5}]", 6, 7, 2, 7, 4, 7); //array rows/columns, smaller arrays
                    case ArrayActions.InkDivide:
                        return string.Format("ARR ink divide[{0}x{1}: {2}x{3}, {4}x{5}]", 8, 8, 4, 4, 4, "4a"); //array rows/columns, smaller arrays
                    case ArrayActions.Rotate:
                        return string.Format("ARR rotate[{0}x{1}:{2}x{3}", 6, 7, 7, 6); //array rows/columns
                    case ArrayActions.Snap:
                        return string.Format("ARR snap[{0}x{1}, {2}x{3}:{4}x{5}", 2, 7, 4, 7, 6, 7); //array rows/columns
                    default:
                        return "ARR modified";
                }
            }
        }

        #endregion //Properties
    }
}
