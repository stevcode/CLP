using System;
using System.Collections.Generic;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineStrategyTag : ARepresentationStrategyBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="NumberLineStrategyTag" /> from scratch.</summary>
        public NumberLineStrategyTag() { }

        /// <summary>Initializes <see cref="NumberLineStrategyTag" /> from values.</summary>
        public NumberLineStrategyTag(CLPPage parentPage, Origin origin, List<IHistoryAction> historyActions, List<CodedRepresentationStrategy> codedStrategies)
            : base(parentPage, origin, historyActions, codedStrategies) { }

        #endregion //Constructors

        #region ATagBase Overrides

        public override string FormattedName => "Number Line";

        #endregion //ATagBase Overrides
    }
}