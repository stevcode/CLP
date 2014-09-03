using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateNoTroubleWithRemaindersTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateNoTroubleWithRemaindersTag" /> from scratch.</summary>
        public DivisionTemplateNoTroubleWithRemaindersTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateNoTroubleWithRemaindersTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateNoTroubleWithRemaindersTag" /> belongs to.</param>
        public DivisionTemplateNoTroubleWithRemaindersTag(CLPPage parentPage,
                                                          Origin origin,
                                                          string divisionTemplateID,
                                                          double dividend,
                                                          double divisor)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor) { }

        /// <summary>Initializes <see cref="DivisionTemplateNoTroubleWithRemaindersTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateNoTroubleWithRemaindersTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "No Trouble With Remainders"; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("No Trouble with {0} / {1}.\n" + "DivisionTemplate {2} on page.",
                                     Dividend,
                                     Divisor,
                                     IsDivisionTemplateStillOnPage ? "still" : "no longer");
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}