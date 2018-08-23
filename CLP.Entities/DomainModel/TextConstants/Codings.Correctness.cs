namespace CLP.Entities
{
    public static partial class Codings
    {
        public const string CORRECTNESS_CODED_CORRECT = "COR";
        public const string CORRECTNESS_CODED_PARTIAL = "PAR";
        public const string CORRECTNESS_CODED_INCORRECT = "INC";
        public const string CORRECTNESS_CODED_ILLEGIBLE = "ILL";
        public const string CORRECTNESS_CODED_UNANSWERED = "UNANSWERED";
        public const string CORRECTNESS_CODED_UNKNOWN = "UNKNOWN";
        public const string CORRECTNESS_CODED_NOT_COR = "NOT_COR";
        public const string CORRECTNESS_CODED_PAR_OR_INC = "PAR_OR_INC";

        public const string ANSWER_UNDEFINED = "UNDEFINED";

        public const string MATCHED_RELATION_LEFT = "LS";
        public const string MATCHED_RELATION_RIGHT = "RS";
        public const string MATCHED_RELATION_ALTERNATIVE = "ALT";
        public const string MATCHED_RELATION_NONE = "UNMATCHED";

        public const string PARTIAL_REASON_UNKNOWN = "UNKNOWN";
        public const string PARTIAL_REASON_SWAPPED = "SWAPPED";
        public const string PARTIAL_REASON_GAPS_AND_OVERLAPS = "GAPS AND OVERLAPS";

        #region Conversion Methods

        public static string CorrectnessToCodedCorrectness(Correctness correctness)
        {
            switch (correctness)
            {
                case Correctness.Correct:
                    return CORRECTNESS_CODED_CORRECT;
                case Correctness.PartiallyCorrect:
                    return CORRECTNESS_CODED_PARTIAL;
                case Correctness.Incorrect:
                    return CORRECTNESS_CODED_INCORRECT;
                case Correctness.Illegible:
                    return CORRECTNESS_CODED_ILLEGIBLE;
                case Correctness.Unanswered:
                    return CORRECTNESS_CODED_UNANSWERED;
                default:
                    return CORRECTNESS_CODED_UNKNOWN;
            }
        }

        public static string CorrectnessToFriendlyCorrectness(Correctness correctness)
        {
            return correctness.ToDescription();
        }

        public static string CodedCorrectnessToFriendlyCorrectness(string codedCorrectness)
        {
            var correctness = CodedCorrectnessToCorrectness(codedCorrectness);
            return CorrectnessToFriendlyCorrectness(correctness);
        }

        public static Correctness CodedCorrectnessToCorrectness(string codedCorrectness)
        {
            switch (codedCorrectness)
            {
                case CORRECTNESS_CODED_CORRECT:
                    return Correctness.Correct;
                case CORRECTNESS_CODED_PARTIAL:
                    return Correctness.PartiallyCorrect;
                case CORRECTNESS_CODED_INCORRECT:
                    return Correctness.Incorrect;
                case CORRECTNESS_CODED_ILLEGIBLE:
                    return Correctness.Illegible;
                case CORRECTNESS_CODED_UNANSWERED:
                    return Correctness.Unanswered;
                default:
                    return Correctness.Unknown;
            }
        }

        #endregion // Conversion Methods
    }
}
