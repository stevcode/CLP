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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CLPPagePreviewView pagePreviewView = (((sender as Button).Parent as Grid).Parent as Grid).Children[0] as CLPPagePreviewView;
            CLPPageViewModel pageViewModel = pagePreviewView.DataContext as CLPPageViewModel;
            string pageID = pageViewModel.Page.UniqueID;
            //if (sideBarViewModel.Submissions.ContainsKey(pageID))
            //{

            //    sideBarViewModel.SubmissionPages = sideBarViewModel.Submissions[pageID];
            //}
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(IWorkspaceViewModel);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as NotebookWorkspaceViewModel).CurrentPage = NotebookPageListBox.Items[0] as CLPPageViewModel;
            //if (!NotebookPageListBox.Items.IsEmpty)
            //{
            //    NotebookPageListBox.SelectedItem = NotebookPageListBox.Items[0];
            //}
        }
    }
}
