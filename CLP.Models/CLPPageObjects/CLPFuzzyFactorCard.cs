using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPFuzzyFactorCard : CLPFactorCard
    {

        #region Constructors

        public CLPFuzzyFactorCard(int rows, int columns, int dividend, ICLPPage page)
            : base(rows, columns, page)
        {
            Dividend = dividend;
        }
        
        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPFuzzyFactorCard(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties
        public override string PageObjectType
        {
            get { return "CLPFuzzyFactorCard"; }
        }

        /// <summary>
        /// Value of the dividend.
        /// </summary>
        public int Dividend
        {
            get
            {
                return GetValue<int>(DividendProperty);
            }
            set
            {
                SetValue(DividendProperty, value);
            }
        }

        /// <summary>
        /// Register the Dividend property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int), null);

        #endregion //Properties
    }
}
