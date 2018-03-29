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
            AvailableAnalysisCodes = AnalysisCode.GenerateAvailableQueryConditions();
        }

        /// <summary>String to run the query on.</summary>
        public ObservableCollection<IAnalysisCode> AvailableAnalysisCodes
        {
            get => GetValue<ObservableCollection<IAnalysisCode>>(AvailableAnalysisCodesProperty);
            set => SetValue(AvailableAnalysisCodesProperty, value);
        }

        public static readonly PropertyData AvailableAnalysisCodesProperty =
            RegisterProperty(nameof(AvailableAnalysisCodes), typeof(ObservableCollection<IAnalysisCode>), () => new ObservableCollection<IAnalysisCode>());

        /// <summary>String to run the query on.</summary>
        public IAnalysisCode SelectedAnalysisCode
        {
            get => GetValue<IAnalysisCode>(SelectedAnalysisCodeProperty);
            set => SetValue(SelectedAnalysisCodeProperty, value);
        }

        public static readonly PropertyData SelectedAnalysisCodeProperty = RegisterProperty(nameof(SelectedAnalysisCode), typeof(IAnalysisCode), null);
    }
}
