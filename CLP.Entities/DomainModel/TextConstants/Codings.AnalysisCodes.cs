﻿using System.Collections.Generic;
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
        public const string ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS = "FILL IN ANSWER CORRECTNESS";
        public const string ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP = "MULTIPLE REPRESENTATIONS - 1-STEP";
        public const string ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP = "MULTIPLE REPRESENTATIONS - 2-STEP";
        
        public const string ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER = "REPRESENTATION AFTER ANSWER";
        public const string ANALYSIS_LABEL_ARRAY_SKIP_COUNTING = "ARRAY SKIP COUNTING";
        public const string ANALYSIS_LABEL_PROBLEM_TYPE = "PROBLEM TYPE";

        #endregion // Analysis Labels

        #region Analysis Short Names

        public const string ANALYSIS_SHORT_NAME_REPRESENTATIONS_USED = "REPS_USED";
        public const string ANALYSIS_SHORT_NAME_REPRESENTATION_ORDER = "REP_ORDER";
        public const string ANALYSIS_SHORT_NAME_ANSWER_BEFORE_REPRESENTATION = "ABR";
        public const string ANALYSIS_SHORT_NAME_CHANGED_ANSWER_AFTER_REPRESENTATION = "CAAR";
        public const string ANALYSIS_SHORT_NAME_FILL_IN_ANSWER_CORRECTNESS = "ANS";
        public const string ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_1_STEP = "MR";
        public const string ANALYSIS_SHORT_NAME_MULTIPLE_REPRESENTATIONS_2_STEP = "MR2STEP";
        
        public const string ANALYSIS_SHORT_NAME_REPRESENTATION_AFTER_ANSWER = "RAA";
        public const string ANALYSIS_SHORT_NAME_ARRAY_SKIP_COUNTING = "SKIP";
        public const string ANALYSIS_SHORT_NAME_PROBLEM_TYPE = "PROBLEM";

        #endregion // Analysis Short Names

        #region Analysis Constraint Labels

        public const string CONSTRAINT_REPRESENTATION_NAME_LAX = "REPRESENTATION_NAME_LAX";
        public const string CONSTRAINT_REPRESENTATION_CORRECTNESS = "REPRESENTATION_CORRECTNESS";
        public const string CONSTRAINT_REPRESENTATION_CODED_ID = "CODED_ID";
        public const string CONSTRAINT_REPRESENTATION_FIRST = "REPRESENTATION_NAME_FIRST";
        public const string CONSTRAINT_REPRESENTATION_LAST = "REPRESENTATION_NAME_LAST";

        public const string CONSTRAINT_HISTORY_STATUS = "HISTORY_STATUS";

        public const string CONSTRAINT_ANSWER_TYPE = "ANSWER_TYPE";
        public const string CONSTRAINT_ANSWER_CORRECTNESS = "ANSWER_CORRECTNESS";
        public const string CONSTRAINT_ANSWER_CHANGE_FROM = "ANSWER_CHANGE_FROM";
        public const string CONSTRAINT_ANSWER_CHANGE_TO = "ANSWER_CHANGE_TO";
        public const string CONSTRAINT_ANSWER_OBJECT = "ANSWER_OBJECT";
        public const string CONSTRAINT_ANSWER_CORRECT_ANSWER = "CORRECT_ANSWER";
        public const string CONSTRAINT_ANSWER_STUDENT_ANSWER = "STUDENT_ANSWER";

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
                case ANALYSIS_LABEL_ARRAY_SKIP_COUNTING:
                    return ANALYSIS_SHORT_NAME_ARRAY_SKIP_COUNTING;
                case ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS:
                    return ANALYSIS_SHORT_NAME_FILL_IN_ANSWER_CORRECTNESS;
                case ANALYSIS_LABEL_PROBLEM_TYPE:
                    return ANALYSIS_SHORT_NAME_PROBLEM_TYPE;
                case ANALYSIS_LABEL_REPRESENTATION_ORDER:
                    return ANALYSIS_SHORT_NAME_REPRESENTATION_ORDER;
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
                case ANALYSIS_SHORT_NAME_ARRAY_SKIP_COUNTING:
                    return ANALYSIS_LABEL_ARRAY_SKIP_COUNTING;
                case ANALYSIS_SHORT_NAME_FILL_IN_ANSWER_CORRECTNESS:
                    return ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS;
                case ANALYSIS_SHORT_NAME_PROBLEM_TYPE:
                    return ANALYSIS_LABEL_PROBLEM_TYPE;
                case ANALYSIS_SHORT_NAME_REPRESENTATION_ORDER:
                    return ANALYSIS_LABEL_REPRESENTATION_ORDER;
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
                           //ANALYSIS_LABEL_PROBLEM_TYPE,
                           ANALYSIS_LABEL_REPRESENTATION_ORDER
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
                case CONSTRAINT_REPRESENTATION_CODED_ID:
                    return "ID";
                case CONSTRAINT_REPRESENTATION_FIRST:
                    return "First";
                case CONSTRAINT_REPRESENTATION_LAST:
                    return "Last";

                case CONSTRAINT_HISTORY_STATUS:
                    return "Status";

                case CONSTRAINT_ANSWER_TYPE:
                    return "Answer Type";
                case CONSTRAINT_ANSWER_CORRECTNESS:
                    return "Correctness";
                case CONSTRAINT_ANSWER_CHANGE_FROM:
                    return "Changed From";
                case CONSTRAINT_ANSWER_CHANGE_TO:
                    return "Changed To";
                case CONSTRAINT_ANSWER_OBJECT:
                    return "Type";
            }

            return "No matching Alias";
        }

        #endregion // Methods
    }
}
