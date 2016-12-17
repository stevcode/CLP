using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public abstract class ADivisionTemplateBaseTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ADivisionTemplateBaseTag" /> from scratch.</summary>
        protected ADivisionTemplateBaseTag() { }

        /// <summary>Initializes <see cref="ADivisionTemplateBaseTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ADivisionTemplateBaseTag" /> belongs to.</param>
        protected ADivisionTemplateBaseTag(CLPPage parentPage, Origin origin, string divisionTemplateID, double dividend, double divisor, int divisionTemplateNumber)
            : base(parentPage, origin)
        {
            DivisionTemplateID = divisionTemplateID;
            Dividend = dividend;
            Divisor = divisor;
            DivisionTemplateNumber = divisionTemplateNumber + 1;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>ID of the Division Template against which this tag compares.</summary>
        public string DivisionTemplateID
        {
            get { return GetValue<string>(DivisionTemplateIDProperty); }
            set { SetValue(DivisionTemplateIDProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIDProperty = RegisterProperty("DivisionTemplateID", typeof(string), string.Empty);

        /// <summary>Dividend of the DivisionTemplate being compared against.</summary>
        public double Dividend
        {
            get { return GetValue<double>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(double), 0);

        /// <summary>Divisor of the DivisionTemplate being compared against.</summary>
        public double Divisor
        {
            get { return GetValue<double>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof(double), 0);

        /// <summary>Order in which the associated Division Template occured in the history.</summary>
        public int DivisionTemplateNumber
        {
            get { return GetValue<int>(DivisionTemplateNumberProperty); }
            set { SetValue(DivisionTemplateNumberProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateNumberProperty = RegisterProperty("DivisionTemplateNumber", typeof(int), 0);

        /// <summary>Determines if the DivisionTemplate this tag applies to is still on the Parent Page or if it has been deleted from the page.</summary>
        public bool IsDivisionTemplateStillOnPage
        {
            get { return ParentPage.GetPageObjectByID(DivisionTemplateID) != null; }
        }

        #region ATagBase Overrides

        public override Category Category => Category.DivisionTemplate;

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}