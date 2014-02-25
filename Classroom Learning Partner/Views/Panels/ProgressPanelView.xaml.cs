using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for ProgressPanelView.xaml
    /// </summary>
    public partial class ProgressPanelView
    {
        public ProgressPanelView()
        {
            InitializeComponent();
            //DataContext is null here.  Too early.  Do something with bindings and events
            //to get actual page numbers?  Or put them elsewhere.
            for(int i = 1; i < 12; i++ )
            {
                var column = new DataGridTemplateColumn();
                column.Header = i;
                column.CellTemplate = (DataTemplate)Resources["ButtonBox"];
                ProgressDataGrid.Columns.Add(column);
            }
        }

        protected override Type GetViewModelType()
        {
            return typeof(ProgressPanelViewModel);
        }
    }
}
