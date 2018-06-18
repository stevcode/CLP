namespace CLP.Entities
{
    public partial class AnalysisCode
    {
        public static void AddFinalAnswerBeforeRepresentation(IAnalysis tag, Correctness correctness)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE, Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS, Codings.CorrectnessToCodedCorrectness(correctness));

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddIntermediaryAnswerBeforeRepresentation(IAnalysis tag, Correctness correctness)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE, Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS, Codings.CorrectnessToCodedCorrectness(correctness));

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddRepresentationAfterFinalAnswer(IAnalysis tag, Correctness correctness)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE, Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, Codings.CorrectnessToCodedCorrectness(correctness));

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddRepresentationAfterIntermediaryAnswer(IAnalysis tag, Correctness correctness)
        {
            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE, Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY);
            analysisCode.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, Codings.CorrectnessToCodedCorrectness(correctness));

            tag.QueryCodes.Add(analysisCode);
        }

        public static void AddChangedAnswerAfterRepresentation(IAnalysis tag, Correctness initialCorrectness, Correctness finalCorrectness)
        {
            var initialCodedCorrectness = Codings.CorrectnessToCodedCorrectness(initialCorrectness);
            var finalCodedCorrectness = Codings.CorrectnessToCodedCorrectness(finalCorrectness);

            var analysisCode = new AnalysisCode(Codings.ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_CHANGE_FROM, initialCodedCorrectness);
            analysisCode.AddConstraint(Codings.CONSTRAINT_ANSWER_CHANGE_TO, finalCodedCorrectness);

            tag.QueryCodes.Add(analysisCode);
        }
    }
}
