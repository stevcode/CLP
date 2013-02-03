using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{

    /// <summary>
    /// Interaction logic for NotebookWorkspaceView.xaml.
    /// </summary>
    public partial class NotebookWorkspaceView : Catel.Windows.Controls.UserControl
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceView"/> class.
        /// </summary>
        public NotebookWorkspaceView()
        {
            InitializeComponent();

            submissionColumnWidth = SubmissionColumn.Width.Value;
            submissionColumnMinWidth = SubmissionColumn.MinWidth;

            SubmissionColumn.MinWidth = 0;
            SubmissionColumn.Width = new GridLength(0);
            SubmissionSplitterColumn.Width = new GridLength(0);
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
                FilterSideBar.Visibility = Visibility.Visible;
                SubmissionPagesSplitter.Visibility = Visibility.Visible;
                submissionsVisibility = Visibility.Visible;
                SubmissionColumn.MinWidth = submissionColumnMinWidth;
                SubmissionColumn.Width = new GridLength(submissionColumnWidth);
                SubmissionSplitterColumn.Width = new GridLength(5);
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
                FilterSideBar.Visibility = Visibility.Collapsed;
                SubmissionPagesSplitter.Visibility = Visibility.Collapsed;
                submissionsVisibility = Visibility.Collapsed;
                submissionColumnWidth = SubmissionColumn.Width.Value;
                submissionColumnMinWidth = SubmissionColumn.MinWidth;

                SubmissionColumn.MinWidth = 0;
                SubmissionColumn.Width = new GridLength(0);
                SubmissionSplitterColumn.Width = new GridLength(0);
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

        [ViewToViewModel("IsSideBarVisible", MappingType = ViewToViewModelMappingType.ViewModelToView)]
        public bool IsSideBarVisible
        {
            get { return (bool)GetValue(IsSideBarVisibleProperty); }
            set { SetValue(IsSideBarVisibleProperty, value);
            ToggleNotebooks(value);
            }
        }

        /// <summary>
        /// Register the IsSideBarVisible property so it is known in the class.
        /// </summary>
        public static readonly DependencyProperty IsSideBarVisibleProperty = DependencyProperty.Register("IsSideBarVisible", typeof(bool), typeof(NotebookWorkspaceView));

        private Visibility submissionsVisibility = Visibility.Collapsed;
        private double submissionColumnWidth = 200;
        private double submissionColumnMinWidth = 150;
        private double notebookPageColumnWidth = 250;
        private double notebookPageColumnMinWidth = 150;

        private void ShowNotebookPagesToggleButton_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)(sender as ToggleButton).IsChecked;

            ToggleNotebooks(isChecked);
            
        }

        private void ToggleNotebooks(bool isChecked)
        {
            if(isChecked)
            {
                NotebookPagesBorder.Visibility = Visibility.Visible;
                NotebookPagesSplitter.Visibility = Visibility.Visible;

                NotebookPageColumn.MinWidth = notebookPageColumnMinWidth;
                NotebookPageColumn.Width = new GridLength(notebookPageColumnWidth);
                NotebookPageSplitterColumn.Width = new GridLength(5);

                if(MainWindow.Ribbon.InstructorVisibility == Visibility.Visible)
                {
                    FilterSideBar.Visibility = submissionsVisibility;
                    SubmissionPagesSplitter.Visibility = submissionsVisibility;

                    if(submissionsVisibility == Visibility.Visible)
                    {
                        SubmissionColumn.MinWidth = submissionColumnMinWidth;
                        SubmissionColumn.Width = new GridLength(submissionColumnWidth);
                        SubmissionSplitterColumn.Width = new GridLength(5);
                    }
                }
            }
            else
            {
                NotebookPagesBorder.Visibility = Visibility.Collapsed;
                NotebookPagesSplitter.Visibility = Visibility.Collapsed;

                notebookPageColumnWidth = NotebookPageColumn.Width.Value;
                notebookPageColumnMinWidth = NotebookPageColumn.MinWidth;

                NotebookPageColumn.MinWidth = 0;
                NotebookPageColumn.Width = new GridLength(0);
                NotebookPageSplitterColumn.Width = new GridLength(0);

                if(MainWindow.Ribbon.InstructorVisibility == Visibility.Visible)
                {
                    FilterSideBar.Visibility = Visibility.Collapsed;
                    SubmissionPagesSplitter.Visibility = Visibility.Collapsed;

                    if(submissionsVisibility == Visibility.Visible)
                    {
                        submissionColumnWidth = SubmissionColumn.Width.Value;
                        submissionColumnMinWidth = SubmissionColumn.MinWidth;

                        SubmissionColumn.MinWidth = 0;
                        SubmissionColumn.Width = new GridLength(0);
                        SubmissionSplitterColumn.Width = new GridLength(0);
                    }
                }
            }
        }
    }
}
