using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineJumpEraseTag : ATagBase
    {
        // HACK: Very hacky and incomplete
        #region Constructors

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> from scratch.</summary>
        public NumberLineJumpEraseTag() { }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="AnswerChangedAfterRepresentationTag" /> belongs to.</param>
        public NumberLineJumpEraseTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin)
        {
            //HistoryActionIDs = historyActions.Select(h => h.ID).ToList();
        }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public NumberLineJumpEraseTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion // Constructors

        #region Properties

        /// <summary>List of <see cref="IHistoryAction" /> IDs used to generate this Tag.</summary>
        public List<string> HistoryActionIDs
        {
            get { return GetValue<List<string>>(HistoryActionIDsProperty); }
            set { SetValue(HistoryActionIDsProperty, value); }
        }

        public static readonly PropertyData HistoryActionIDsProperty = RegisterProperty("HistoryActionIDs", typeof (List<string>), () => new List<string>());

        #region Calculated Properties

        public List<IHistoryAction> HistoryActions
        {
            get { return ParentPage.History.HistoryActions.Where(h => HistoryActionIDs.Contains(h.ID)).OrderBy(h => h.HistoryActionIndex).Distinct().ToList(); }
        }

        #endregion // Calculated Properties

        #endregion // Properties

        #region ATagBase Overrides

        public string AnalysisCode
        {
            get { return "NLJE"; }
        }

        public override Category Category
        {
            get { return Category.NumberLine; }
        }

        public override string FormattedName
        {
            get { return "Number Line Jump Erase"; }
        }

        public override string FormattedValue
        {
            get { return "1 jump of 7 erased from 48 to 55\nCode: NLJE"; }
        }

        #endregion //ATagBase Overrides
    }
}