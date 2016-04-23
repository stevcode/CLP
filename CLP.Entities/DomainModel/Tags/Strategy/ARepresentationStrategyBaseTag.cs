using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public abstract class ARepresentationStrategyBaseTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ARepresentationStrategyBaseTag" /> from scratch.</summary>
        public ARepresentationStrategyBaseTag() { }

        /// <summary>Initializes <see cref="ARepresentationStrategyBaseTag" /> from values.</summary>
        public ARepresentationStrategyBaseTag(CLPPage parentPage, Origin origin, List<IHistoryAction> historyActions, List<CodedRepresentationStrategy> codedStrategies)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            CodedStrategies = codedStrategies;
            HistoryActions = historyActions;
        }

        /// <summary>Initializes <see cref="ARepresentationStrategyBaseTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ARepresentationStrategyBaseTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of all the Representation Strategies used on the page.</summary>
        public List<CodedRepresentationStrategy> CodedStrategies
        {
            get { return GetValue<List<CodedRepresentationStrategy>>(CodedStrategiesProperty); }
            set { SetValue(CodedStrategiesProperty, value); }
        }

        public static readonly PropertyData CodedStrategiesProperty = RegisterProperty("CodedStrategies", typeof(List<CodedRepresentationStrategy>), () => new List<CodedRepresentationStrategy>());

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
            get { return Category.Strategy; }
        }

        public override string FormattedValue
        {
            get { return string.Join("\n", CodedStrategies.Select(s => s.CodedValue)); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}