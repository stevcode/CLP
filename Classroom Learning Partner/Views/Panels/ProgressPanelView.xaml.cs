using System;
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
            for(int i = 1; i < 20; i++)
            {
                var column = new DataGridTextColumn();
                column.Header = i;
                ProgressDataGrid.Columns.Add(column);
            }
        }
        protected override Type GetViewModelType()
        {
            return typeof(ProgressPanelViewModel);
        }
    }
}
