using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateDeletedTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="DivisionTemplateDeletedTag" /> from scratch.
        /// </summary>
        public DivisionTemplateDeletedTag() { }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateDeletedTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateDeletedTag" /> belongs to.</param>
        public DivisionTemplateDeletedTag(CLPPage parentPage, Origin origin, int dividend, int givenFactor)
            : base(parentPage, origin)
        {
            Dividend = dividend;
            GivenFactor = givenFactor;
        }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateDeletedTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateDeletedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Dividend of the deleted Division Template.
        /// </summary>
        public int Dividend
        {
            get { return GetValue<int>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int));

        /// <summary>
        /// Given Factor of the deleted Division Template.
        /// </summary>
        public int GivenFactor
        {
            get { return GetValue<int>(GivenFactorProperty); }
            set { SetValue(GivenFactorProperty, value); }
        }

        public static readonly PropertyData GivenFactorProperty = RegisterProperty("GivenFactor", typeof(int));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedValue
        {
            get { return string.Format("Dividend: {0}\n" + "Given Factor: {1}", 
                                       Dividend,
                                       GivenFactor); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}