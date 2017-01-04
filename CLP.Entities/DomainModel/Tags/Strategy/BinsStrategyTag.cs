using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CLP.Entities.Demo
{
    [Serializable]
    public class BinsStrategyTag : ARepresentationStrategyBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="BinsStrategyTag" /> from scratch.</summary>
        public BinsStrategyTag() { }

        /// <summary>Initializes <see cref="BinsStrategyTag" /> from values.</summary>
        public BinsStrategyTag(CLPPage parentPage, Origin origin, List<IHistoryAction> historyActions, List<CodedRepresentationStrategy> codedStrategies)
            : base(parentPage, origin, historyActions, codedStrategies) { }

        /// <summary>Initializes <see cref="BinsStrategyTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public BinsStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "Bins"; }
        }

        #endregion //ATagBase Overrides
    }
}
