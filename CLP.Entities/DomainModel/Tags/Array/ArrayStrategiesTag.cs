using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class ArrayStrategiesTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ArrayStrategiesTag" /> from scratch.</summary>
        public ArrayStrategiesTag()
        { }

        /// <summary>Initializes <see cref="ArrayStrategiesTag" /> from values.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ObjectTypesOnPageTag" /> belongs to.</param>
        /// <param name="origin"></param>
        /// <param name="currentUserID"></param>
        public ArrayStrategiesTag(CLPPage parentPage, Origin origin, List<IHistoryAction> historyActions)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            HistoryActions = historyActions;
        }

        /// <summary>Initializes <see cref="ArrayStrategiesTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayStrategiesTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of all the Array Strategies used on the <see cref="CLPPage" />.</summary>
        public List<IHistoryAction> HistoryActions
        {
            get { return GetValue<List<IHistoryAction>>(HistoryActionsProperty); }
            set { SetValue(HistoryActionsProperty, value); }
        }

        public static readonly PropertyData HistoryActionsProperty = RegisterProperty("HistoryActions", typeof(List<IHistoryAction>));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedName
        {
            get { return "Array Strategies"; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Join("\n", HistoryActions.Select(h => h.CodedValue));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
