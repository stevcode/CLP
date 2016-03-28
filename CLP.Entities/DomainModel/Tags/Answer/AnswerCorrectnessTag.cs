using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnswerCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> from scratch.</summary>
        public AnswerCorrectnessTag()
        { }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="AnswerChangedAfterRepresentationTag" /> belongs to.</param>
        public AnswerCorrectnessTag(CLPPage parentPage, Origin origin, List<IHistoryAction> historyActions)
            : base(parentPage, origin)
        {
            HistoryActionIDs = historyActions.Select(h => h.ID).ToList();
        }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public AnswerCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion // Constructors

        #region Properties

        /// <summary>List of <see cref="IHistoryAction" /> IDs used to generate this Tag.</summary>
        public List<string> HistoryActionIDs
        {
            get { return GetValue<List<string>>(HistoryActionIDsProperty); }
            set { SetValue(HistoryActionIDsProperty, value); }
        }

        public static readonly PropertyData HistoryActionIDsProperty = RegisterProperty("HistoryActionIDs", typeof(List<string>), () => new List<string>());

        #region Calculated Properties

        public List<IHistoryAction> HistoryActions
        {
            get { return ParentPage.History.HistoryActions.Where(h => HistoryActionIDs.Contains(h.ID)).OrderBy(h => h.HistoryActionIndex).Distinct().ToList(); }
        }

        public IHistoryAction Answer
        {
            get { return HistoryActions.FirstOrDefault(Codings.IsAnswerObject); }
        }

        #endregion // Calculated Properties

        #endregion // Properties

        #region ATagBase Overrides

        public string AnalysisCode
        {
            get
            {
                return Answer.CodedValue;
            }
        }

        public override Category Category
        {
            get { return Category.Answer; }
        }

        public override string FormattedName
        {
            get { return "Answer Correctness"; }
        }

        public override string FormattedValue
        {
            get
            {
                if (Answer == null)
                {
                    return "[ERROR]: Tag generated with incorrect variables.";
                }

                var isAnswerCorrect = Codings.GetAnswerObjectCorrectness(Answer) == "COR";
                var answerContents = Codings.GetAnswerObjectContent(Answer);
                var correctAnswer = Answer.CodedObjectID;
                var answerAction = Answer.CodedObject == Codings.OBJECT_MULTIPLE_CHOICE ? "filled in" : "inked";

                var answerSet = string.Format("{0}: correct answer is {1}\nStudent {2} {3} ({4})",
                                              Codings.FriendlyObjects[Answer.CodedObject],
                                              correctAnswer,
                                              answerAction,
                                              answerContents,
                                              isAnswerCorrect ? "correct" : "incorrect");

                return string.Format("{0}{1}",
                                     answerSet,
                                     string.IsNullOrWhiteSpace(AnalysisCode) ? string.Empty : "\nCode: " + AnalysisCode);
            }
        }

        #endregion //ATagBase Overrides
    }
}
