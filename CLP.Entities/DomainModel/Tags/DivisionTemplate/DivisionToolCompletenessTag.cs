using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum DivisionToolCompletenessValues
    {
        NoArrays,
        NotEnoughArrays,
        Complete
    }

    [Serializable]
    public class DivisionToolCompletenessTag : ADivisionToolBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionToolCompletenessTag" /> from scratch.</summary>
        public DivisionToolCompletenessTag() { }

        /// <summary>Initializes <see cref="DivisionToolCompletenessTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionToolCompletenessTag" /> belongs to.</param>
        public DivisionToolCompletenessTag(CLPPage parentPage,
                                               Origin origin,
                                               string divisionToolID,
                                               double dividend,
                                               double divisor,
                                               int divisionToolNumber,
                                               DivisionToolCompletenessValues completenessValue)
            : base(parentPage, origin, divisionToolID, dividend, divisor, divisionToolNumber)
        {
            CompletenessValue = completenessValue;
        }

        /// <summary>Initializes <see cref="DivisionToolCompletenessTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionToolCompletenessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Value of the Starred Tag.</summary>
        public DivisionToolCompletenessValues CompletenessValue
        {
            get { return GetValue<DivisionToolCompletenessValues>(CompletenessValueProperty); }
            set { SetValue(CompletenessValueProperty, value); }
        }

        public static readonly PropertyData CompletenessValueProperty = RegisterProperty("CompletenessValue",
                                                                                         typeof (DivisionToolCompletenessValues));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Template {0} Completeness", DivisionToolNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Completeness for {0} / {1}\n" + "DivisionTool {2} on page.\n" + "Value: {3}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionToolStillOnPage ? "still" : "no longer",
                                     CompletenessValue);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}