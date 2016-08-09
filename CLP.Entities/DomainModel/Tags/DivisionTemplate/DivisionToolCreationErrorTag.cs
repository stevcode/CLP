using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum DivisionToolIncorrectCreationReasons
    {
        WrongDividendAndDivisor,
        WrongDividend,
        WrongDivisor,
        SwappedDividendAndDivisor
    }

    [Serializable]
    public class DivisionToolCreationErrorTag : ADivisionToolBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionToolCreationErrorTag" /> from scratch.</summary>
        public DivisionToolCreationErrorTag() { }

        /// <summary>Initializes <see cref="DivisionToolCreationErrorTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionToolCreationErrorTag" /> belongs to.</param>
        public DivisionToolCreationErrorTag(CLPPage parentPage,
                                                Origin origin,
                                                string divisionToolID,
                                                double dividend,
                                                double divisor,
                                                int divisionToolNumber,
                                                DivisionToolIncorrectCreationReasons reason)
            : base(parentPage, origin, divisionToolID, dividend, divisor, divisionToolNumber)
        {
            Reason = reason;
        }

        /// <summary>Initializes <see cref="DivisionToolCreationErrorTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionToolCreationErrorTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Reason the Division Template creation is wrong.</summary>
        public DivisionToolIncorrectCreationReasons Reason
        {
            get { return GetValue<DivisionToolIncorrectCreationReasons>(ReasonProperty); }
            set { SetValue(ReasonProperty, value); }
        }

        public static readonly PropertyData ReasonProperty = RegisterProperty("Reason", typeof (DivisionToolIncorrectCreationReasons));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Template {0} Creation Error", DivisionToolNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0} / {1} Created.\n" + "DivisionTool {2} on page.\n" + "Reason: {3}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionToolStillOnPage ? "still" : "no longer",
                                     Reason);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}