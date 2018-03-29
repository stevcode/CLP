using System.Windows;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    public partial class QuerySelectorView
    {
        public QuerySelectorView()
        {
            InitializeComponent();
        }

        static QuerySelectorView()
        {
            typeof(QuerySelectorView).AutoDetectViewPropertiesToSubscribe();
        }

        #region Dependency Properties

        [ViewToViewModel("ConditionPlace", MappingType = ViewToViewModelMappingType.ViewToViewModel)]
        public ConditionPlaces ConditionPlace
        {
            get => (ConditionPlaces)GetValue(ConditionPlaceProperty);
            set => SetValue(ConditionPlaceProperty, value);
        }

        public static readonly DependencyProperty ConditionPlaceProperty =
            DependencyProperty.Register(nameof(ConditionPlace), typeof(ConditionPlaces), typeof(QuerySelectorView), new FrameworkPropertyMetadata());

        #endregion // Dependency Properties
    }
}