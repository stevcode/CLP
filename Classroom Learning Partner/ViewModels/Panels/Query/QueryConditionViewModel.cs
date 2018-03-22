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
            AvailableAnalysisCodes = QueryCondition.GenerateAvailableQueryConditions();
        }

        /// <summary>String to run the query on.</summary>
        public ObservableCollection<QueryCondition> AvailableAnalysisCodes
        {
            get => GetValue<ObservableCollection<QueryCondition>>(AvailableAnalysisCodesProperty);
            set => SetValue(AvailableAnalysisCodesProperty, value);
        }

        public static readonly PropertyData AvailableAnalysisCodesProperty = RegisterProperty(nameof(AvailableAnalysisCodes), typeof(ObservableCollection<QueryCondition>), () => new ObservableCollection<QueryCondition>());

        /// <summary>String to run the query on.</summary>
        public QueryCondition SelectedAnalysisCode
        {
            get => GetValue<QueryCondition>(SelectedAnalysisCodeProperty);
            set => SetValue(SelectedAnalysisCodeProperty, value);
        }

        public static readonly PropertyData SelectedAnalysisCodeProperty = RegisterProperty(nameof(SelectedAnalysisCode), typeof(QueryCondition), null);
    }
}
