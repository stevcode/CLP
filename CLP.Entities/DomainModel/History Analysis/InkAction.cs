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
            Over,
            Left,
            Right,
            Top,
            Bottom
        }

        #region Constructors

        /// <summary>Initializes <see cref="InkAction" /> using <see cref="CLPPage" />.</summary>
        public InkAction(CLPPage parentPage)
            : base(parentPage)
        {
            var historyItems = HistoryItems;
            
        }

        /// <summary>Initializes <see cref="InkAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public InkAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// The type of Ink Action this HistoryAction represents.
        /// </summary>
        public InkActions InkActionType
        {
            get { return GetValue<InkActions>(InkActionTypeProperty); }
            set { SetValue(InkActionTypeProperty, value); }
        }

        public static readonly PropertyData InkActionTypeProperty = RegisterProperty("InkActionType", typeof(InkActions));

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
