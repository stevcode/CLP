using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTroublesErasedTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="DivisionTroublesErasedTag" /> from scratch.
        /// </summary>
        public DivisionTroublesErasedTag() { }

        /// <summary>
        /// Initializes <see cref="DivisionTroublesErasedTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTroublesErasedTag" /> belongs to.</param>
        public DivisionTroublesErasedTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="DivisionTroublesErasedTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTroublesErasedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region ATagBase Overrides

        public override Category Category
        {
            //HACK: Not necessarily related to DivisionTemplate. Likely need a new category.
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedName
        {
            get { return "Division Troubles Erased"; }
        }

        public override string FormattedValue
        {
            get { return string.Format("Trouble With Division Removed: {0}\n" + "Trouble With Division on Page: {1}\n" + "No Trouble With Division Removed: {2}\n" +
                                       "No Trouble With Division on Page: {3}", 
                                       ParentPage.Tags.Count(tag => tag is DivisionTemplateTroubleWithDivisionTag && !(tag as DivisionTemplateTroubleWithDivisionTag).IsDivisionTemplateStillOnPage),
                                       ParentPage.Tags.Count(tag => tag is DivisionTemplateTroubleWithDivisionTag && (tag as DivisionTemplateTroubleWithDivisionTag).IsDivisionTemplateStillOnPage),
                                       ParentPage.Tags.Count(tag => tag is DivisionTemplateNoTroubleWithDivisionTag && !(tag as DivisionTemplateNoTroubleWithDivisionTag).IsDivisionTemplateStillOnPage),
                                       ParentPage.Tags.Count(tag => tag is DivisionTemplateNoTroubleWithDivisionTag && (tag as DivisionTemplateNoTroubleWithDivisionTag).IsDivisionTemplateStillOnPage));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}