using System.Collections.ObjectModel;
using System.Windows;
using Catel.MVVM.Views;
using Catel.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPAggregationDataTableView.xaml.
    /// </summary>
    public partial class CLPAggregationDataTableView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPAggregationDataTableView"/> class.
        /// </summary>
        public CLPAggregationDataTableView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPAggregationDataTableViewModel);
        }

        //[ViewToViewModel(MappingType=ViewToViewModelMappingType.ViewModelToView)]
        //public ObservableCollection<CLPGridPart> Rows
        //{
        //    get { return (ObservableCollection<CLPGridPart>)GetValue(RowsProperty); }
        //    set { SetValue(RowsProperty, value); }
        //}

        //public static readonly DependencyProperty RowsProperty =
        //    DependencyProperty.Register("Rows", typeof(ObservableCollection<CLPGridPart>), typeof(CLPAggregationDataTableView));

        //[ViewToViewModel(MappingType = ViewToViewModelMappingType.ViewModelToView)]
        //public ObservableCollection<CLPGridPart> Columns
        //{
        //    get { return (ObservableCollection<CLPGridPart>)GetValue(ColumnsProperty); }
        //    set { SetValue(ColumnsProperty, value); }
        //}

        //public static readonly DependencyProperty ColumnsProperty =
        //    DependencyProperty.Register("Columns", typeof(ObservableCollection<CLPGridPart>), typeof(CLPAggregationDataTableView));
    }
}
