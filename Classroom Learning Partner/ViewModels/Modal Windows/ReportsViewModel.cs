using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ReportsViewModel : ViewModelBase
    {
        #region Constructor

        public ReportsViewModel(QueryService.Report report)
        {
            Report = report;
        }

        #endregion // Constructor

        #region Model

        [Model]
        public QueryService.Report Report
        {
            get => GetValue<QueryService.Report>(ReportProperty);
            set => SetValue(ReportProperty, value);
        }

        public static readonly PropertyData ReportProperty = RegisterProperty(nameof(Report), typeof(QueryService.Report));

        #endregion // Model
    }
}