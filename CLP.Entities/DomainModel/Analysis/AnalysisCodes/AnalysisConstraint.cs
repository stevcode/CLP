using System;
using System.Collections.Generic;
using System.Linq;
using Catel;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnalysisConstraint : ASerializableBase
    {
        #region Constructors

        public AnalysisConstraint() { }

        public AnalysisConstraint(string constraintLabel, string constraintValue = Codings.CONSTRAINT_VALUE_ANY)
        {
            ConstraintLabel = constraintLabel;
            ConstraintValue = constraintValue;
        }

        #endregion // Constructors

        #region Properties

        /// <summary>Name of the constraint.</summary>
        public string ConstraintLabel
        {
            get => GetValue<string>(ConstraintLabelProperty);
            set => SetValue(ConstraintLabelProperty, value);
        }

        public static readonly PropertyData ConstraintLabelProperty = RegisterProperty(nameof(ConstraintLabel), typeof(string), string.Empty);

        /// <summary>Value of the constraint.</summary>
        public string ConstraintValue
        {
            get => GetValue<string>(ConstraintValueProperty);
            set => SetValue(ConstraintValueProperty, value);
        }

        public static readonly PropertyData ConstraintValueProperty = RegisterProperty(nameof(ConstraintValue), typeof(string), string.Empty);

        #region Calculated Properties

        public string ConstraintShortName => Codings.ConstraintLabelToShortName(ConstraintLabel);
        public bool IsQueryable => GenerateIsQueryable(ConstraintLabel);
        public bool IsOverridingDisplayName => GenerateIsOverridingDisplayName(ConstraintLabel);
        public List<string> PossibleConstraintValues => GeneratePossibleConstraintValues(ConstraintLabel);

        #endregion // Calculated Properties

        #endregion // Properties
      
        #region Static Methods

        private bool GenerateIsQueryable(string constraintLabel)
        {
            return !(constraintLabel == Codings.CONSTRAINT_ANSWER_CORRECT_ANSWER ||
                     constraintLabel == Codings.CONSTRAINT_ANSWER_STUDENT_ANSWER ||
                     constraintLabel == Codings.CONSTRAINT_REPRESENTATION_CODED_ID ||
                     constraintLabel == Codings.CONSTRAINT_COUNT);
        }

        private bool GenerateIsOverridingDisplayName(string constraintLabel)
        {
            return constraintLabel == Codings.CONSTRAINT_REPRESENTATION_NAME_LAX ||
                   constraintLabel == Codings.CONSTRAINT_ANSWER_OBJECT;
        }

        private List<string> GeneratePossibleConstraintValues(string constraintLabel)
        {
            var possibleConstraintValues = new List<string>
                                           {
                                               Codings.CONSTRAINT_VALUE_ANY
                                           };

            var codedCorrectnessValues = Enum<Correctness>.GetValues().Select(Codings.CorrectnessToCodedCorrectness).ToList();
            // TODO: Expand
            switch (constraintLabel)
            {
                case Codings.CONSTRAINT_REPRESENTATION_NAME_LAX:
                    possibleConstraintValues.AddRange(new List<string>
                                                      {
                                                          Codings.OBJECT_ARRAY,
                                                          Codings.OBJECT_NUMBER_LINE,
                                                          Codings.OBJECT_STAMP,
                                                          Codings.OBJECT_BINS,
                                                          Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_INK_ONLY,
                                                          Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_BLANK_PAGE,
                                                          Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_NONE
                                                      });
                    break;
                case Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS:
                    possibleConstraintValues.AddRange(codedCorrectnessValues.ToList());
                    break;
                case Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON:
                    possibleConstraintValues.Add(Codings.NOT_APPLICABLE);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_GAPS_OR_OVERLAPS);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_SWAPPED);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_INCORRECT_JUMPS);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_INCORRECT_DIMENSIONS);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_INCORRECT_GROUPS);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_REPRESENTATION_CORRECTNESS_REASON_UNKNOWN);
                    break;
                case Codings.CONSTRAINT_REPRESENTATION_FIRST:
                    possibleConstraintValues.AddRange(new List<string>
                                                      {
                                                          Codings.OBJECT_ARRAY,
                                                          Codings.OBJECT_NUMBER_LINE,
                                                          Codings.OBJECT_STAMP,
                                                          Codings.OBJECT_BINS
                                                      });
                    break;
                case Codings.CONSTRAINT_REPRESENTATION_LAST:
                    possibleConstraintValues.AddRange(new List<string>
                                                      {
                                                          Codings.OBJECT_ARRAY,
                                                          Codings.OBJECT_NUMBER_LINE,
                                                          Codings.OBJECT_STAMP,
                                                          Codings.OBJECT_BINS
                                                      });
                    break;

                case Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_ALL);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_SOME);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_MULTIPLE_REPRESENTATION_CORRECTNESS_NONE);
                    break;

                case Codings.CONSTRAINT_HISTORY_STATUS:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_HISTORY_STATUS_FINAL);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_HISTORY_STATUS_DELETED);
                    break;

                case Codings.CONSTRAINT_ANSWER_TYPE:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY);
                    break;
                case Codings.CONSTRAINT_ANSWER_CORRECTNESS:
                    possibleConstraintValues.AddRange(codedCorrectnessValues.ToList());
                    break;
                case Codings.CONSTRAINT_ANSWER_MODIFICATION:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_YES);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_NO);
                    break;
                case Codings.CONSTRAINT_ANSWER_CHANGE_FROM:
                    possibleConstraintValues.AddRange(codedCorrectnessValues.ToList());
                    break;
                case Codings.CONSTRAINT_ANSWER_CHANGE_TO:
                    possibleConstraintValues.AddRange(codedCorrectnessValues.ToList());
                    break;
                case Codings.CONSTRAINT_ANSWER_OBJECT:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_ANSWER_OBJECT_MULTIPLE_CHOICE);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_ANSWER_OBJECT_FILL_IN);
                    break;

                case Codings.CONSTRAINT_ARITH_STATUS:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_ARITH_STATUS_NO_ARITH);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_ARITH_STATUS_PLUS_ARITH);
                    break;

                case Codings.CONSTRAINT_LOCATION:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_LOCATION_RIGHT);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_LOCATION_BOTTOM);
                    break;

                case Codings.CONSTRAINT_STRATEGY_CORRECTNESS:
                    possibleConstraintValues.Add(Codings.CORRECTNESS_CODED_CORRECT);
                    possibleConstraintValues.Add(Codings.CORRECTNESS_CODED_PARTIAL);
                    possibleConstraintValues.Add(Codings.CORRECTNESS_CODED_INCORRECT);
                    break;
                case Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_WRONG_DIMENSION);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_LIKELY_ARITH_ERROR);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_CORRECTNESS_REASON_UNKNOWN);
                    break;

                case Codings.CONSTRAINT_STRATEGY_TECHNIQUE:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_DIVIDE);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_INK_DIVIDE);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_TECHNIQUE_CUT_AND_SNAP);
                    break;
                case Codings.CONSTRAINT_STRATEGY_FRIENDLY_NUMBERS:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_TWO);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_FIVE);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_TEN);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_HALF);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_STRATEGY_FRIENDLY_NUMBERS_NONE);
                    break;

                case Codings.CONSTRAINT_IS_WORD_PROBLEM:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_YES);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_NO);
                    break;
                case Codings.CONSTRAINT_PROBLEM_TYPE:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_MULTIPLICATION);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_DIVISION);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_EQUIVALENCE);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_OTHER);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_PROBLEM_TYPE_NONE);
                    break;

                case Codings.CONSTRAINT_OVERALL_CORRECTNESS:
                    possibleConstraintValues.AddRange(codedCorrectnessValues.ToList());
                    break;
            }

            return possibleConstraintValues;
        }

        #endregion // Static Methods
    }
}