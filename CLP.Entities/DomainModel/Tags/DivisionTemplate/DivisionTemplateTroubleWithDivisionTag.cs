using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum TroubleWithDivisionReasons
    {
        TroubleWithRemainders,
        TroubleWithDimensions
    }

    [Serializable]
    public class DivisionTemplateTroubleWithDivisionTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithDivisionTag" /> from scratch.</summary>
        public DivisionTemplateTroubleWithDivisionTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithDivisionTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateTroubleWithDivisionTag" /> belongs to.</param>
        public DivisionTemplateTroubleWithDivisionTag(CLPPage parentPage, Origin origin, string divisionTemplateID, double dividend, double divisor)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor) { }

        /// <summary>Initializes <see cref="DivisionTemplateTroubleWithDivisionTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateTroubleWithDivisionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Reasons the TroubleWithDivisionTag was set.
        /// </summary>
        public List<TroubleWithDivisionReasons> Reasons
        {
            get { return GetValue<List<TroubleWithDivisionReasons>>(ReasonsProperty); }
            set
            {
                SetValue(ReasonsProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData ReasonsProperty = RegisterProperty("Reasons", typeof(List<TroubleWithDivisionReasons>), () => new List<TroubleWithDivisionReasons>());

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "Trouble With Division"; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Trouble with {0} / {1}.\n" + "DivisionTemplate {2} on page.\n" + "Reason: {3}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                     string.Join(", ", Reasons));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}