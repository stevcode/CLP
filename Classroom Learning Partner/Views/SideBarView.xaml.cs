using Classroom_Learning_Partner.ViewModels;
using Catel.Windows.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for SideBarView.xaml
    /// </summary>
    public partial class SideBarView : UserControl<SideBarViewModel>
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
            SideBarViewModel sideBarViewModel = this.DataContext as SideBarViewModel;
            if (sideBarViewModel.Submissions.ContainsKey(pageID))
            {

                sideBarViewModel.SubmissionPages = sideBarViewModel.Submissions[pageID];
            }
        }

        private void preview_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            CLPPagePreviewView preview = grid.Children[0] as CLPPagePreviewView;
            CLPPageViewModel pageViewModel = preview.DataContext as CLPPageViewModel;
            SideBarViewModel sideBarViewModel = this.DataContext as SideBarViewModel;

            if (pageViewModel.IsSubmission)
            {
                sideBarViewModel.SelectedSubmissionPage = pageViewModel;
            }
            else
            {
                sideBarViewModel.SelectedNotebookPage = pageViewModel;
            }
        }
    }
}
