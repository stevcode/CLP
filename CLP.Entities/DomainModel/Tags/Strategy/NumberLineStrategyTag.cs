using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CLP.Entities.Demo
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

        /// <summary>Initializes <see cref="NumberLineStrategyTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public NumberLineStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "Number Line"; }
        }

        #endregion //ATagBase Overrides
    }
}
