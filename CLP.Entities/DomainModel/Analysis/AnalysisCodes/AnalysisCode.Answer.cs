namespace CLP.Entities
{
    public partial class AnalysisCode
    {
        public static void AddFinalAnswerCorrectness(IAnalysis tag, string answerObject, string correctAnswer, string studentAnswer, string codedCorrectness, bool isAnswerManuallyModified)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS);
            
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
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_MODIFICATION, isAnswerManuallyModified ? Codings.CONSTRAINT_VALUE_YES : Codings.CONSTRAINT_VALUE_NO);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddFinalRepresentationCorrectness(IAnalysis tag, string codedCorrectness)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_FINAL_REPRESENTATION_CORRECTNESS);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, codedCorrectness);

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddOverallCorrectness(IAnalysis tag, string codedCorrectness)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_OVERALL_CORRECTNESS);
            analysisCode.AddConstraint(Codings.CONSTRAINT_OVERALL_CORRECTNESS, codedCorrectness);

            tag.QueryCodes.Add(analysisCode);
        }
    }
}
