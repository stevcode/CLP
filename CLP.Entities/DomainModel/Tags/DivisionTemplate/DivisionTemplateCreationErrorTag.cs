using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
{
    public enum DivisionTemplateIncorrectCreationReasons
    {
        WrongDividendAndDivisor,
        WrongDividend,
        WrongDivisor,
        SwappedDividendAndDivisor
    }

    [Serializable]
    public class DivisionTemplateCreationErrorTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateCreationErrorTag" /> from scratch.</summary>
        public DivisionTemplateCreationErrorTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateCreationErrorTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateCreationErrorTag" /> belongs to.</param>
        public DivisionTemplateCreationErrorTag(CLPPage parentPage,
                                                Origin origin,
                                                string divisionTemplateID,
                                                double dividend,
                                                double divisor,
                                                int divisionTemplateNumber,
                                                DivisionTemplateIncorrectCreationReasons reason)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor, divisionTemplateNumber)
        {
            Reason = reason;
        }

        /// <summary>Initializes <see cref="DivisionTemplateCreationErrorTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateCreationErrorTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Reason the Division Template creation is wrong.</summary>
        public DivisionTemplateIncorrectCreationReasons Reason
        {
            get { return GetValue<DivisionTemplateIncorrectCreationReasons>(ReasonProperty); }
            set { SetValue(ReasonProperty, value); }
        }

        public static readonly PropertyData ReasonProperty = RegisterProperty("Reason", typeof (DivisionTemplateIncorrectCreationReasons));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Template {0} Creation Error", DivisionTemplateNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0} / {1} Created.\n" + "DivisionTemplate {2} on page.\n" + "Reason: {3}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                     Reason);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}