using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Catel;
using Catel.Data;

namespace CLP.Entities
{
    public class ConstraintScaffold : ModelBase
    {
        public ConstraintScaffold()
        {
        }

        public ConstraintScaffold(string constraintName)
        {
            ConstraintName = constraintName;
        }

        public ConstraintScaffold(string constraintName, List<string> constraintValues)
        {
            ConstraintName = constraintName;
            ConstraintValues = constraintValues;
        }

        /// <summary>Determines if the constraint shows up in the Query UI.</summary>
        public bool IsQueryable
        {
            get => GetValue<bool>(IsQueryableProperty);
            set => SetValue(IsQueryableProperty, value);
        }

        public static readonly PropertyData IsQueryableProperty = RegisterProperty(nameof(IsQueryable), typeof(bool), true);

        /// <summary>Name of the constraint.</summary>
        public string ConstraintName
        {
            get => GetValue<string>(ConstraintNameProperty);
            set => SetValue(ConstraintNameProperty, value);
        }

        public static readonly PropertyData ConstraintNameProperty = RegisterProperty(nameof(ConstraintName), typeof(string), string.Empty);

        /// <summary>Name of the constraint.</summary>
        public string SelectedConstraintValue
        {
            get => GetValue<string>(SelectedConstraintValueProperty);
            set => SetValue(SelectedConstraintValueProperty, value);
        }

        public static readonly PropertyData SelectedConstraintValueProperty = RegisterProperty(nameof(SelectedConstraintValue), typeof(string), string.Empty);

        /// <summary>Allowable values for the constraint.</summary>
        public List<string> ConstraintValues
        {
            get => GetValue<List<string>>(ConstraintValuesProperty);
            set => SetValue(ConstraintValuesProperty, value);
        }

        public static readonly PropertyData ConstraintValuesProperty = RegisterProperty(nameof(ConstraintValues),
                                                                                        typeof(List<string>),
                                                                                        () =>
                                                                                        new List<string>());
    }

    public class ConditionScaffold : ModelBase
    {
        public ConditionScaffold()
        {
        }

        public ConditionScaffold(string analysisCodeLabel, string analysisCodeAlias)
        {
            AnalysisCodeLabel = analysisCodeLabel;
            AnalysisCodeAlias = analysisCodeAlias;
        }

        /// <summary>Full name of the scaffolded analysis code.</summary>
        public string AnalysisCodeLabel
        {
            get => GetValue<string>(AnalysisCodeLabelProperty);
            set => SetValue(AnalysisCodeLabelProperty, value);
        }

        public static readonly PropertyData AnalysisCodeLabelProperty = RegisterProperty(nameof(AnalysisCodeLabel), typeof(string), string.Empty);

        /// <summary>Alias of the scaffolded analysis code.</summary>
        public string AnalysisCodeAlias
        {
            get => GetValue<string>(AnalysisCodeAliasProperty);
            set => SetValue(AnalysisCodeAliasProperty, value);
        }

        public static readonly PropertyData AnalysisCodeAliasProperty = RegisterProperty(nameof(AnalysisCodeAlias), typeof(string), string.Empty);

        /// <summary>Allowable values for the constraint.</summary>
        public List<ConstraintScaffold> Constraints
        {
            get => GetValue<List<ConstraintScaffold>>(ConstraintsProperty);
            set => SetValue(ConstraintsProperty, value);
        }

        public static readonly PropertyData ConstraintsProperty = RegisterProperty(nameof(Constraints),
                                                                                   typeof(List<ConstraintScaffold>),
                                                                                   () =>
                                                                                   new List<ConstraintScaffold>());

        public static ObservableCollection<ConditionScaffold> GenerateAvailableConditions()
        {
            var conditions = new ObservableCollection<ConditionScaffold>();

            var repsUsed = new ConditionScaffold(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.ANALYSIS_ALIAS_REPRESENTATIONS_USED);
            repsUsed.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_REPRESENTATION_NAME, new List<string>
                                                {
                                                    Codings.CONSTRAINT_VALUE_ANY,
                                                    Codings.OBJECT_ARRAY,
                                                    Codings.OBJECT_NUMBER_LINE,
                                                    Codings.OBJECT_STAMP,
                                                    Codings.OBJECT_BINS,
                                                    Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_INK_ONLY,
                                                    Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_BLANK_PAGE
                                                }));

