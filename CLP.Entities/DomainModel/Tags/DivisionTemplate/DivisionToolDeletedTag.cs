using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionToolDeletedTag : ADivisionToolBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionToolDeletedTag" /> from scratch.</summary>
        public DivisionToolDeletedTag() { }

        /// <summary>Initializes <see cref="DivisionToolDeletedTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionToolDeletedTag" /> belongs to.</param>
        public DivisionToolDeletedTag(CLPPage parentPage,
                                          Origin origin,
                                          string divisionToolID,
                                          int dividend,
                                          int divisor,
                                          int divisionToolNumber,
                                          List<string> arrayDimensions
                                          )
            : base(parentPage, origin, divisionToolID, dividend, divisor, divisionToolNumber)
        {
            ArrayDimensions = arrayDimensions;
        }

        /// <summary>Initializes <see cref="DivisionToolDeletedTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionToolDeletedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Dimensions of all the snapped-in arrays.</summary>
        public List<string> ArrayDimensions
        {
            get { return GetValue<List<string>>(ArrayDimensionsProperty); }
            set { SetValue(ArrayDimensionsProperty, value); }
        }

        public static readonly PropertyData ArrayDimensionsProperty = RegisterProperty("ArrayDimensions", typeof (List<string>));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Template {0} Deleted", DivisionToolNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0} / {1} Deleted.\n" + "DivisionTool {2} on page.\n" + "Snapped-In Arrays: {3}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionToolStillOnPage ? "still" : "no longer",
                                     string.Join(",", ArrayDimensions));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}