using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnalysisConstraint : ASerializableBase
    {
        #region Constructors

        public AnalysisConstraint() { }

        public AnalysisConstraint(string constraintLabel, string constraintValue)
        {
            ConstraintLabel = constraintLabel;
            ConstraintValue = constraintValue;
        }

        #endregion // Constructors

        #region Properties

        public string ConstraintLabel
        {
            get => GetValue<string>(ConstraintLabelProperty);
            set => SetValue(ConstraintLabelProperty, value);
        }

        public static readonly PropertyData ConstraintLabelProperty = RegisterProperty(nameof(ConstraintLabel), typeof(string), string.Empty);

        public string ConstraintValue
        {
            get => GetValue<string>(ConstraintValueProperty);
            set => SetValue(ConstraintValueProperty, value);
        }

        public static readonly PropertyData ConstraintValueProperty = RegisterProperty(nameof(ConstraintValue), typeof(string), string.Empty);

        #endregion // Properties
    }

    [Serializable]
    public partial class AnalysisCode : ASerializableBase, IAnalysisCode
    {
        #region Constructors

        public AnalysisCode() { }

        public AnalysisCode(string analysisLabel)
        {
            AnalysisLabel = analysisLabel;
            Alias = Codings.AnalysisLabelToAlias(analysisLabel);
        }

        #endregion // Constructors

        #region Properties

        public string AnalysisLabel
        {
            get => GetValue<string>(AnalysisLabelProperty);
            set => SetValue(AnalysisLabelProperty, value);
        }

        public static readonly PropertyData AnalysisLabelProperty = RegisterProperty(nameof(AnalysisLabel), typeof(string), string.Empty);

        /// <summary>Short-form alias of the analysis code.</summary>
        public string Alias
        {
            get => GetValue<string>(AliasProperty);
            set => SetValue(AliasProperty, value);
        }

        public static readonly PropertyData AliasProperty = RegisterProperty(nameof(Alias), typeof(string), string.Empty);

        public ObservableCollection<AnalysisConstraint> ConstraintValues
        {
            get => GetValue<ObservableCollection<AnalysisConstraint>>(ConstraintValuesProperty);
            set => SetValue(ConstraintValuesProperty, value);
        }

        public static readonly PropertyData ConstraintValuesProperty =
            RegisterProperty(nameof(ConstraintValues), typeof(ObservableCollection<AnalysisConstraint>), () => new ObservableCollection<AnalysisConstraint>());

        public string FormattedValue
        {
            get
            {
                var constraintValues = string.Join(" - ", ConstraintValues.Select(c => c.ConstraintValue).ToList());
                var bracesString = ConstraintValues.Any() ? $" {{{constraintValues}}}" : string.Empty;

                return $"{Alias}{bracesString}";
            }
        }

        public List<string> ConstraintLabels
        {
            get
            {
                return ConstraintValues.Select(c => c.ConstraintLabel).ToList();
            }
        }

        #endregion // Properties

        #region Methods

        public void AddConstraint(string constraintLabel, string constraintValue)
        {
            ConstraintValues.Add(new AnalysisConstraint(constraintLabel, constraintValue));
        }

        #endregion // Methods
    }
}