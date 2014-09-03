using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateNoTroubleWithDimensionsTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateNoTroubleWithDimensionsTag" /> from scratch.</summary>
        public DivisionTemplateNoTroubleWithDimensionsTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateNoTroubleWithDimensionsTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateNoTroubleWithDimensionsTag" /> belongs to.</param>
        public DivisionTemplateNoTroubleWithDimensionsTag(CLPPage parentPage,
                                                          Origin origin,
                                                          string divisionTemplateID,
                                                          double dividend,
                                                          double divisor)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor) { }

        /// <summary>Initializes <see cref="DivisionTemplateNoTroubleWithDimensionsTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateNoTroubleWithDimensionsTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "No Trouble With Dimensions"; }
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