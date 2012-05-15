using Classroom_Learning_Partner.ViewModels.Workspaces;
using Classroom_Learning_Partner.ViewModels;
using System;
using Classroom_Learning_Partner.ViewModels.Displays;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.Model;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Data;

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
                ((currentToggledButton.Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Transparent);
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
                (((sender as ToggleButton).Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Lavender);
            }
            else
            {
                SubmissionsSideBar.Visibility = Visibility.Collapsed;
                (((sender as ToggleButton).Parent as Grid).Parent as Grid).Background = new SolidColorBrush(Colors.Transparent);
            }

        }

        private void toggle_Click(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = ((sender as ToggleButton).Parent as Grid).Children[1] as ItemsPresenter;
            var vsp = GetVisualChild<VirtualizingStackPanel>(itemsPresenter);

            foreach (ListBoxItem item in vsp.Children)
            {
                if (item != vsp.Children[vsp.Children.Count - 1])
                {
                    if ((bool)(sender as ToggleButton).IsChecked)
                    {
                        //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Visible;
                        //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Collapsed;

                        item.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //((sender as ToggleButton).Parent as Grid).Children[2].Visibility = Visibility.Collapsed;
                        //((sender as ToggleButton).Parent as Grid).Children[1].Visibility = Visibility.Visible;

                        item.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void allItems_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsPresenter = sender as ItemsPresenter;
            var vsp = GetVisualChild<VirtualizingStackPanel>(itemsPresenter);

            foreach (ListBoxItem item in vsp.Children)
            {
                //use snoop, find visual child down to the clppagepreview, set them all invis by default and change below to Visible if == instead of !=
                if (item != vsp.Children[vsp.Children.Count - 1])
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
