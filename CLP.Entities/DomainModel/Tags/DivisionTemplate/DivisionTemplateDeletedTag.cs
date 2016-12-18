using System;
using System.Collections.Generic;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateDeletedTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateDeletedTag" /> from scratch.</summary>
        public DivisionTemplateDeletedTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateDeletedTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateDeletedTag" /> belongs to.</param>
        public DivisionTemplateDeletedTag(CLPPage parentPage, Origin origin, string divisionTemplateID, int dividend, int divisor, int divisionTemplateNumber, List<string> arrayDimensions)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor, divisionTemplateNumber)
        {
            ArrayDimensions = arrayDimensions;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Dimensions of all the snapped-in arrays.</summary>
        public List<string> ArrayDimensions
        {
            get { return GetValue<List<string>>(ArrayDimensionsProperty); }
            set { SetValue(ArrayDimensionsProperty, value); }
        }

        public static readonly PropertyData ArrayDimensionsProperty = RegisterProperty("ArrayDimensions", typeof(List<string>), () => new List<string>());

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Template {0} Deleted", DivisionTemplateNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0} / {1} Deleted.\n" + "DivisionTemplate {2} on page.\n" + "Snapped-In Arrays: {3}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                     string.Join(",", ArrayDimensions));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}