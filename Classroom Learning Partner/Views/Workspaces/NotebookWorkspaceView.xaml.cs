using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Classroom_Learning_Partner.Model; //Steve - No Model in View?
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
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
            if(currentToggledButton != null && currentToggledButton != button)
            {
                currentToggledButton.IsChecked = false;
                ((currentToggledButton.Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Transparent);
            }
            currentToggledButton = button;
            if((bool)currentToggledButton.IsChecked)
            {
                //SubmissionBorder.Visibility = Visibility.Visible;
                SubmissionPagesSplitter.Visibility = Visibility.Visible;
                CLPPage page = (((((sender as ToggleButton).Parent as Grid).Parent as Grid).Children[1] as Border).Child as ContentPresenter).Content as CLPPage;
                string pageID = page.UniqueID;
                var viewModel = this.ViewModel as NotebookWorkspaceViewModel;
                if(viewModel.Notebook.Submissions.ContainsKey(pageID))
                {
                    viewModel.SubmissionPages = viewModel.Notebook.Submissions[pageID];
                }
                (((sender as ToggleButton).Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Lavender);
            }
            else
            {
                SubmissionPagesSplitter.Visibility = Visibility.Collapsed;
                (((sender as ToggleButton).Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Transparent);
            }

        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if(child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if(child != null)
                {
                    break;
                }
            }
            return child;
        }

    }
}
