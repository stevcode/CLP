using System;
using System.Collections.Generic;

namespace CLP.Entities
{
    [Serializable]
    public class BinsStrategyTag : ARepresentationStrategyBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="BinsStrategyTag" /> from scratch.</summary>
        public BinsStrategyTag() { }

        /// <summary>Initializes <see cref="BinsStrategyTag" /> from values.</summary>
        public BinsStrategyTag(CLPPage parentPage, Origin origin, List<ISemanticEvent> semanticEvents, List<CodedRepresentationStrategy> codedStrategies)
            : base(parentPage, origin, semanticEvents, codedStrategies) { }

        #endregion //Constructors

        #region ATagBase Overrides

        public override string FormattedName => "Bins";

        #endregion //ATagBase Overrides
    }
}