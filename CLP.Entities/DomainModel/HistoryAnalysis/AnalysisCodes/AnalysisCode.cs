using System;
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

        #region Properties

        public string Hack
        {
            set => RaisePropertyChanged(nameof(LongFormattedValue));
        }

        #endregion // Properties

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
            repsUsed.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME);
            repsUsed.AddConstraint(Codings.CONSTRAINT_HISTORY_STATUS);
            repsUsed.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS);
            conditions.Add(repsUsed);

            var abr = new AnalysisCode(Codings.ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION);
            //abr.Constraints.Add(new AnalysisConstraint(Codings.CONSTRAINT_ANSWER_CHANGE));
            abr.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE);
            abr.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS);
            conditions.Add(abr);

            var raa = new AnalysisCode(Codings.ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER);
            //raa.Constraints.Add(new AnalysisConstraint(Codings.CONSTRAINT_ANSWER_CHANGE));
            raa.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE);
            raa.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS);
            conditions.Add(raa);

            var caar = new AnalysisCode(Codings.ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION);
            caar.AddConstraint(Codings.CONSTRAINT_ANSWER_CHANGE);
            conditions.Add(caar);

            return conditions;
        }

        #endregion // Static Methods
    }
}
