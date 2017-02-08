using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnswerBeforeRepresentationTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> from scratch.</summary>
        public AnswerBeforeRepresentationTag() { }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="AnswerChangedAfterRepresentationTag" /> belongs to.</param>
        public AnswerBeforeRepresentationTag(CLPPage parentPage, Origin origin, List<ISemanticEvent> semanticEvents)
            : base(parentPage, origin)
        {
            SemanticEventIDs = semanticEvents.Select(h => h.ID).ToList();
        }

        #endregion // Constructors

        #region Properties

        /// <summary>List of <see cref="ISemanticEvent" /> IDs used to generate this Tag.</summary>
        public List<string> SemanticEventIDs
        {
            get { return GetValue<List<string>>(SemanticEventIDsProperty); }
            set { SetValue(SemanticEventIDsProperty, value); }
        }

        public static readonly PropertyData SemanticEventIDsProperty = RegisterProperty("SemanticEventIDs", typeof(List<string>), () => new List<string>());

        #region Calculated Properties

        public List<ISemanticEvent> SemanticEvents
        {
            get { return ParentPage.History.SemanticEvents.Where(h => SemanticEventIDs.Contains(h.ID)).OrderBy(h => h.SemanticPassNumber).ThenBy(h => h.SemanticEventIndex).ToList(); }
        }

        public ISemanticEvent Answer
        {
            get { return SemanticEvents.FirstOrDefault(Codings.IsFinalAnswerEvent); }
        }

        #endregion // Calculated Properties

        #endregion // Properties

        #region ATagBase Overrides

        public string AnalysisCode
        {
            get
            {
                var isAnswerCorrect = Codings.GetFinalAnswerEventCorrectness(Answer) == "COR";
                var answerContent = Codings.GetFinalAnswerEventContent(Answer);
                var analysisObjectCode = isAnswerCorrect ? Codings.ANALYSIS_COR_BEFORE_REP : Codings.ANALYSIS_INC_BEFORE_REP;
                return string.Format("{0} [{1}]", analysisObjectCode, answerContent);
            }
        }

        public override Category Category => Category.Answer;

        public override string FormattedName => "Answer Before Representation";

        public override string FormattedValue
        {
            get
            {
                if (Answer == null)
                {
                    return "[ERROR]: Tag generated with incorrect variables.";
                }

                var isAnswerCorrect = Codings.GetFinalAnswerEventCorrectness(Answer) == "COR";
                var analysisObjectCode = isAnswerCorrect ? Codings.ANALYSIS_COR_BEFORE_REP : Codings.ANALYSIS_INC_BEFORE_REP;

                var answerContents = Codings.GetFinalAnswerEventContent(Answer);

                var answerSet = string.Format("{0} {1} ({2})", Codings.FriendlyObjects[Answer.CodedObject], answerContents, isAnswerCorrect ? "correct" : "incorrect");

                var analysisCode = string.IsNullOrWhiteSpace(analysisObjectCode) ? string.Empty : analysisObjectCode;

                var representationsAdded =
                    SemanticEvents.Where(h => Codings.IsRepresentationEvent(h) && h.EventType == Codings.EVENT_OBJECT_ADD && Codings.FriendlyObjects.ContainsKey(h.CodedObject))
                                  .Select(h => string.Format("{0} [{1}]", h.CodedObject, h.CodedObjectID));

                return string.Format("{0}\nRepresentations: {1}{2}",
                                     answerSet,
                                     string.Join(", ", representationsAdded),
                                     string.IsNullOrWhiteSpace(analysisCode) ? string.Empty : "\nCode: " + analysisCode);
            }
        }

        #endregion //ATagBase Overrides
    }
}