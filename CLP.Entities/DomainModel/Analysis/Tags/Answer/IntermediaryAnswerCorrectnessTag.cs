using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class IntermediaryAnswerCorrectnessTag : AAnalysisTagBase
    {
        #region Constants

        public const string BLANK_STUDENT_ANSWER = "BLANK";

        #endregion // Constants

        #region Constructors

        public IntermediaryAnswerCorrectnessTag() { }

        public IntermediaryAnswerCorrectnessTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion // Constructors

        #region Properties

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
        public Correctness IntermediaryAnswerCorrectness
        {
            get { return GetValue<Correctness>(IntermediaryAnswerCorrectnessProperty); }
            set { SetValue(IntermediaryAnswerCorrectnessProperty, value); }
        }

        public static readonly PropertyData IntermediaryAnswerCorrectnessProperty = RegisterProperty("IntermediaryAnswerCorrectness", typeof(Correctness), Correctness.Unknown);

        #endregion // Properties

        #region ATagBase Overrides

        public override Category Category => Category.Answer;

        public override string FormattedName => "Intermediary Answer Correctness";

        public override string FormattedValue
        {
            get
            {
                //var correctAnswerDescription = $"Correct answer is {CorrectAnswer}";
                //var studentAnswerDescription = StudentAnswer == BLANK_STUDENT_ANSWER ? "Student left answer blank" : $"Student answered {StudentAnswer}";
                //var finalAnswerFriendlyCorrectness = Codings.CorrectnessToFriendlyCorrectness(IntermediaryAnswerCorrectness);
                //var analysisCodesDescription = SpreadSheetCodes.Any() ? $"\nCodes:\n{AnalysisCodesReport}" : string.Empty;

                //var formattedValue = $"{correctAnswerDescription}\n{studentAnswerDescription} ({finalAnswerFriendlyCorrectness}){analysisCodesDescription}";
                //return formattedValue;

                return string.Empty;
            }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static IntermediaryAnswerCorrectnessTag AttemptTagGeneration(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var lastIntermediaryAnswerEvent = semanticEvents.LastOrDefault(e => e.CodedObject == Codings.OBJECT_INTERMEDIARY_FILL_IN);

            var tag = new IntermediaryAnswerCorrectnessTag(page, Origin.StudentPageGenerated);
            if (lastIntermediaryAnswerEvent == null)
            {
                var interpretationRegion = page.PageObjects.OfType<InterpretationRegion>().FirstOrDefault(r => r.IsIntermediary);
                if (interpretationRegion == null)
                {
                    return null;
                }

                tag.CorrectAnswer = interpretationRegion.ExpectedValue.ToString();
                tag.StudentAnswer = BLANK_STUDENT_ANSWER;
                tag.IntermediaryAnswerCorrectness = Correctness.Unanswered;
            }
            else
            {
                tag.SemanticEventIDs.Add(lastIntermediaryAnswerEvent.ID);

                tag.CorrectAnswer = lastIntermediaryAnswerEvent.CodedObjectID;
                tag.StudentAnswer = Codings.GetFinalAnswerEventStudentAnswer(lastIntermediaryAnswerEvent);
                var codedCorrectness = Codings.GetFinalAnswerEventCorrectness(lastIntermediaryAnswerEvent);
                tag.IntermediaryAnswerCorrectness = Codings.CodedCorrectnessToCorrectness(codedCorrectness);

                tag.SpreadSheetCodes.Add(lastIntermediaryAnswerEvent.CodedValue);
            }

            page.AddTag(tag);

            return tag;
        }

        #endregion // Static Methods
    }
}
