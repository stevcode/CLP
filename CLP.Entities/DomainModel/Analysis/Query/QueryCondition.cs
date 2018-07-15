using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class QueryCondition : ASerializableBase
    {
        #region Constructors

        public QueryCondition() { }

        public QueryCondition(IQueryPart queryPart)
        {
            QueryPart = queryPart;
        }

        #endregion // Constructors

        #region Properties

        /// <summary></summary>
        public IQueryPart QueryPart
        {
            get => GetValue<IQueryPart>(QueryPartProperty);
            set => SetValue(QueryPartProperty, value);
        }

        public static readonly PropertyData QueryPartProperty = RegisterProperty(nameof(QueryPart), typeof(IQueryPart), null);

        #endregion // Properties
    }
}