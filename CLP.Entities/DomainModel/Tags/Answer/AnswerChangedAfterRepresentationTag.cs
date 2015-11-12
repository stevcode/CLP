using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class AnswerChangedAfterRepresentationTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> from scratch.</summary>
        public AnswerChangedAfterRepresentationTag() { }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="AnswerChangedAfterRepresentationTag" /> belongs to.</param>
        public AnswerChangedAfterRepresentationTag(CLPPage parentPage, Origin origin, List<IHistoryAction> historyActions)
            : base(parentPage, origin)
        {
            HistoryActionIDs = historyActions.Select(h => h.ID).ToList();
        }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public AnswerChangedAfterRepresentationTag(SerializationInfo info, StreamingContext context)
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

        public IHistoryAction FirstAnswer
        {
            get { return HistoryActions.FirstOrDefault(Codings.IsAnswerObject); }
        }

        public IHistoryAction LastAnswer
        {
            get { return HistoryActions.LastOrDefault(Codings.IsAnswerObject); }
        }

        #endregion // Calculated Properties

        #endregion // Properties

        #region ATagBase Overrides

        public string AnalysisCode
        {
            get
            {
                var firstAnswer = FirstAnswer;
                var lastAnswer = LastAnswer;
                if (firstAnswer.ID == lastAnswer.ID)
                {
                    return "[ERROR]: Tag generated with incorrect variables.";
                }

                var isFirstAnswerCorrect = Codings.GetAnswerObjectCorrectness(firstAnswer) == "COR";
                var isLastAnswerCorrect = Codings.GetAnswerObjectCorrectness(lastAnswer) == "COR";
                var analysisObjectCode = string.Empty;
                if (isFirstAnswerCorrect)
                {
                    analysisObjectCode = isLastAnswerCorrect ? Codings.ANALYSIS_COR_TO_COR_AFTER_REP : Codings.ANALYSIS_COR_TO_INC_AFTER_REP;
                }
                else
                {
                    analysisObjectCode = isLastAnswerCorrect ? Codings.ANALYSIS_INC_TO_COR_AFTER_REP : Codings.ANALYSIS_INC_TO_INC_AFTER_REP;
                }

                return analysisObjectCode;
            }
        }

        public override Category Category
        {
            get { return Category.Answer; }
        }

        public override string FormattedName
        {
            get { return "Answer Changed After Representation"; }
        }

        public override string FormattedValue
        {
            get
            {
                var firstAnswer = FirstAnswer;
                var lastAnswer = LastAnswer;
                if (firstAnswer.ID == lastAnswer.ID)
                {
                    return "[ERROR]: Tag generated with incorrect variables.";
                }

                var isFirstAnswerCorrect = Codings.GetAnswerObjectCorrectness(firstAnswer) == "COR";
                var isLastAnswerCorrect = Codings.GetAnswerObjectCorrectness(lastAnswer) == "COR";
                var analysisObjectCode = string.Empty;
                if (isFirstAnswerCorrect)
                {
                    analysisObjectCode = isLastAnswerCorrect ? Codings.ANALYSIS_COR_TO_COR_AFTER_REP : Codings.ANALYSIS_COR_TO_INC_AFTER_REP;
                }
                else
                {
                    analysisObjectCode = isLastAnswerCorrect ? Codings.ANALYSIS_INC_TO_COR_AFTER_REP : Codings.ANALYSIS_INC_TO_INC_AFTER_REP;
                }

                var firstAnswerContents = Codings.GetAnswerObjectContent(firstAnswer);
                var lastAnswerContents = Codings.GetAnswerObjectContent(lastAnswer);

                var answerChanged = string.Format("{0} {1} ({2}) changed to {3} ({4})",
                                                  Codings.FriendlyObjects[firstAnswer.CodedObject],
                                                  firstAnswerContents,
                                                  isFirstAnswerCorrect ? "correct" : "incorrect",
                                                  lastAnswerContents,
                                                  isLastAnswerCorrect ? "correct" : "incorrect");

                var analysisCode = string.IsNullOrWhiteSpace(analysisObjectCode) ? string.Empty : string.Format("{0} [{1}, {2}]", analysisObjectCode, firstAnswerContents, lastAnswerContents);

                var representationsAdded =
                    HistoryActions.Where(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD && Codings.FriendlyObjects.ContainsKey(h.CodedObject))
                                  .Select(h => Codings.FriendlyObjects[h.CodedObject]);

                return string.Format("{0}\nRepresentations:\n{1}{2}",
                                     answerChanged,
                                     string.Join("\n", representationsAdded),
                                     string.IsNullOrWhiteSpace(analysisCode) ? string.Empty : "\nAnalysis Code: " + analysisCode);
            }
        }

        #endregion //ATagBase Overrides
    }
}