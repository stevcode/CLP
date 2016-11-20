using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnswerChangedAfterRepresentationTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> from scratch.</summary>
        public AnswerChangedAfterRepresentationTag() { }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="AnswerChangedAfterRepresentationTag" /> belongs to.</param>
        public AnswerChangedAfterRepresentationTag(CLPPage parentPage, Origin origin, List<ISemanticEvent> semanticEvents)
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
            get { return ParentPage.History.SemanticEvents.Where(h => SemanticEventIDs.Contains(h.ID)).OrderBy(h => h.SemanticEventIndex).Distinct().ToList(); }
        }

        public ISemanticEvent FirstAnswer
        {
            get { return SemanticEvents.FirstOrDefault(Codings.IsAnswerObject); }
        }

        public ISemanticEvent LastAnswer
        {
            get { return SemanticEvents.LastOrDefault(Codings.IsAnswerObject); }
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

                var firstAnswerContents = Codings.GetAnswerObjectContent(firstAnswer);
                var lastAnswerContents = Codings.GetAnswerObjectContent(lastAnswer);

                var analysisCode = string.IsNullOrWhiteSpace(analysisObjectCode) ? string.Empty : string.Format("{0} [{1}, {2}]", analysisObjectCode, firstAnswerContents, lastAnswerContents);

                return analysisCode;
            }
        }

        public override Category Category => Category.Answer;

        public override string FormattedName => "Answer Changed After Representation";

        public override string FormattedValue
        {
            get
            {
                var firstAnswer = FirstAnswer;
                var lastAnswer = LastAnswer;
                if (firstAnswer == null ||
                    lastAnswer == null ||
                    firstAnswer.ID == lastAnswer.ID)
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
                    SemanticEvents.Where(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD && Codings.FriendlyObjects.ContainsKey(h.CodedObject))
                                  .Select(h => string.Format("{0} [{1}]", h.CodedObject, h.CodedObjectID));

                return string.Format("{0}\nRepresentations: {1}{2}",
                                     answerChanged,
                                     string.Join(", ", representationsAdded),
                                     string.IsNullOrWhiteSpace(analysisCode) ? string.Empty : "\nCode: " + analysisCode);
            }
        }

        #endregion //ATagBase Overrides
    }
}