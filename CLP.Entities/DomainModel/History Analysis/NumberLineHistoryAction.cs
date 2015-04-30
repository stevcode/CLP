using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    class NumberLineHistoryAction : AHistoryActionBase
    {
        public enum NumberLineActions
        {
            Jump,
            Change,
            InkChange
        }

        #region Constructors
        public NumberLineHistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            :base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
        }

        /// <summary>Initializes <see cref="ArrayHistoryAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public NumberLineHistoryAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion //Constructors

        #region Properties

        public NumberLineActions NumberLineAction
        {
            get { return GetValue<NumberLineActions>(NumberLineActionProperty); }
            set { SetValue(NumberLineActionProperty, value); }
        }

        public static readonly PropertyData NumberLineActionProperty = RegisterProperty("NumberLineAction", typeof(NumberLineActions));

        public override string CodedValue
        {
            get
            {
                switch (NumberLineAction)
                {
                    case NumberLineActions.Change:
                        return string.Format("NL cut[{0}x{1}: {2}x{3}, {4}x{5}]", 6, 7, 2, 7, 4, 7); //array rows/columns, smaller arrays
                    case NumberLineActions.InkChange:
                        return string.Format("NL divide[{0}x{1}: {2}x{3}, {4}x{5}]", 6, 7, 2, 7, 4, 7); //array rows/columns, smaller arrays
                    case NumberLineActions.Jump:
                        return string.Format("NL jump[{0}: {1}, {2}-{3}]", 42, 7, 0, 42); //numberline jump sizes+start/end values
                         //possibly multiple jump sizes NL jump [70: 7, 0-35, 6, 35-41, 7, 48-55]
                        //possibly off numberline
                    default:
                        return "NL modified";
                }
            }
        }

        #endregion //Properties
    }
}
