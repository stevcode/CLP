using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static partial class Codings
    {
        #region Generic

        public const string NOT_APPLICABLE = "NA";

        #endregion // Generic

        #region Analysis Labels

        public const string ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP = "MULTIPLE REPRESENTATIONS - 1-STEP";
        public const string ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP = "MULTIPLE REPRESENTATIONS - 2-STEP";
        public const string ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION = "CHANGED ANSWER AFTER REPRESENTATION";
        public const string ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION = "ANSWER BEFORE REPRESENTATION";
        public const string ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER = "REPRESENTATION AFTER ANSWER";
        public const string ANALYSIS_LABEL_REPRESENTATIONS_USED = "REPRESENTATIONS USED";
        public const string ANALYSIS_LABEL_ARRAY_SKIP_COUNTING = "ARRAY SKIP COUNTING";
        public const string ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS = "FILL IN ANSWER CORRECTNESS";
        public const string ANALYSIS_LABEL_PROBLEM_TYPE = "PROBLEM TYPE";

        #endregion // Analysis Labels

        #region Analysis Aliases

        public const string ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_1_STEP = "MR";
        public const string ANALYSIS_ALIAS_MULTIPLE_REPRESENTATIONS_2_STEP = "MR2STEP";
        public const string ANALYSIS_ALIAS_CHANGED_ANSWER_AFTER_REPRESENTATION = "CAAR";
        public const string ANALYSIS_ALIAS_ANSWER_BEFORE_REPRESENTATION = "ABR";
        public const string ANALYSIS_ALIAS_REPRESENTATION_AFTER_ANSWER = "RAA";
        public const string ANALYSIS_ALIAS_REPRESENTATIONS_USED = "REPS_USED";
        public const string ANALYSIS_ALIAS_ARRAY_SKIP_COUNTING = "SKIP";
        public const string ANALYSIS_ALIAS_FILL_IN_ANSWER_CORRECTNESS = "ANS";
        public const string ANALYSIS_SHORT_NAME_PROBLEM_TYPE = "PROBLEM";

        #endregion // Analysis Labels

        #region Analysis Constraint Labels

        public const string CONSTRAINT_ANSWER_CHANGE = "ANSWER_CHANGE";
        public const string CONSTRAINT_ANSWER_TYPE = "ANSWER_TYPE";
        public const string CONSTRAINT_ANSWER_CORRECTNESS = "ANSWER_CORRECTNESS";
        public const string CONSTRAINT_ANSWER_OBJECT = "ANSWER_OBJECT";
        public const string CONSTRAINT_ANSWER_CORRECT_ANSWER = "CORRECT_ANSWER";
        public const string CONSTRAINT_ANSWER_STUDENT_ANSWER = "STUDENT_ANSWER";

        public const string CONSTRAINT_REPRESENTATION_CORRECTNESS = "REPRESENTATION_CORRECTNESS";
        public const string CONSTRAINT_REPRESENTATION_NAME = "REPRESENTATION_NAME";
        public const string CONSTRAINT_REPRESENTATION_CODED_ID = "CODED_ID";

        public const string CONSTRAINT_HISTORY_STATUS = "HISTORY_STATUS";

        #endregion // Analysis Constraint Labels

        #region Analysis Constraint Values

        public const string CONSTRAINT_VALUE_ANY = "ANY";

        public const string CONSTRAINT_VALUE_REPRESENTATION_NAME_BLANK_PAGE = "BLANK_PAGE";
        public const string CONSTRAINT_VALUE_REPRESENTATION_NAME_INK_ONLY = "INK_ONLY";
        public const string CONSTRAINT_VALUE_REPRESENTATION_NAME_NONE = "NONE";

        public const string CONSTRAINT_VALUE_HISTORY_STATUS_FINAL = "FINAL";
        public const string CONSTRAINT_VALUE_HISTORY_STATUS_DELETED = "DELETED";
        public const string CONSTRAINT_VALUE_HISTORY_STATUS_ERASED = "ERASED";

        public const string CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY = "INTERMEDIARY";
        public const string CONSTRAINT_VALUE_ANSWER_TYPE_FINAL = "FINAL";

        public const string CONSTRAINT_VALUE_ANSWER_OBJECT_MULTIPLE_CHOICE = "MULTIPLE_CHOICE";
        public const string CONSTRAINT_VALUE_ANSWER_OBJECT_FILL_IN = "FILL_IN";

        public const string CONSTRAINT_VALUE_ANSWER_CHANGE_DELIMITER = "_TO_";

        #endregion // Analysis Constraint Values

        #region Methods

        public static string AnalysisLabelToShortName(string label)
        {
            switch (label)
            {
                case ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP:
                    return ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_1_STEP;
                case ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP:
                    return ANALYSIS_ALIAS_MULTIPLE_REPRESENTATIONS_2_STEP;
                case ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION:
                    return ANALYSIS_ALIAS_CHANGED_ANSWER_AFTER_REPRESENTATION;
                case ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION:
                    return ANALYSIS_ALIAS_ANSWER_BEFORE_REPRESENTATION;
                case ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER:
                    return ANALYSIS_ALIAS_REPRESENTATION_AFTER_ANSWER;
                case ANALYSIS_LABEL_REPRESENTATIONS_USED:
                    return ANALYSIS_ALIAS_REPRESENTATIONS_USED;
                case ANALYSIS_LABEL_ARRAY_SKIP_COUNTING:
                    return ANALYSIS_ALIAS_ARRAY_SKIP_COUNTING;
                case ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS:
                    return ANALYSIS_ALIAS_FILL_IN_ANSWER_CORRECTNESS;
                case ANALYSIS_LABEL_PROBLEM_TYPE:
                    return ANALYSIS_SHORT_NAME_PROBLEM_TYPE;
            }

            return "No matching Alias";
        }

        public static string AnalysisAliasToLabel(string alias)
        {
            switch (alias)
            {
                case ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_1_STEP:
                    return ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP;
                case ANALYSIS_ALIAS_MULTIPLE_REPRESENTATIONS_2_STEP:
                    return ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP;
                case ANALYSIS_ALIAS_CHANGED_ANSWER_AFTER_REPRESENTATION:
                    return ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION;
                case ANALYSIS_ALIAS_ANSWER_BEFORE_REPRESENTATION:
                    return ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION;
                case ANALYSIS_ALIAS_REPRESENTATION_AFTER_ANSWER:
                    return ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER;
                case ANALYSIS_ALIAS_REPRESENTATIONS_USED:
                    return ANALYSIS_LABEL_REPRESENTATIONS_USED;
                case ANALYSIS_ALIAS_ARRAY_SKIP_COUNTING:
                    return ANALYSIS_LABEL_ARRAY_SKIP_COUNTING;
                case ANALYSIS_ALIAS_FILL_IN_ANSWER_CORRECTNESS:
                    return ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS;
                case ANALYSIS_SHORT_NAME_PROBLEM_TYPE:
                    return ANALYSIS_LABEL_PROBLEM_TYPE;
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
                           ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS,
                       //ANALYSIS_LABEL_PROBLEM_TYPE
                       };

            return list;
        }

        public static List<string> GetAllAnalysisAliases()
        {
            var labels = GetAllAnalysisLabels();
            var aliases = labels.Select(AnalysisLabelToShortName).ToList();

            return aliases;
        }

        public static string ConstraintLabelToShortName(string label)
        {
            switch (label)
            {
                case CONSTRAINT_ANSWER_CHANGE:
                    return "Answer Change";
                case CONSTRAINT_ANSWER_TYPE:
                    return "Answer Type";
                case CONSTRAINT_ANSWER_CORRECTNESS:
                    return "Correctness";
                case CONSTRAINT_ANSWER_OBJECT:
                    return "Answer Object";
                case CONSTRAINT_REPRESENTATION_CORRECTNESS:
                    return "Correctness";
                case CONSTRAINT_REPRESENTATION_NAME:
                    return "Type";
                case CONSTRAINT_REPRESENTATION_CODED_ID:
                    return "ID";
                case CONSTRAINT_HISTORY_STATUS:
                    return "Status";
            }

            return "No matching Alias";
        }

        #endregion // Methods
    }
}
