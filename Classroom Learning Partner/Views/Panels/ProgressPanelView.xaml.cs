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
            //to get actual page numbers?

            //If I could access view things directly from the viewmodel, I could just put
            //code much like the below right after pagenumlist gets generated.  But not allowed.

            //If I knew how events worked, I could maybe make a binding to PageNumList and 
            //regenerate the columns whenever it changed?

            //If I could make arbitrarily many fields in StudentProgressInfo, I could have a 
            //field for each page.  I could maybe even make a placeholder field and a list of
            //pages, and generate the columns when it tries to add the placeholder field?
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
