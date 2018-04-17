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
                     constraintLabel == Codings.CONSTRAINT_REPRESENTATION_CODED_ID);
        }

        private bool GenerateIsOverridingDisplayName(string constraintLabel)
        {
            return constraintLabel == Codings.CONSTRAINT_REPRESENTATION_NAME ||
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
                case Codings.CONSTRAINT_ANSWER_CHANGE:
                    possibleConstraintValues.AddRange(from fromCorrectness in codedCorrectnessValues
                                                      from toCorrectness in codedCorrectnessValues
                                                      select $"{fromCorrectness}{Codings.CONSTRAINT_VALUE_ANSWER_CHANGE_DELIMITER}{toCorrectness}");
                    break;
                case Codings.CONSTRAINT_ANSWER_CORRECTNESS:
                    possibleConstraintValues.AddRange(codedCorrectnessValues.ToList());
                    break;
                case Codings.CONSTRAINT_ANSWER_OBJECT:
                    possibleConstraintValues.Add(Codings.FriendlyObjects[Codings.OBJECT_MULTIPLE_CHOICE]);
                    possibleConstraintValues.Add(Codings.FriendlyObjects[Codings.OBJECT_FILL_IN]);
                    break;
                case Codings.CONSTRAINT_ANSWER_TYPE:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY);                   
                    break;
                case Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS:
                    possibleConstraintValues.AddRange(codedCorrectnessValues.ToList());
                    break;
                case Codings.CONSTRAINT_HISTORY_STATUS:
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_HISTORY_STATUS_FINAL);
                    possibleConstraintValues.Add(Codings.CONSTRAINT_VALUE_HISTORY_STATUS_DELETED);
                    break;
                case Codings.CONSTRAINT_REPRESENTATION_NAME:
                    possibleConstraintValues.AddRange(new List<string>
                                                      {
                                                          Codings.OBJECT_ARRAY,
                                                          Codings.OBJECT_NUMBER_LINE,
                                                          Codings.OBJECT_STAMP,
                                                          Codings.OBJECT_BINS,
                                                          Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_INK_ONLY,
                                                          Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_BLANK_PAGE
                                                      });
                    break;
            }

            return possibleConstraintValues;
        }

        #endregion // Static Methods
    }
}