            repsUsed.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_HISTORY_STATUS, new List<string>
                                                {
                                                    Codings.CONSTRAINT_VALUE_ANY,
                                                    Codings.CONSTRAINT_VALUE_HISTORY_STATUS_FINAL,
                                                    Codings.CONSTRAINT_VALUE_HISTORY_STATUS_DELETED
                                                }));

            repsUsed.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, new List<string>
                                                {
                                                    Codings.CONSTRAINT_VALUE_ANY,
                                                    Codings.CORRECTNESS_CORRECT,
                                                    Codings.CORRECTNESS_PARTIAL,
                                                    Codings.CORRECTNESS_INCORRECT,
                                                    Codings.CORRECTNESS_ILLEGIBLE,
                                                    Codings.CORRECTNESS_UNANSWERED,
                                                    Codings.CORRECTNESS_UNKNOWN
                                                }));

            conditions.Add(repsUsed);


            var answerChangedConstraintValues = new List<string>
                                           {
                                               Codings.CONSTRAINT_VALUE_ANY
                                           };
            var codedCorrectnessValues = Enum<Correctness>.GetValues().Select(Codings.CorrectnessToCodedCorrectness).ToList();
            answerChangedConstraintValues.AddRange(from fromCorrectness in codedCorrectnessValues
                                                    from toCorrectness in codedCorrectnessValues
                                                    select $"{fromCorrectness}{Codings.CONSTRAINT_VALUE_ANSWER_CHANGE_DELIMITER}{toCorrectness}");

            var abr = new ConditionScaffold(Codings.ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION, Codings.ANALYSIS_ALIAS_ANSWER_BEFORE_REPRESENTATION);
            //abr.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_ANSWER_CHANGE, answerChangedConstraintValues.ToList()));
            abr.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_ANSWER_TYPE,
                                                       new List<string>
                                                       {
                                                           Codings.CONSTRAINT_VALUE_ANY,
                                                           Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL,
                                                           Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY
                                                       }));
            abr.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_ANSWER_CORRECTNESS,
                                                       new List<string>
                                                       {
                                                           Codings.CONSTRAINT_VALUE_ANY,
                                                           Codings.CORRECTNESS_CORRECT,
                                                           Codings.CORRECTNESS_PARTIAL,
                                                           Codings.CORRECTNESS_INCORRECT,
                                                           Codings.CORRECTNESS_ILLEGIBLE,
                                                           Codings.CORRECTNESS_UNANSWERED,
                                                           Codings.CORRECTNESS_UNKNOWN
                                                       }));
            conditions.Add(abr);

            var raa = new ConditionScaffold(Codings.ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER, Codings.ANALYSIS_ALIAS_REPRESENTATION_AFTER_ANSWER);
            //raa.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_ANSWER_CHANGE, answerChangedConstraintValues.ToList()));
            raa.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_ANSWER_TYPE,
                                                       new List<string>
                                                       {
                                                           Codings.CONSTRAINT_VALUE_ANY,
                                                           Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL,
                                                           Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY
                                                       }));
            raa.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_ANSWER_CORRECTNESS,
                                                       new List<string>
                                                       {
                                                           Codings.CONSTRAINT_VALUE_ANY,
                                                           Codings.CORRECTNESS_CORRECT,
                                                           Codings.CORRECTNESS_PARTIAL,
                                                           Codings.CORRECTNESS_INCORRECT,
                                                           Codings.CORRECTNESS_ILLEGIBLE,
                                                           Codings.CORRECTNESS_UNANSWERED,
                                                           Codings.CORRECTNESS_UNKNOWN
                                                       }));
            conditions.Add(raa);

            var caar = new ConditionScaffold(Codings.ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION, Codings.ANALYSIS_ALIAS_CHANGED_ANSWER_AFTER_REPRESENTATION);
            caar.Constraints.Add(new ConstraintScaffold(Codings.CONSTRAINT_ANSWER_CHANGE, answerChangedConstraintValues.ToList()));
            conditions.Add(caar);

            return conditions;
        }
    }
}
