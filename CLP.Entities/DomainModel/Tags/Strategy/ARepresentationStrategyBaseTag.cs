using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public abstract class ARepresentationStrategyBaseTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ARepresentationStrategyBaseTag" /> from scratch.</summary>
        protected ARepresentationStrategyBaseTag() { }

        /// <summary>Initializes <see cref="ARepresentationStrategyBaseTag" /> from values.</summary>
        protected ARepresentationStrategyBaseTag(CLPPage parentPage, Origin origin, List<ISemanticEvent> semanticEvents, List<CodedRepresentationStrategy> codedStrategies)
            : base(parentPage, origin)
        {
            CodedStrategies = codedStrategies;
            SemanticEvents = semanticEvents;
        }

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
        public List<ISemanticEvent> SemanticEvents
        {
            get { return GetValue<List<ISemanticEvent>>(SemanticEventsProperty); }
            set { SetValue(SemanticEventsProperty, value); }
        }

        public static readonly PropertyData SemanticEventsProperty = RegisterProperty("SemanticEvents", typeof(List<ISemanticEvent>), () => new List<ISemanticEvent>());

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Strategy;

        public override string FormattedValue
        {
            get { return string.Join("\n", CodedStrategies.Select(s => s.CodedValue)); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}