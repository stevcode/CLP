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
            : base(parentPage, origin) { IsSingleValueTag = true; }

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
            get { return Category.DivisionTool; }
        }

        public override string FormattedName
        {
            get { return "Trouble With Division"; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0}{1}{2}{3}{4}{5}",
                                     GetTroubleWithFactorPairsCount(ParentPage) == 0
                                         ? string.Empty
                                         : string.Format("Trouble with Factor Pairs {0} time(s).\n",
                                                         GetTroubleWithFactorPairsCount(ParentPage)),
                                     GetTroubleWithFactorPairsCount(ParentPage, true) == 0
                                         ? string.Empty
                                         : string.Format("{0} deleted.\n", GetTroubleWithFactorPairsCount(ParentPage, true)),
                                     GetTroubleWithRemaindersCount(ParentPage) == 0
                                         ? string.Empty
                                         : string.Format("Trouble with Remainders {0} time(s).\n", GetTroubleWithRemaindersCount(ParentPage)),
                                     GetTroubleWithRemaindersCount(ParentPage, true) == 0
                                         ? string.Empty
                                         : string.Format("{0} deleted.\n", GetTroubleWithRemaindersCount(ParentPage, true)),
                                     GetTroubleWithDivisionToolCreationCount(ParentPage) == 0
                                         ? string.Empty
                                         : string.Format("Trouble with Division Template Creation {0} time(s).\n",
                                                         GetTroubleWithDivisionToolCreationCount(ParentPage)),
                                     GetTroubleWithDivisionToolCreationCount(ParentPage, true) == 0
                                         ? string.Empty
                                         : string.Format("{0} deleted.\n", GetTroubleWithDivisionToolCreationCount(ParentPage, true))).TrimEnd('\n');
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties

        #region Static Methods

        public static int GetTroubleWithFactorPairsCount(CLPPage page, bool returnOnlyDeleted = false)
        {
            var totalCount =
                page.Tags.Count(tag => tag is DivisionToolFactorPairErrorsTag && (tag as DivisionToolFactorPairErrorsTag).HadTrouble);

            var deletedCount =
                page.Tags.Count(
                                tag =>
                                tag is DivisionToolFactorPairErrorsTag && (tag as DivisionToolFactorPairErrorsTag).HadTrouble &&
                                !(tag as DivisionToolFactorPairErrorsTag).IsDivisionToolStillOnPage);

            return returnOnlyDeleted ? deletedCount : totalCount;
        }

        public static int GetTroubleWithRemaindersCount(CLPPage page, bool returnOnlyDeleted = false)
        {
            var totalCount =
                page.Tags.Count(tag => tag is DivisionToolRemainderErrorsTag && (tag as DivisionToolRemainderErrorsTag).HadTrouble);

            var deletedCount =
                page.Tags.Count(
                                tag =>
                                tag is DivisionToolRemainderErrorsTag && (tag as DivisionToolRemainderErrorsTag).HadTrouble &&
                                !(tag as DivisionToolRemainderErrorsTag).IsDivisionToolStillOnPage);

            return returnOnlyDeleted ? deletedCount : totalCount;
        }

        public static int GetTroubleWithDivisionToolCreationCount(CLPPage page, bool returnOnlyDeleted = false)
        {
            var totalCount = page.Tags.Count(tag => tag is DivisionToolCreationErrorTag);

            var deletedCount =
                page.Tags.Count(
                                tag =>
                                tag is DivisionToolCreationErrorTag && !(tag as DivisionToolCreationErrorTag).IsDivisionToolStillOnPage);

            return returnOnlyDeleted ? deletedCount : totalCount;
        }

        #endregion //Static Methods
    }
}