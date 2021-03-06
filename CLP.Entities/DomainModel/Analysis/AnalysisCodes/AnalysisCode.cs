﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public partial class AnalysisCode : ASerializableBase, IAnalysisCode, IQueryPart
    {
        #region Constructors

        public AnalysisCode() { }

        public AnalysisCode(string analysisCodeLabel)
        {
            AnalysisCodeLabel = analysisCodeLabel;
        }

        #endregion // Constructors

        #region IAnalysisCode Implementation

        public string AnalysisCodeLabel
        {
            get => GetValue<string>(AnalysisCodeLabelProperty);
            set => SetValue(AnalysisCodeLabelProperty, value);
        }

        public static readonly PropertyData AnalysisCodeLabelProperty = RegisterProperty(nameof(AnalysisCodeLabel), typeof(string), string.Empty);

        public ObservableCollection<AnalysisConstraint> Constraints
        {
            get => GetValue<ObservableCollection<AnalysisConstraint>>(ConstraintsProperty);
            set => SetValue(ConstraintsProperty, value);
        }

        public static readonly PropertyData ConstraintsProperty =
            RegisterProperty(nameof(Constraints), typeof(ObservableCollection<AnalysisConstraint>), () => new ObservableCollection<AnalysisConstraint>());

        public string AnalysisCodeName => Codings.AnalysisLabelToShortName(AnalysisCodeLabel);
        public string AnalysisCodeShortName => Codings.AnalysisLabelToShortName(AnalysisCodeLabel);
        public List<string> ConstraintLabels => Constraints.Select(c => c.ConstraintLabel).ToList();

        public string FormattedValue
        {
            get
            {
                var constraintValues = string.Join(" - ", Constraints.Select(c => c.ConstraintValue).ToList());
                var bracesString = Constraints.Any() ? $" {{{constraintValues}}}" : string.Empty;

                return $"{AnalysisCodeShortName}{bracesString}";
            }
        }

        public void AddConstraint(string constraintLabel, string constraintValue = Codings.CONSTRAINT_VALUE_ANY)
        {
            Constraints.Add(new AnalysisConstraint(constraintLabel, constraintValue));
        }

        #endregion // IAnalysisCode Implementation

        #region IQueryPart Implementation

        public string LongFormattedValue
        {
            get
            {
                var overridingConstraint = Constraints.FirstOrDefault(c => c.IsOverridingDisplayName && c.ConstraintValue != Codings.CONSTRAINT_VALUE_ANY);
                var mainName = overridingConstraint == null ? AnalysisCodeShortName : overridingConstraint.ConstraintValue;

                var normalConstraints = Constraints.Where(c => !(c.IsOverridingDisplayName && c.ConstraintValue != Codings.CONSTRAINT_VALUE_ANY)).ToList();
                var formattedConstraintValues = string.Join(" & ", normalConstraints.Where(c => c.ConstraintValue != Codings.CONSTRAINT_VALUE_ANY || c.IsOverridingDisplayName).Select(c => c.ConstraintValue));
                if (!string.IsNullOrWhiteSpace(formattedConstraintValues))
                {
                    formattedConstraintValues = $": {formattedConstraintValues}";
                }
                return $"{mainName}{formattedConstraintValues}";
            }
        }

        public string ButtonFormattedValue
        {
            get
            {
                var overridingConstraint = Constraints.FirstOrDefault(c => c.IsOverridingDisplayName);
                var mainName = overridingConstraint == null ? AnalysisCodeShortName : overridingConstraint.ConstraintValue;

                var normalConstraints = Constraints.Where(c => !c.IsOverridingDisplayName).ToList();
                return $"{mainName}\n{string.Join("\n", normalConstraints)}";
            }
        }

        #endregion // IQueryPart Implementation

        #region Static Methods

        public static ObservableCollection<IAnalysisCode> GenerateAvailableQueryConditions()
        {
            var conditions = new ObservableCollection<IAnalysisCode>();

            var repsUsed = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED);
            repsUsed.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME_LAX);
            repsUsed.AddConstraint(Codings.CONSTRAINT_HISTORY_STATUS);
            repsUsed.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS);
            repsUsed.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS_REASON);
            conditions.Add(repsUsed);

            var repsUsedSummary = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED_SUMMARY);
            repsUsedSummary.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_OVERALL_CORRECTNESS);
            repsUsedSummary.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_COUNT);
            repsUsedSummary.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_DELETED_COUNT);
            repsUsedSummary.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_FINAL_COUNT);
            conditions.Add(repsUsedSummary);

            var repsDeletedSummary = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATIONS_DELETED_SUMMARY);
            repsDeletedSummary.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_OVERALL_CORRECTNESS);
            conditions.Add(repsDeletedSummary);

            var repOrder = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATION_ORDER);
            repOrder.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_FIRST);
            repOrder.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_LAST);
            conditions.Add(repOrder);

            var abr = new AnalysisCode(Codings.ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION);
            abr.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE);    // Only useful for large cache
            abr.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS);
            conditions.Add(abr);

            var caar = new AnalysisCode(Codings.ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION);
            caar.AddConstraint(Codings.CONSTRAINT_ANSWER_CHANGE_FROM);
            caar.AddConstraint(Codings.CONSTRAINT_ANSWER_CHANGE_TO);
            conditions.Add(caar);

            var finalAnswerCorrectness = new AnalysisCode(Codings.ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS);
            finalAnswerCorrectness.AddConstraint(Codings.CONSTRAINT_ANSWER_OBJECT);
            finalAnswerCorrectness.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS);
            finalAnswerCorrectness.AddConstraint(Codings.CONSTRAINT_ANSWER_MODIFICATION);
            conditions.Add(finalAnswerCorrectness);

            var finalRepresentationCorrectness = new AnalysisCode(Codings.ANALYSIS_LABEL_FINAL_REPRESENTATION_CORRECTNESS);
            finalRepresentationCorrectness.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS);
            conditions.Add(finalRepresentationCorrectness);

            var overallCorrectness = new AnalysisCode(Codings.ANALYSIS_LABEL_OVERALL_CORRECTNESS);
            overallCorrectness.AddConstraint(Codings.CONSTRAINT_OVERALL_CORRECTNESS);
            conditions.Add(overallCorrectness);

            var mr = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP);
            mr.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_MATCHED_STEP);
            mr.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS);
            conditions.Add(mr);

            var mr2step = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP);
            mr2step.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_CORRECTNESS);
            conditions.Add(mr2step);

            var ma = new AnalysisCode(Codings.ANALYSIS_LABEL_MULTIPLE_APPROACHES);
            ma.AddConstraint(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_MATCHED_STEP);
            ma.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME);
            conditions.Add(ma);

            var arrEQN = new AnalysisCode(Codings.ANALYSIS_LABEL_ARRAY_EQUATION);
            conditions.Add(arrEQN);

            var arrSkip = new AnalysisCode(Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP);
            arrSkip.AddConstraint(Codings.CONSTRAINT_ARITH_STATUS);
            arrSkip.AddConstraint(Codings.CONSTRAINT_LOCATION);
            arrSkip.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS);
            arrSkip.AddConstraint(Codings.CONSTRAINT_STRATEGY_CORRECTNESS_REASON);
            conditions.Add(arrSkip);

            var partialProduct = new AnalysisCode(Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_PARTIAL_PRODUCT);
            partialProduct.AddConstraint(Codings.CONSTRAINT_STRATEGY_TECHNIQUE);
            partialProduct.AddConstraint(Codings.CONSTRAINT_STRATEGY_FRIENDLY_NUMBERS);
            conditions.Add(partialProduct);

            var wordProblem = new AnalysisCode(Codings.ANALYSIS_LABEL_WORD_PROBLEM);
            wordProblem.AddConstraint(Codings.CONSTRAINT_IS_WORD_PROBLEM);
            conditions.Add(wordProblem);

            var pageDef = new AnalysisCode(Codings.ANALYSIS_LABEL_PAGE_DEFINITION);
            pageDef.AddConstraint(Codings.CONSTRAINT_PROBLEM_TYPE);
            pageDef.AddConstraint(Codings.CONSTRAINT_PROBLEM_STEP_COUNT);
            conditions.Add(pageDef);

            var nlje = new AnalysisCode(Codings.ANALYSIS_LABEL_NUMBER_LINE_JUMP_ERASURES);
            conditions.Add(nlje);

            var skipConsolidation = new AnalysisCode(Codings.ANALYSIS_LABEL_SKIP_CONSOLIDATION);
            skipConsolidation.AddConstraint(Codings.CONSTRAINT_ANY_ARITH);
            conditions.Add(skipConsolidation);

            var incorrectReasons = new AnalysisCode(Codings.ANALYSIS_LABEL_WRONG_GROUPS);
            incorrectReasons.AddConstraint(Codings.CONSTRAINT_DELETED_HAS_WRONG_GROUPS);
            incorrectReasons.AddConstraint(Codings.CONSTRAINT_FINAL_HAS_WRONG_GROUPS);
            conditions.Add(incorrectReasons);

            return conditions;
        }

        #endregion // Static Methods
    }
}
