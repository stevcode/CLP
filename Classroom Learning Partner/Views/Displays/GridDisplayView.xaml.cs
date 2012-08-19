using System;
using System.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;


namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for GridDisplayView.xaml
    /// </summary>
    public partial class GridDisplayView : Catel.Windows.Controls.UserControl
    {
        public GridDisplayView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(GridDisplayViewModel);
        }

        private void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            (ViewModel as GridDisplayViewModel).OnRemovePageFromGridDisplayCommandExecute((((sender as Button).Parent as Grid).Children[0] as CLPPageView).ViewModel as CLPPageViewModel);
        }
    }
}
