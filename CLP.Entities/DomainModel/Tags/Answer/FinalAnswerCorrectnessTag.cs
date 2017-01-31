using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class FinalAnswerCorrectnessTag : AAnalysisTagBase
    {
        #region Constructors

        public FinalAnswerCorrectnessTag() { }

        public FinalAnswerCorrectnessTag(CLPPage parentPage, Origin origin, List<ISemanticEvent> semanticEvents)
            : base(parentPage, origin)
        {
            SemanticEventIDs = semanticEvents.Select(h => h.ID).ToList();

            Initialize();
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

        /// <summary>Classifies Final Answer as MC or Fill In.</summary>
        public string FinalAnswerPageObjectType
        {
            get { return GetValue<string>(FinalAnswerPageObjectTypeProperty); }
            set { SetValue(FinalAnswerPageObjectTypeProperty, value); }
        }

        public static readonly PropertyData FinalAnswerPageObjectTypeProperty = RegisterProperty("FinalAnswerPageObjectType", typeof(string), string.Empty);

        /// <summary>Value of the correct answer.</summary>
        public string CorrectAnswer
        {
            get { return GetValue<string>(CorrectAnswerProperty); }
            set { SetValue(CorrectAnswerProperty, value); }
        }

        public static readonly PropertyData CorrectAnswerProperty = RegisterProperty("CorrectAnswer", typeof(string), Codings.ANSWER_UNDEFINED);

        /// <summary>Value of the student's answer.</summary>
        public string StudentAnswer
        {
            get { return GetValue<string>(StudentAnswerProperty); }
            set { SetValue(StudentAnswerProperty, value); }
        }

        public static readonly PropertyData StudentAnswerProperty = RegisterProperty("StudentAnswer", typeof(string), string.Empty);

        /// <summary>Correctness of student's answer.</summary>
        public string FinalAnswerCorrectness
        {
            get { return GetValue<string>(FinalAnswerCorrectnessProperty); }
            set { SetValue(FinalAnswerCorrectnessProperty, value); }
        }

        public static readonly PropertyData FinalAnswerCorrectnessProperty = RegisterProperty("FinalAnswerCorrectness", typeof(string), string.Empty);

        #region Calculated Properties

        public List<ISemanticEvent> SemanticEvents
        {
            get { return ParentPage.History.SemanticEvents.Where(h => SemanticEventIDs.Contains(h.ID)).OrderBy(h => h.SemanticEventIndex).Distinct().ToList(); }
        }

        public ISemanticEvent Answer => SemanticEvents.FirstOrDefault(Codings.IsFinalAnswerEvent);

        #endregion // Calculated Properties

        #endregion // Properties

        #region Methods

        public void Initialize()
        {
            if (Answer == null)
            {
                var finalAnswerPageObject = ParentPage.PageObjects.FirstOrDefault(p => p is MultipleChoice || p is InterpretationRegion);
                if (finalAnswerPageObject == null)
                {
                    return;
                }

                var multipleChoice = finalAnswerPageObject as MultipleChoice;
                if (multipleChoice != null)
                {
                    FinalAnswerPageObjectType = Codings.FriendlyObjects[Codings.OBJECT_MULTIPLE_CHOICE];
                    CorrectAnswer = multipleChoice.CodedID;
                }

                var interpretationRegion = finalAnswerPageObject as InterpretationRegion;
                if (interpretationRegion != null)
                {
                    FinalAnswerPageObjectType = Codings.FriendlyObjects[Codings.OBJECT_FILL_IN];
                    var relationDefinitionTag = ParentPage.Tags.FirstOrDefault(t => t is IDefinition) as IDefinition;
                    if (relationDefinitionTag != null)
                    {
                        var definitionAnswer = relationDefinitionTag.Answer;
                        var truncatedAnswer = (int)Math.Truncate(definitionAnswer);
                        CorrectAnswer = truncatedAnswer.ToString();
                    }
                }

                StudentAnswer = "BLANK";
                FinalAnswerCorrectness = Codings.CORRECTNESS_INCORRECT;
            }
            else
            {
                FinalAnswerPageObjectType = Codings.FriendlyObjects[Answer.CodedObject];
                CorrectAnswer = Answer.CodedObjectID;
                StudentAnswer = Codings.GetFinalAnswerEventContent(Answer);
                FinalAnswerCorrectness = Codings.GetFinalAnswerEventCorrectness(Answer);

                AnalysisCodes.Add(Answer.CodedValue);
            }
        }

        #endregion // Methods

        #region ATagBase Overrides

        public override Category Category => Category.Answer;

        public override string FormattedName => "Final Answer Correctness";

        public override string FormattedValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FinalAnswerPageObjectType))
                {
                    return "[ERROR] No Final Answer Expected";
                }

                var correctAnswerDescription = $"{FinalAnswerPageObjectType}: Correct answer is {CorrectAnswer}";
                var studentAnswerDescription = StudentAnswer == "BLANK" ? "Student left final answer blank" : $"Student answered {StudentAnswer}";
                var finalAnswerFriendlyCorrectness = Codings.FriendlyCorrectness[FinalAnswerCorrectness];
                var analysisCodesDescription = AnalysisCodes.Any() ? $"\nCodes: {AnalysisCodesReport}" : string.Empty;

                var formattedValue = $"{correctAnswerDescription}\n{studentAnswerDescription} ({finalAnswerFriendlyCorrectness}){analysisCodesDescription}";
                return formattedValue;
            }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static void AttemptTagGeneration(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var lastFinalAnswerEvent =
                semanticEvents.LastOrDefault(e => Codings.IsFinalAnswerEvent(e) && 
                                                  e.EventType != Codings.EVENT_MULTIPLE_CHOICE_ADD_PARTIAL &&
                                                  e.EventType != Codings.EVENT_MULTIPLE_CHOICE_ERASE_PARTIAL &&
                                                  e.EventType != Codings.EVENT_MULTIPLE_CHOICE_ERASE);
            if (lastFinalAnswerEvent == null)
            {
                var isFinalAnswerOnPage = page.PageObjects.Any(p => p is InterpretationRegion || p is MultipleChoice);
                if (!isFinalAnswerOnPage)
                {
                    return;
                }
            }

            var tag = new FinalAnswerCorrectnessTag(page,
                                                    Origin.StudentPageGenerated,
                                                    new List<ISemanticEvent>
                                                    {
                                                        lastFinalAnswerEvent
                                                    });
            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}