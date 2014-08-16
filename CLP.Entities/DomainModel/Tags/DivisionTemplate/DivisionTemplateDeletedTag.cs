using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateDeletedTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateDeletedTag" /> from scratch.</summary>
        public DivisionTemplateDeletedTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateDeletedTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateDeletedTag" /> belongs to.</param>
        public DivisionTemplateDeletedTag(CLPPage parentPage, Origin origin, string divisionTemplateID, int dividend, int divisor, List<string> arrayDimensions)
            : base(parentPage, origin)
        {
            DivisionTemplateID = divisionTemplateID;
            Dividend = dividend;
            Divisor = divisor;
            ArrayDimensions = arrayDimensions;
        }

        /// <summary>Initializes <see cref="DivisionTemplateDeletedTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateDeletedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// ID of the deleted Division Template.
        /// </summary>
        public string DivisionTemplateID
        {
            get { return GetValue<string>(DivisionTemplateIDProperty); }
            set { SetValue(DivisionTemplateIDProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIDProperty = RegisterProperty("DivisionTemplateID", typeof (string));

        /// <summary>Dividend of the deleted Division Template.</summary>
        public int Dividend
        {
            get { return GetValue<int>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof (int));

        /// <summary>Divisor of the division relation.</summary>
        public double Divisor
        {
            get { return GetValue<double>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof (double));

        /// <summary>Dimensions of all the snapped-in arrays.</summary>
        public List<string> ArrayDimensions
        {
            get { return GetValue<List<string>>(ArrayDimensionsProperty); }
            set { SetValue(ArrayDimensionsProperty, value); }
        }

        public static readonly PropertyData ArrayDimensionsProperty = RegisterProperty("ArrayDimensions", typeof (List<string>));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedValue
        {
            get { return string.Format("Dividend: {0}\n" + "Divisor: {1}\n" + "Snapped-In Arrays: {2}", Dividend, Divisor, string.Join(",", ArrayDimensions)); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}