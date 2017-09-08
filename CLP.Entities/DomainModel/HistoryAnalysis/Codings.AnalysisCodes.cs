using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLP.Entities
{
    public static partial class Codings
    {
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

        public const string ANALYSIS_ALIAS_MULTIPLE_REPRESENTATIONS_1_STEP = "MR";
        public const string ANALYSIS_ALIAS_MULTIPLE_REPRESENTATIONS_2_STEP = "MR2STEP";
        public const string ANALYSIS_ALIAS_CHANGED_ANSWER_AFTER_REPRESENTATION = "AR";
        public const string ANALYSIS_ALIAS_ANSWER_BEFORE_REPRESENTATION = "ABR";
        public const string ANALYSIS_ALIAS_REPRESENTATION_AFTER_ANSWER = "RAA";
        public const string ANALYSIS_ALIAS_REPRESENTATIONS_USED = "REPS_USED";
        public const string ANALYSIS_ALIAS_ARRAY_SKIP_COUNTING = "SKIP";
        public const string ANALYSIS_ALIAS_FILL_IN_ANSWER_CORRECTNESS = "ANS";
        public const string ANALYSIS_ALIAS_PROBLEM_TYPE = "PROBLEM";

        #endregion // Analysis Labels
    }
}
