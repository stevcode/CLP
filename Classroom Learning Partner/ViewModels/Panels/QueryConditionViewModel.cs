using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
{
    public class QueryConditionViewModel : ViewModelBase
    {
        /// <summary>String to run the query on.</summary>
        public ObservableCollection<string> AvailableAnalysisCodes
        {
            get => GetValue<ObservableCollection<string>>(AvailableAnalysisCodesProperty);
            set => SetValue(AvailableAnalysisCodesProperty, value);
        }

        public static readonly PropertyData AvailableAnalysisCodesProperty = RegisterProperty(nameof(AvailableAnalysisCodes), typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>String to run the query on.</summary>
        public string SelectedAnalysisCode
        {
            get => GetValue<string>(SelectedAnalysisCodeProperty);
            set => SetValue(SelectedAnalysisCodeProperty, value);
        }

        public static readonly PropertyData SelectedAnalysisCodeProperty = RegisterProperty(nameof(SelectedAnalysisCode), typeof(string), string.Empty);
    }
}
