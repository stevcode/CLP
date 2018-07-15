using System;
using Catel.Data;

namespace CLP.Entities
{
    public enum DivisionTemplateCompletenessValues
    {
        NoArrays,
        NotEnoughArrays,
        Complete
    }

    [Serializable]
    public class DivisionTemplateCompletenessTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateCompletenessTag" /> from scratch.</summary>
        public DivisionTemplateCompletenessTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateCompletenessTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateCompletenessTag" /> belongs to.</param>
        public DivisionTemplateCompletenessTag(CLPPage parentPage,
                                               Origin origin,
                                               string divisionTemplateID,
                                               double dividend,
                                               double divisor,
                                               int divisionTemplateNumber,
                                               DivisionTemplateCompletenessValues completenessValue)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor, divisionTemplateNumber)
        {
            CompletenessValue = completenessValue;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Value of the Starred Tag.</summary>
        public DivisionTemplateCompletenessValues CompletenessValue
        {
            get { return GetValue<DivisionTemplateCompletenessValues>(CompletenessValueProperty); }
            set { SetValue(CompletenessValueProperty, value); }
        }

        public static readonly PropertyData CompletenessValueProperty = RegisterProperty("CompletenessValue", typeof(DivisionTemplateCompletenessValues), DivisionTemplateCompletenessValues.NoArrays);

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Template {0} Completeness", DivisionTemplateNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Completeness for {0} / {1}\n" + "DivisionTemplate {2} on page.\n" + "Value: {3}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                     CompletenessValue);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}