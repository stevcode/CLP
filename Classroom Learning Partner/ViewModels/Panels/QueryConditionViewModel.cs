using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class QueryConditionViewModel : ViewModelBase
    {
        public QueryConditionViewModel()
        {
            AvailableAnalysisCodes = ConditionScaffold.GenerateAvailableConditions();
        }

        /// <summary>String to run the query on.</summary>
        public ObservableCollection<ConditionScaffold> AvailableAnalysisCodes
        {
            get => GetValue<ObservableCollection<ConditionScaffold>>(AvailableAnalysisCodesProperty);
            set => SetValue(AvailableAnalysisCodesProperty, value);
        }

        public static readonly PropertyData AvailableAnalysisCodesProperty = RegisterProperty(nameof(AvailableAnalysisCodes), typeof(ObservableCollection<ConditionScaffold>), () => new ObservableCollection<ConditionScaffold>());

        /// <summary>String to run the query on.</summary>
        public ConditionScaffold SelectedAnalysisCode
        {
            get => GetValue<ConditionScaffold>(SelectedAnalysisCodeProperty);
            set => SetValue(SelectedAnalysisCodeProperty, value);
        }

        public static readonly PropertyData SelectedAnalysisCodeProperty = RegisterProperty(nameof(SelectedAnalysisCode), typeof(ConditionScaffold), null);
    }
}
