using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class TroubleWithDivisionTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="TroubleWithDivisionTag" /> from scratch.</summary>
        public TroubleWithDivisionTag() { }

        /// <summary>Initializes <see cref="TroubleWithDivisionTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TroubleWithDivisionTag" /> belongs to.</param>
        public TroubleWithDivisionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>Initializes <see cref="TroubleWithDivisionTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public TroubleWithDivisionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedName
        {
            get { return "Trouble With Division"; }
        }

        public override string FormattedValue
        {
            get
            {
                return
                    string.Format(
                                  "Trouble with Array Dimensions {0} time(s).\n" + "{1} were deleted.\n" + "Trouble with Remainders {2} time(s).\n" +
                                  "{3} were deleted.\n" + "Trouble with Division Template Creation {4} time(s).\n" + "{5} were deleted.",
                                  GetTroubleWithArrayDimensionsCount(ParentPage),
                                  GetTroubleWithArrayDimensionsCount(ParentPage, true),
                                  GetTroubleWithRemaindersCount(ParentPage),
                                  GetTroubleWithRemaindersCount(ParentPage, true),
                                  GetTroubleWithDivisionTemplateCreationCount(ParentPage),
                                  GetTroubleWithDivisionTemplateCreationCount(ParentPage, true));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties

        #region Static Methods

        public static int GetTroubleWithArrayDimensionsCount(CLPPage page, bool returnOnlyDeleted = false)
        {
            var totalCount =
                page.Tags.Count(tag => tag is DivisionTemplateArrayDimensionErrorsTag && (tag as DivisionTemplateArrayDimensionErrorsTag).HadTrouble);

            var deletedCount =
                page.Tags.Count(
                                tag =>
                                tag is DivisionTemplateArrayDimensionErrorsTag && (tag as DivisionTemplateArrayDimensionErrorsTag).HadTrouble &&
                                !(tag as DivisionTemplateArrayDimensionErrorsTag).IsDivisionTemplateStillOnPage);

            return returnOnlyDeleted ? deletedCount : totalCount;
        }

        public static int GetTroubleWithRemaindersCount(CLPPage page, bool returnOnlyDeleted = false)
        {
            var totalCount =
                page.Tags.Count(tag => tag is DivisionTemplateRemainderErrorsTag && (tag as DivisionTemplateRemainderErrorsTag).HadTrouble);

            var deletedCount =
                page.Tags.Count(
                                tag =>
                                tag is DivisionTemplateRemainderErrorsTag && (tag as DivisionTemplateRemainderErrorsTag).HadTrouble &&
                                !(tag as DivisionTemplateRemainderErrorsTag).IsDivisionTemplateStillOnPage);

            return returnOnlyDeleted ? deletedCount : totalCount;
        }

        public static int GetTroubleWithDivisionTemplateCreationCount(CLPPage page, bool returnOnlyDeleted = false)
        {
            var totalCount = page.Tags.Count(tag => tag is DivisionTemplateCreationErrorTag);

            var deletedCount =
                page.Tags.Count(
                                tag =>
                                tag is DivisionTemplateCreationErrorTag && !(tag as DivisionTemplateCreationErrorTag).IsDivisionTemplateStillOnPage);

            return returnOnlyDeleted ? deletedCount : totalCount;
        }

        #endregion //Static Methods
    }
}