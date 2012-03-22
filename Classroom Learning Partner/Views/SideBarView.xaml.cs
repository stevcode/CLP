using Classroom_Learning_Partner.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using Classroom_Learning_Partner.ViewModels.Workspaces;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for SideBarView.xaml
    /// </summary>
    public partial class SideBarView : Catel.Windows.Controls.UserControl
    {
        public SideBarView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CLPPagePreviewView pagePreviewView = (((sender as Button).Parent as Grid).Parent as Grid).Children[0] as CLPPagePreviewView;
            CLPPageViewModel pageViewModel = pagePreviewView.DataContext as CLPPageViewModel;
            string pageID = pageViewModel.Page.UniqueID;
            var viewModel = (this.DataContext as NotebookWorkspaceViewModel);
            if (viewModel.Notebook.Submissions.ContainsKey(pageID))
            {
                viewModel.SubmissionPages = viewModel.Notebook.Submissions[pageID];
            }
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(IWorkspaceViewModel);
        }
    }
}
