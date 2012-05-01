using Classroom_Learning_Partner.ViewModels.Workspaces;
using Classroom_Learning_Partner.ViewModels;
using System;
using Classroom_Learning_Partner.ViewModels.Displays;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.Model;
using System.Windows;

namespace Classroom_Learning_Partner.Views.Workspaces
{

    /// <summary>
    /// Interaction logic for NotebookWorkspaceView.xaml.
    /// </summary>
    public partial class NotebookWorkspaceView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceView"/> class.
        /// </summary>
        public NotebookWorkspaceView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(NotebookWorkspaceViewModel);
        }

        private ToggleButton currentToggledButton = null;
        private void ToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            if (currentToggledButton != null && currentToggledButton != button)
            {
                currentToggledButton.IsChecked = false;
            }
            currentToggledButton = button;
            if ((bool)currentToggledButton.IsChecked)
            {
                SubmissionsSideBar.Visibility = Visibility.Visible;
                CLPPage page = (((((sender as ToggleButton).Parent as Grid).Parent as Grid).Children[0] as Border).Child as ContentPresenter).Content as CLPPage;
                string pageID = page.UniqueID;
                var viewModel = this.ViewModel as NotebookWorkspaceViewModel;
                if (viewModel.Notebook.Submissions.ContainsKey(pageID))
                {
                    viewModel.SubmissionPages = viewModel.Notebook.Submissions[pageID];
                }
            }
            else
            {
                SubmissionsSideBar.Visibility = Visibility.Collapsed;
            }
        }
    }
}
