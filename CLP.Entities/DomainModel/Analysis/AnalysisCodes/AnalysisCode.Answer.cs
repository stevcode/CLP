namespace CLP.Entities
{
    public partial class AnalysisCode
    {
        public static void AddFinalAnswerCorrectness(IAnalysis tag, string answerObject, string correctAnswer, string studentAnswer, string codedCorrectness)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS);
            
            if (answerObject == "Final Answer Fill In")
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_OBJECT, Codings.CONSTRAINT_VALUE_ANSWER_OBJECT_FILL_IN);
            }
            else
            {
                analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_OBJECT, Codings.CONSTRAINT_VALUE_ANSWER_OBJECT_MULTIPLE_CHOICE);
            }
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECT_ANSWER, correctAnswer);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_STUDENT_ANSWER, studentAnswer);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS, codedCorrectness);
            analysisCode.AddConstraint(Codings.CONSTRAINT_HISTORY_STATUS, Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL);

            tag.QueryCodes.Add(analysisCode);
        }
    }
}
