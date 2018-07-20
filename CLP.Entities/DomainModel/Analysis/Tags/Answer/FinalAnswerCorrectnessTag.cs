using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class FinalAnswerCorrectnessTag : AAnalysisTagBase
    {
        #region Constants

        public const string BLANK_STUDENT_ANSWER = "BLANK";

        #endregion // Constants

        #region Constructors

        public FinalAnswerCorrectnessTag() { }

        public FinalAnswerCorrectnessTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion // Constructors

        #region Properties

        /// <summary>Classifies Final Answer as MC or Fill In.</summary>
        public string FinalAnswerPageObjectType
        {
            get => GetValue<string>(FinalAnswerPageObjectTypeProperty);
            set => SetValue(FinalAnswerPageObjectTypeProperty, value);
        }

        public static readonly PropertyData FinalAnswerPageObjectTypeProperty = RegisterProperty("FinalAnswerPageObjectType", typeof(string), string.Empty);

        /// <summary>Value of the correct answer.</summary>
        public string CorrectAnswer
        {
            get => GetValue<string>(CorrectAnswerProperty);
            set => SetValue(CorrectAnswerProperty, value);
        }

        public static readonly PropertyData CorrectAnswerProperty = RegisterProperty("CorrectAnswer", typeof(string), Codings.ANSWER_UNDEFINED);

        /// <summary>Value of the student's answer.</summary>
        public string StudentAnswer
        {
            get => GetValue<string>(StudentAnswerProperty);
            set => SetValue(StudentAnswerProperty, value);
        }

        public static readonly PropertyData StudentAnswerProperty = RegisterProperty("StudentAnswer", typeof(string), string.Empty);

        /// <summary>Correctness of student's answer.</summary>
        public Correctness FinalAnswerCorrectness
        {
            get => GetValue<Correctness>(FinalAnswerCorrectnessProperty);
            set => SetValue(FinalAnswerCorrectnessProperty, value);
        }

        public static readonly PropertyData FinalAnswerCorrectnessProperty = RegisterProperty("FinalAnswerCorrectness", typeof(Correctness), Correctness.Unknown);

        #endregion // Properties

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

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
                var studentAnswerDescription = StudentAnswer == BLANK_STUDENT_ANSWER ? "Student left final answer blank" : $"Student answered {StudentAnswer}";
                var finalAnswerFriendlyCorrectness = Codings.CorrectnessToFriendlyCorrectness(FinalAnswerCorrectness);
                var analysisCodes = string.Join("\n", QueryCodes.Select(c => c.FormattedValue));
                var codedSection = QueryCodes.Any() ? $"\nCodes:\n{analysisCodes}" : string.Empty;

                var formattedValue = $"{correctAnswerDescription}\n{studentAnswerDescription} ({finalAnswerFriendlyCorrectness}){codedSection}";
                return formattedValue;
            }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static FinalAnswerCorrectnessTag AttemptTagGeneration(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var lastFinalAnswerEvent = semanticEvents.LastOrDefault(e => Codings.IsFinalAnswerEvent(e) &&
                                                                         e.CodedObject != Codings.OBJECT_INTERMEDIARY_FILL_IN &&
                                                                         e.EventType != Codings.EVENT_MULTIPLE_CHOICE_ADD_PARTIAL &&
                                                                         e.EventType != Codings.EVENT_MULTIPLE_CHOICE_ERASE_PARTIAL &&
                                                                         (e.CodedObject == Codings.OBJECT_MULTIPLE_CHOICE
                                                                              ? e.EventType != Codings.EVENT_MULTIPLE_CHOICE_ERASE
                                                                              : true));

            var tag = new FinalAnswerCorrectnessTag(page, Origin.StudentPageGenerated);
            if (lastFinalAnswerEvent == null)
            {
                IPageObject finalAnswerPageObject = page.PageObjects.OfType<MultipleChoice>().FirstOrDefault();
                if (finalAnswerPageObject == null)
                {
                    finalAnswerPageObject = page.PageObjects.OfType<InterpretationRegion>().FirstOrDefault(p => !p.IsIntermediary);
                    if (finalAnswerPageObject == null)
                    {
                        return null;
                    }
                }

                if (finalAnswerPageObject is MultipleChoice multipleChoice)
                {
                    tag.FinalAnswerPageObjectType = Codings.FriendlyObjects[Codings.OBJECT_MULTIPLE_CHOICE];
                    tag.CorrectAnswer = multipleChoice.CodedID;
                }

                if (finalAnswerPageObject is InterpretationRegion)
                {
                    tag.FinalAnswerPageObjectType = Codings.FriendlyObjects[Codings.OBJECT_FILL_IN];
                    if (page.Tags.FirstOrDefault(t => t is IDefinition) is IDefinition relationDefinitionTag)
                    {
                        var definitionAnswer = relationDefinitionTag.Answer;
                        var truncatedAnswer = (int)Math.Truncate(definitionAnswer);
                        tag.CorrectAnswer = truncatedAnswer.ToString();
                    }
                }

                tag.StudentAnswer = BLANK_STUDENT_ANSWER;
                tag.FinalAnswerCorrectness = Correctness.Unanswered;
            }
            else
            {
                tag.SemanticEventIDs.Add(lastFinalAnswerEvent.ID);

                tag.FinalAnswerPageObjectType = Codings.FriendlyObjects[lastFinalAnswerEvent.CodedObject];
                tag.CorrectAnswer = lastFinalAnswerEvent.CodedObjectID;
                tag.StudentAnswer = Codings.GetFinalAnswerEventStudentAnswer(lastFinalAnswerEvent);
                var codedCorrectness = Codings.GetFinalAnswerEventCorrectness(lastFinalAnswerEvent);
                tag.FinalAnswerCorrectness = Codings.CodedCorrectnessToCorrectness(codedCorrectness);
            }

            AnalysisCode.AddFinalAnswerCorrectness(tag,
                                                   tag.FinalAnswerPageObjectType,
                                                   tag.CorrectAnswer,
                                                   tag.StudentAnswer,
                                                   Codings.CorrectnessToCodedCorrectness(tag.FinalAnswerCorrectness));
            page.AddTag(tag);

            return tag;
        }

        #endregion // Static Methods
    }
}