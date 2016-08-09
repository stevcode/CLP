using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public abstract class ADivisionToolBaseTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ADivisionToolBaseTag" /> from scratch.</summary>
        public ADivisionToolBaseTag() { }

        /// <summary>Initializes <see cref="ADivisionToolBaseTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ADivisionToolBaseTag" /> belongs to.</param>
        public ADivisionToolBaseTag(CLPPage parentPage, Origin origin, string divisionToolID, double dividend, double divisor, int divisionToolNumber)
            : base(parentPage, origin)
        {
            DivisionToolID = divisionToolID;
            Dividend = dividend;
            Divisor = divisor;
            DivisionToolNumber = divisionToolNumber + 1;
        }

        /// <summary>Initializes <see cref="ADivisionToolBaseTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ADivisionToolBaseTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>ID of the Division Tool against which this tag compares.</summary>
        public string DivisionToolID
        {
            get { return GetValue<string>(DivisionToolIDProperty); }
            set { SetValue(DivisionToolIDProperty, value); }
        }

        public static readonly PropertyData DivisionToolIDProperty = RegisterProperty("DivisionToolID", typeof (string), string.Empty);

        /// <summary>Dividend of the DivisionTool being compared against.</summary>
        public double Dividend
        {
            get { return GetValue<double>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof (double));

        /// <summary>Divisor of the DivisionTool being compared against.</summary>
        public double Divisor
        {
            get { return GetValue<double>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof (double));

        /// <summary>Order in which the associated Division Template occured in the history.</summary>
        public int DivisionToolNumber
        {
            get { return GetValue<int>(DivisionToolNumberProperty); }
            set { SetValue(DivisionToolNumberProperty, value); }
        }

        public static readonly PropertyData DivisionToolNumberProperty = RegisterProperty("DivisionToolNumber", typeof (int), 0);

        /// <summary>Determines if the DivisionTool this tag applies to is still on the Parent Page or if it has been deleted from the page.</summary>
        public bool IsDivisionToolStillOnPage
        {
            get { return ParentPage.GetPageObjectByID(DivisionToolID) != null; }
        }

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTool; }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}