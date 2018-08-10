using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static partial class Codings
    {
        #region Analysis Labels

        public const string ANALYSIS_LABEL_REPRESENTATIONS_USED = "REPRESENTATIONS USED";
        public const string ANALYSIS_LABEL_REPRESENTATION_ORDER = "REPRESENTATION ORDER";
        public const string ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION = "ANSWER BEFORE REPRESENTATION";
        public const string ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION = "CHANGED ANSWER AFTER REPRESENTATION";
        public const string ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS = "FINAL ANSWER CORRECTNESS";
        public const string ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP = "MULTIPLE REPRESENTATIONS - 1-STEP";
        public const string ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP = "MULTIPLE REPRESENTATIONS - 2-STEP";
        
        public const string ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER = "REPRESENTATION AFTER ANSWER";
        public const string ANALYSIS_LABEL_PROBLEM_TYPE = "PROBLEM TYPE";

        public const string ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP = "ARRAY SKIP";
        public const string ANALYSIS_LABEL_STRATEGY_ARRAY_PARTIAL_PRODUCT = "ARRAY PARTIAL PRODUCT";

        public const string ANALYSIS_LABEL_WORD_PROBLEM = "WORD PROBLEM";
        public const string ANALYSIS_LABEL_PAGE_DEFINITION = "ANSWER DEFINITION";

        public const string ANALYSIS_LABEL_NUMBER_LINE_JUMP_ERASURES = "NUMBER LINE JUMP ERASURES";

        #endregion // Analysis Labels

        #region Analysis Short Names

        public const string ANALYSIS_SHORT_NAME_REPRESENTATIONS_USED = "REPS_USED";
        public const string ANALYSIS_SHORT_NAME_REPRESENTATION_ORDER = "REP_ORDER";
        public const string ANALYSIS_SHORT_NAME_ANSWER_BEFORE_REPRESENTATION = "ABR";
        public const string ANALYSIS_SHORT_NAME_CHANGED_ANSWER_AFTER_REPRESENTATION = "CAAR";
        public const string ANALYSIS_SHORT_NAME_FINAL_ANSWER_CORRECTNESS = "ANS_FINAL";
        public const string ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_1_STEP = "MR";
        public const string ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_2_STEP = "MR2STEP";
        
        public const string ANALYSIS_SHORT_NAME_REPRESENTATION_AFTER_ANSWER = "RAA";
        public const string ANALYSIS_SHORT_NAME_PROBLEM_TYPE = "PROBLEM";

        public const string ANALYSIS_SHORT_NAME_STRATEGY_ARRAY_SKIP = "ARR_SKIP";
        public const string ANALYSIS_SHORT_NAME_STRATEGY_ARRAY_PARTIAL_PRODUCT = "ARR_PARTIAL_PRODUCT";

        public const string ANALYSIS_SHORT_NAME_WORD_PROBLEM = "WORD_PROBLEM";
        public const string ANALYSIS_SHORT_NAME_PAGE_DEFINITION = "ANSWER_DEF";

        public const string ANALYSIS_SHORT_NAME_NUMBER_LINE_JUMP_ERASURES = "NLJE";

        #endregion // Analysis Short Names

        #region Analysis Constraint Labels

        public const string CONSTRAINT_REPRESENTATION_NAME_LAX = "REPRESENTATION_NAME_LAX";
        public const string CONSTRAINT_REPRESENTATION_CORRECTNESS = "REPRESENTATION_CORRECTNESS";
        public const string CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON = "REPRESENTATION_CORRECTNESS_REASON";
        public const string CONSTRAINT_REPRESENTATION_CODED_ID = "CODED_ID";
        public const string CONSTRAINT_REPRESENTATION_FIRST = "REPRESENTATION_NAME_FIRST";
        public const string CONSTRAINT_REPRESENTATION_LAST = "REPRESENTATION_NAME_LAST";

        public const string CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS = "MULTIPLE_REPRESENTATION_CORRECTNESS";

        public const string CONSTRAINT_HISTORY_STATUS = "HISTORY_STATUS";

        public const string CONSTRAINT_ANSWER_TYPE = "ANSWER_TYPE";
        public const string CONSTRAINT_ANSWER_CORRECTNESS = "ANSWER_CORRECTNESS";
        public const string CONSTRAINT_ANSWER_MODIFICATION = "ANSWER_MODIFICATION";
        
        public const string CONSTRAINT_ANSWER_CHANGE_FROM = "ANSWER_CHANGE_FROM";
        public const string CONSTRAINT_ANSWER_CHANGE_TO = "ANSWER_CHANGE_TO";
        public const string CONSTRAINT_ANSWER_OBJECT = "ANSWER_OBJECT";
        public const string CONSTRAINT_ANSWER_CORRECT_ANSWER = "CORRECT_ANSWER";
        public const string CONSTRAINT_ANSWER_STUDENT_ANSWER = "STUDENT_ANSWER";

        public const string CONSTRAINT_ARITH_STATUS = "ARITH_STATUS";

        public const string CONSTRAINT_LOCATION = "LOCATION";

        public const string CONSTRAINT_STRATEGY_CORRECTNESS = "STRATEGY_CORRECTNESS";
        public const string CONSTRAINT_STRATEGY_CORRECTNESS_REASON = "STRATEGY_CORRECTNESS_REASON";

        public const string CONSTRAINT_STRATEGY_TECHNIQUE = "STRATEGY_TECHNIQUE";
        public const string CONSTRAINT_STRATEGY_FRIENDLY_NUMBERS = "STRATEGY_FRIENDLY_NUMBERS";

        public const string CONSTRAINT_IS_WORD_PROBLEM = "IS_WORD_PROBLEM";
        public const string CONSTRAINT_PROBLEM_TYPE = "PROBLEM_TYPE";

        public const string CONSTRAINT_COUNT = "COUNT";

        #endregion // Analysis Constraint Labels

        #region Analysis Constraint Values

        public const string CONSTRAINT_VALUE_ANY = "ANY";
        public const string CONSTRAINT_VALUE_YES = "YES";
        public const string CONSTRAINT_VALUE_NO = "NO";

        public const string CONSTRAINT_VALUE_REPRESENTATION_NAME_BLANK_PAGE = "BLANK_PAGE";
        public const string CONSTRAINT_VALUE_REPRESENTATION_NAME_INK_ONLY = "INK_ONLY";
        public const string CONSTRAINT_VALUE_REPRESENTATION_NAME_NONE = "NONE";

        public const string CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_ALL = "ALL_COR";
        public const string CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_SOME = "SOME_COR";
        public const string CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_NONE = "NONE_COR";

        public const string CONSTRAINT_VALUE_HISTORY_STATUS_FINAL = "FINAL";
        public const string CONSTRAINT_VALUE_HISTORY_STATUS_DELETED = "DELETED";
        public const string CONSTRAINT_VALUE_HISTORY_STATUS_ERASED = "ERASED";

        public const string CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY = "IA";
        public const string CONSTRAINT_VALUE_ANSWER_TYPE_FINAL = "FA";

        public const string CONSTRAINT_VALUE_ANSWER_OBJECT_MULTIPLE_CHOICE = "MULTIPLE_CHOICE";
        public const string CONSTRAINT_VALUE_ANSWER_OBJECT_FILL_IN = "FILL_IN";

        public const string CONSTRAINT_VALUE_ARITH_STATUS_NO_ARITH = "NO_ARITH";
        public const string CONSTRAINT_VALUE_ARITH_STATUS_PLUS_ARITH = "PLUS_ARITH";

        public const string CONSTRAINT_VALUE_LOCATION_RIGHT = "RIGHT";
        public const string CONSTRAINT_VALUE_LOCATION_BOTTOM = "BOTTOM";

        public const string CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_GAPS_OR_OVERLAPS = "GAPS_OR_OVERLAPS";
        public const string CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_SWAPPED = "SWAPPED";
        public const string CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_UNKNOWN = "UNKNOWN";

        public const string CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_WRONG_DIMENSION = "WRONG_DIMENSION";
        public const string CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_LIKELY_ARITH_ERROR = "LIKELY_ARITH_ERROR";
        public const string CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_UNKNOWN = "UNKNOWN";

        public const string CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_DIVIDE = "DIVIDE";
        public const string CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_INK_DIVIDE = "INK_DIVIDE";
        public const string CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_CUT_AND_SNAP = "CUT_AND_SNAP";

        public const string CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_TWO = "TWO";
        public const string CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_FIVE = "FIVE";
        public const string CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_TEN = "TEN";
        public const string CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_HALF = "HALF";
        public const string CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_NONE = "NONE";

        public const string CONSTRAINT_VALUE_PROBLEM_TYPE_MULTIPLICATION = "MULTIPLICATION";
        public const string CONSTRAINT_VALUE_PROBLEM_TYPE_DIVISION = "DIVISION";
        public const string CONSTRAINT_VALUE_PROBLEM_TYPE_EQUIVALENCE = "EQUIVALENCE";
        public const string CONSTRAINT_VALUE_PROBLEM_TYPE_OTHER = "OTHER";
        public const string CONSTRAINT_VALUE_PROBLEM_TYPE_NONE = "NONE";

        #endregion // Analysis Constraint Values

        #region (OBSOLETE?) Analysis Codes

        #region Representation Sequence

        public const string ANALYSIS_FINAL_ANS_COR_BEFORE_REP = "FABR-C";
        public const string ANALYSIS_FINAL_ANS_INC_BEFORE_REP = "FABR-I";
        public const string ANALYSIS_INTERMEDIARY_ANS_COR_BEFORE_REP = "IABR-C";
        public const string ANALYSIS_INTERMEDIARY_ANS_INC_BEFORE_REP = "IABR-I";
        public const string ANALYSIS_INC_TO_COR_AFTER_REP = "ARIC";
        public const string ANALYSIS_COR_TO_INC_AFTER_REP = "ARCI";
        public const string ANALYSIS_COR_TO_COR_AFTER_REP = "ARCC";
        public const string ANALYSIS_INC_TO_INC_AFTER_REP = "ARII";
        public const string ANALYSIS_REP_AFTER_FINAL_ANSWER = "RAFA";
        public const string ANALYSIS_REP_AFTER_INTERMEDIARY_ANSWER = "RAIA";

        #endregion // Representation Sequence

        #region Strategies

        public const string STRATEGY_NAME_ARRAY_COUNT_BY_ONE = "COUNT-BY-ONE";
        public const string STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT = "PART";
        public const string STRATEGY_NAME_ARRAY_SKIP = "SKIP";

        public const string STRATEGY_NAME_NUMBER_LINE_JUMP = "JUMP";
        public const string STRATEGY_NAME_NUMBER_LINE_REPEAT_ADDITION = "REPEAT ADD";

        public const string STRATEGY_NAME_BINS_DEAL = "DEAL";

        #endregion // Strategies

        #region Strategy Specifics

        public const string STRATEGY_SPECIFICS_ARRAY_CUT = "cut";
        public const string STRATEGY_SPECIFICS_ARRAY_SNAP = "snap";
        public const string STRATEGY_SPECIFICS_ARRAY_CUT_SNAP = "cut and snap";
        public const string STRATEGY_SPECIFICS_ARRAY_DIVIDE = "divide";
        public const string STRATEGY_SPECIFICS_ARRAY_DIVIDE_INK = "ink divide";
        public const string STRATEGY_SPECIFICS_ARRAY_ARITH = "+arith";
        public const string STRATEGY_SPECIFICS_ARRAY_DOTS = "dots";

        public const string STRATEGY_SPECIFICS_NUMBER_LINE_ARITH = "+arith";

        #endregion // Strategy Specifics

        #region Misc

        public const string NUMBER_LINE_NLJE = "NLJE";

        public const string NUMBER_LINE_BLANK_PARTIAL_MATCH = "NLBP";

        public const string REPRESENTATIONS_MR = "MR";
        public const string REPRESENTATIONS_MR2STEP = "MR2STEP";

        #endregion // Misc

        #endregion // (OBSOLETE?) Analysis Codes

        #region Methods

        public static string AnalysisLabelToShortName(string label)
        {
            switch (label)
            {
                case ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP:
                    return ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_1_STEP;
                case ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP:
                    return ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_2_STEP;
                case ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION:
                    return ANALYSIS_SHORT_NAME_CHANGED_ANSWER_AFTER_REPRESENTATION;
                case ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION:
                    return ANALYSIS_SHORT_NAME_ANSWER_BEFORE_REPRESENTATION;
                case ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER:
                    return ANALYSIS_SHORT_NAME_REPRESENTATION_AFTER_ANSWER;
                case ANALYSIS_LABEL_REPRESENTATIONS_USED:
                    return ANALYSIS_SHORT_NAME_REPRESENTATIONS_USED;
                case ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS:
                    return ANALYSIS_SHORT_NAME_FINAL_ANSWER_CORRECTNESS;
                case ANALYSIS_LABEL_PROBLEM_TYPE:
                    return ANALYSIS_SHORT_NAME_PROBLEM_TYPE;
                case ANALYSIS_LABEL_REPRESENTATION_ORDER:
                    return ANALYSIS_SHORT_NAME_REPRESENTATION_ORDER;
                case ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP:
                    return ANALYSIS_SHORT_NAME_STRATEGY_ARRAY_SKIP;
                case ANALYSIS_LABEL_STRATEGY_ARRAY_PARTIAL_PRODUCT:
                    return ANALYSIS_SHORT_NAME_STRATEGY_ARRAY_PARTIAL_PRODUCT;
                case ANALYSIS_LABEL_WORD_PROBLEM:
                    return ANALYSIS_SHORT_NAME_WORD_PROBLEM;
                case ANALYSIS_LABEL_PAGE_DEFINITION:
                    return ANALYSIS_SHORT_NAME_PAGE_DEFINITION;
                case ANALYSIS_LABEL_NUMBER_LINE_JUMP_ERASURES:
                    return ANALYSIS_SHORT_NAME_NUMBER_LINE_JUMP_ERASURES;
            }

            return "No matching Short Name";
        }

        public static string AnalysisShortNameToLabel(string alias)
        {
            switch (alias)
            {
                case ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_1_STEP:
                    return ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP;
                case ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_2_STEP:
                    return ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP;
                case ANALYSIS_SHORT_NAME_CHANGED_ANSWER_AFTER_REPRESENTATION:
                    return ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION;
                case ANALYSIS_SHORT_NAME_ANSWER_BEFORE_REPRESENTATION:
                    return ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION;
                case ANALYSIS_SHORT_NAME_REPRESENTATION_AFTER_ANSWER:
                    return ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER;
                case ANALYSIS_SHORT_NAME_REPRESENTATIONS_USED:
                    return ANALYSIS_LABEL_REPRESENTATIONS_USED;
                case ANALYSIS_SHORT_NAME_FINAL_ANSWER_CORRECTNESS:
                    return ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS;
                case ANALYSIS_SHORT_NAME_PROBLEM_TYPE:
                    return ANALYSIS_LABEL_PROBLEM_TYPE;
                case ANALYSIS_SHORT_NAME_REPRESENTATION_ORDER:
                    return ANALYSIS_LABEL_REPRESENTATION_ORDER;
                case ANALYSIS_SHORT_NAME_STRATEGY_ARRAY_SKIP:
                    return ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP;
                case ANALYSIS_SHORT_NAME_STRATEGY_ARRAY_PARTIAL_PRODUCT:
                    return ANALYSIS_LABEL_STRATEGY_ARRAY_PARTIAL_PRODUCT;
                case ANALYSIS_SHORT_NAME_WORD_PROBLEM:
                    return ANALYSIS_LABEL_WORD_PROBLEM;
                case ANALYSIS_SHORT_NAME_PAGE_DEFINITION:
                    return ANALYSIS_LABEL_PAGE_DEFINITION;
                case ANALYSIS_SHORT_NAME_NUMBER_LINE_JUMP_ERASURES:
                    return ANALYSIS_LABEL_NUMBER_LINE_JUMP_ERASURES;
            }

            return "No matching Label";
        }

        public static List<string> GetAllAnalysisLabels()
        {
            var list = new List<string>
                       {
                           ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP,
                           ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP,
                           ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION,
                           ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION,
                           //ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER,
                           ANALYSIS_LABEL_REPRESENTATIONS_USED,
                           //ANALYSIS_LABEL_ARRAY_SKIP_COUNTING,
                           ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS,
                           //ANALYSIS_LABEL_PROBLEM_TYPE,
                           ANALYSIS_LABEL_REPRESENTATION_ORDER,
                           ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP,
                           ANALYSIS_LABEL_STRATEGY_ARRAY_PARTIAL_PRODUCT,
                           ANALYSIS_LABEL_WORD_PROBLEM,
                           ANALYSIS_LABEL_PAGE_DEFINITION,
                           ANALYSIS_LABEL_NUMBER_LINE_JUMP_ERASURES
                       };

            return list;
        }

        public static List<string> GetAllAnalysisShortNames()
        {
            var labels = GetAllAnalysisLabels();
            var aliases = labels.Select(AnalysisLabelToShortName).ToList();

            return aliases;
        }

        public static string ConstraintLabelToShortName(string label)
        {
            switch (label)
            {
                case CONSTRAINT_REPRESENTATION_NAME_LAX:
                    return "Type";
                case CONSTRAINT_REPRESENTATION_CORRECTNESS:
                    return "Correctness";
                case CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON:
                    return "Reason";
                case CONSTRAINT_REPRESENTATION_CODED_ID:
                    return "ID";
                case CONSTRAINT_REPRESENTATION_FIRST:
                    return "First";
                case CONSTRAINT_REPRESENTATION_LAST:
                    return "Last";

                case CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS:
                    return "Correctness";

                case CONSTRAINT_HISTORY_STATUS:
                    return "Status";

                case CONSTRAINT_ANSWER_TYPE:
                    return "Answer Type";
                case CONSTRAINT_ANSWER_CORRECTNESS:
                    return "Correctness";
                case CONSTRAINT_ANSWER_MODIFICATION:
                    return "Manually Modified";
                case CONSTRAINT_ANSWER_CHANGE_FROM:
                    return "Changed From";
                case CONSTRAINT_ANSWER_CHANGE_TO:
                    return "Changed To";
                case CONSTRAINT_ANSWER_OBJECT:
                    return "Type";

                case CONSTRAINT_ARITH_STATUS:
                    return "Arith Status";

                case CONSTRAINT_LOCATION:
                    return "Location";

                case CONSTRAINT_STRATEGY_CORRECTNESS:
                    return "Correctness";
                case CONSTRAINT_STRATEGY_CORRECTNESS_REASON:
                    return "Reason";

                case CONSTRAINT_STRATEGY_TECHNIQUE:
                    return "Technique";
                case CONSTRAINT_STRATEGY_FRIENDLY_NUMBERS:
                    return "Friendly Numbers";

                case CONSTRAINT_IS_WORD_PROBLEM:
                    return "Value";
                case CONSTRAINT_PROBLEM_TYPE:
                    return "Problem Type";

                case CONSTRAINT_COUNT:
                    return "Count";
            }

            return "No matching Alias";
        }

        #endregion // Methods
    }
}
