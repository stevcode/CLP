using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class QueryCondition : ASerializableBase, IQueryPart
    {
        #region Constructors

        public QueryCondition() { }

        public QueryCondition(string analysisCodeLabel)
        {
            AnalysisCodeLabel = analysisCodeLabel;
        }

        #endregion // Constructors

        #region Properties

        public string AnalysisCodeLabel
        {
            get => GetValue<string>(AnalysisCodeLabelProperty);
            set => SetValue(AnalysisCodeLabelProperty, value);
        }

        public static readonly PropertyData AnalysisCodeLabelProperty = RegisterProperty(nameof(AnalysisCodeLabel), typeof(string), string.Empty);

        public List<QueryConstraint> Constraints
        {
            get => GetValue<List<QueryConstraint>>(ConstraintsProperty);
            set => SetValue(ConstraintsProperty, value);
        }

        public static readonly PropertyData ConstraintsProperty = RegisterProperty(nameof(Constraints), typeof(List<QueryConstraint>), () => new List<QueryConstraint>());

        #region Calculated Properties

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

        #endregion // Calculated Properties

        #endregion // Properties

        #region Methods

        public void AddConstraint(string constraintLabel, string constraintValue = Codings.CONSTRAINT_VALUE_ANY)
        {
            Constraints.Add(new QueryConstraint(constraintLabel, constraintValue));
        }

        #endregion // Methods

        #region IQueryPart Implementation

        public string LongFormattedValue
        {
            get
            {
                var overridingConstraint = Constraints.FirstOrDefault(c => c.IsOverridingDisplayName);
                var mainName = overridingConstraint == null ? AnalysisCodeShortName : overridingConstraint.ConstraintValue;

                var normalConstraints = Constraints.Where(c => !c.IsOverridingDisplayName).ToList();
                return $"{mainName}: {string.Join(" - ", normalConstraints)}";
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

        public static ObservableCollection<QueryCondition> GenerateAvailableQueryConditions()
        {
            var conditions = new ObservableCollection<QueryCondition>();

            var repsUsed = new QueryCondition(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED);
            repsUsed.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_NAME);
            repsUsed.AddConstraint(Codings.CONSTRAINT_HISTORY_STATUS);
            repsUsed.AddConstraint(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS);
            conditions.Add(repsUsed);

            var abr = new QueryCondition(Codings.ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION);
            //abr.Constraints.Add(new QueryConstraint(Codings.CONSTRAINT_ANSWER_CHANGE));
            abr.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE);
            abr.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS);
            conditions.Add(abr);

            var raa = new QueryCondition(Codings.ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER);
            //raa.Constraints.Add(new QueryConstraint(Codings.CONSTRAINT_ANSWER_CHANGE));
            raa.AddConstraint(Codings.CONSTRAINT_ANSWER_TYPE);
            raa.AddConstraint(Codings.CONSTRAINT_ANSWER_CORRECTNESS);
            conditions.Add(raa);

            var caar = new QueryCondition(Codings.ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION);
            caar.AddConstraint(Codings.CONSTRAINT_ANSWER_CHANGE);
            conditions.Add(caar);

            return conditions;
        }

        #endregion // Static Methods
    }
}
